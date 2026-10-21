using System.Security.Cryptography;
using System.Text;
using ECommerce.Api.Data;
using ECommerce.Api.Features.AI.Contracts;
using ECommerce.Api.Features.AI.Entities;
using ECommerce.Api.Features.AI.Indexing;
using ECommerce.Api.Features.AI.VectorStore;
using Microsoft.Extensions.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerce.Api.Features.AI.Chat;

public sealed class AiChatService(
    AppDbContext dbContext,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IAiVectorStore vectorStore,
    IGeminiAnswerGenerator answerGenerator,
    IOptions<AiChatOptions> chatOptions,
    IConfiguration configuration,
    ILogger<AiChatService> logger) : IChatService
{
    private readonly AiChatOptions _chatOptions = chatOptions.Value;
    private readonly string _geminiModel = configuration["Gemini:Model"] ?? "gemini-3.8-flash";

    public async Task<ChatResponseDto> ChatAsync(
        ChatRequestDto request,
        int? userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 2000)
        {
            throw new ArgumentException("Message length must be between 1 and 2000 characters.", nameof(request));
        }

        var sessionResolution = await ResolveSessionAsync(request, userId, cancellationToken);
        var session = sessionResolution.Session;
        var history = await dbContext.AiChatMessages
            .AsNoTracking()
            .Where(message => message.SessionID == session.SessionID)
            .OrderByDescending(message => message.CreatedAt)
            .ThenByDescending(message => message.MessageID)
            .Take(_chatOptions.HistoryMessageLimit)
            .OrderBy(message => message.CreatedAt)
            .ThenBy(message => message.MessageID)
            .ToListAsync(cancellationToken);

        var retrievedProducts = await RetrieveProductsAsync(request.Message, cancellationToken);
        var sourceDtos = retrievedProducts
            .Select(product => new ChatSourceDto(product.ProductID, product.ProductName, product.Score))
            .ToList();
        var productDtos = retrievedProducts
            .Select(product => new RecommendedProductDto(
                product.ProductID,
                product.ProductName,
                product.Price,
                product.ImageUrl,
                null))
            .ToList();

        string reply;
        string answerType;
        if (retrievedProducts.Count == 0)
        {
            reply = "Mình chưa tìm thấy thông tin phù hợp trong danh mục hiện tại. Bạn có thể cho mình biết tên sản phẩm, thương hiệu hoặc nhu cầu cụ thể hơn không?";
            answerType = "no_information";
        }
        else
        {
            var context = BuildRetrievedContext(retrievedProducts);
            try
            {
                var generated = await answerGenerator.GenerateAsync(
                    history,
                    request.Message.Trim(),
                    context,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(generated))
                {
                    reply = generated;
                    answerType = "grounded_answer";
                }
                else
                {
                    reply = "Mình đã tìm thấy sản phẩm phù hợp bên dưới, nhưng Gemini chưa được cấu hình để tổng hợp câu trả lời.";
                    answerType = "generation_unavailable";
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    "Gemini answer generation failed for chat session {SessionId} ({ErrorType}).",
                    session.SessionID,
                    exception.GetType().Name);
                reply = "Mình đã tìm thấy một số sản phẩm phù hợp bên dưới nhưng hiện chưa thể tổng hợp câu trả lời. Bạn có thể xem thông tin sản phẩm để biết giá và tình trạng còn hàng mới nhất.";
                answerType = "generation_error";
            }
        }

        var userMessage = new AiChatMessage
        {
            SessionID = session.SessionID,
            Role = "user",
            Content = request.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        var assistantMessage = new AiChatMessage
        {
            SessionID = session.SessionID,
            Role = "assistant",
            Content = reply,
            AnswerType = answerType,
            ModelName = retrievedProducts.Count > 0 ? _geminiModel : null,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.AiChatMessages.AddRange(userMessage, assistantMessage);
        foreach (var (product, index) in retrievedProducts.Select((product, index) => (product, index)))
        {
            dbContext.AiMessageSources.Add(new AiMessageSource
            {
                Message = assistantMessage,
                ChunkID = product.ChunkID,
                ProductID = product.ProductID,
                Rank = index + 1,
                Score = product.Score,
                SourceVersion = product.ContentHash,
                CreatedAt = DateTime.UtcNow
            });
        }

        session.UpdatedAt = DateTime.UtcNow;
        session.ExpiresAt = DateTime.UtcNow.AddDays(_chatOptions.SessionLifetimeDays);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ChatResponseDto(
            reply,
            productDtos.Count == 0 ? null : productDtos,
            session.SessionID,
            sessionResolution.GuestToken,
            sourceDtos);
    }

    private async Task<SessionResolution> ResolveSessionAsync(
        ChatRequestDto request,
        int? userId,
        CancellationToken cancellationToken)
    {
        if (request.SessionId is null)
        {
            return await CreateSessionAsync(request, userId, importHistory: true, cancellationToken);
        }

        var existing = await dbContext.AiChatSessions
            .FirstOrDefaultAsync(session => session.SessionID == request.SessionId.Value, cancellationToken);

        if (existing is null || existing.ExpiresAt <= DateTime.UtcNow)
        {
            return await CreateSessionAsync(request, userId, importHistory: false, cancellationToken);
        }

        if (userId is not null && existing.UserID == userId)
        {
            return new SessionResolution(existing, null);
        }

        if (existing.UserID is null && userId is not null && IsValidGuestToken(existing, request.SessionToken))
        {
            existing.UserID = userId;
            existing.GuestTokenHash = null;
            return new SessionResolution(existing, null);
        }

        if (userId is null && existing.UserID is null && IsValidGuestToken(existing, request.SessionToken))
        {
            return new SessionResolution(existing, request.SessionToken);
        }

        // A stale or foreign session starts a new isolated conversation; the old session is never read.
        return await CreateSessionAsync(request, userId, importHistory: false, cancellationToken);
    }

    private async Task<SessionResolution> CreateSessionAsync(
        ChatRequestDto request,
        int? userId,
        bool importHistory,
        CancellationToken cancellationToken)
    {
        var guestToken = userId is null ? CreateGuestToken() : null;
        var session = new AiChatSession
        {
            SessionID = Guid.NewGuid(),
            UserID = userId,
            GuestTokenHash = guestToken is null ? null : HashGuestToken(guestToken),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_chatOptions.SessionLifetimeDays)
        };
        dbContext.AiChatSessions.Add(session);

        if (importHistory)
        {
            foreach (var message in request.History?.TakeLast(_chatOptions.HistoryMessageLimit) ?? [])
            {
                if ((message.Role is "user" or "assistant") && !string.IsNullOrWhiteSpace(message.Content))
                {
                    session.Messages.Add(new AiChatMessage
                    {
                        Role = message.Role,
                        Content = message.Content.Trim(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new SessionResolution(session, guestToken);
    }

    private async Task<List<RetrievedProduct>> RetrieveProductsAsync(
        string question,
        CancellationToken cancellationToken)
    {
        try
        {
            var queryEmbedding = await embeddingGenerator.GenerateAsync(
                question,
                cancellationToken: cancellationToken);
            var matches = await vectorStore.SearchAsync(
                queryEmbedding.Vector.ToArray(),
                Math.Max(_chatOptions.MaxRetrievedSources * 3, _chatOptions.MaxRetrievedSources),
                cancellationToken);

            if (matches.Count == 0)
            {
                return [];
            }

            var chunkIds = matches.Select(match => match.ChunkID).Distinct().ToArray();
            var chunks = await dbContext.AiKnowledgeChunks
                .Include(chunk => chunk.Product)
                    .ThenInclude(product => product.Category)
                .Include(chunk => chunk.Product)
                    .ThenInclude(product => product.Brand)
                .Include(chunk => chunk.Product)
                    .ThenInclude(product => product.Images)
                .AsNoTracking()
                .Where(chunk => chunkIds.Contains(chunk.ChunkID) && chunk.Status == AiIndexStatuses.Indexed)
                .ToDictionaryAsync(chunk => chunk.ChunkID, cancellationToken);

            var retrieved = new List<RetrievedProduct>();
            var seenProducts = new HashSet<int>();
            foreach (var match in matches)
            {
                if (!chunks.TryGetValue(match.ChunkID, out var chunk) ||
                    chunk.ProductID != match.ProductID ||
                    chunk.ContentHash != match.ContentHash ||
                    !chunk.Product.IsActive ||
                    !seenProducts.Add(chunk.ProductID))
                {
                    continue;
                }

                var currentDocument = AiProductDocumentBuilder.Build(chunk.Product);
                if (!string.Equals(currentDocument.Hash, chunk.ContentHash, StringComparison.Ordinal))
                {
                    continue;
                }

                retrieved.Add(new RetrievedProduct(
                    chunk.ChunkID,
                    chunk.ProductID,
                    chunk.ContentHash,
                    match.Score,
                    chunk.Product.ProductName,
                    chunk.Product.SKU,
                    chunk.Product.Brand.BrandName,
                    chunk.Product.Category.CategoryName,
                    chunk.Product.Price,
                    chunk.Product.StockQuantity,
                    chunk.Product.Description,
                    chunk.Product.Specifications,
                    chunk.Product.Images.FirstOrDefault(image => image.IsPrimary)?.ImageURL));

                if (retrieved.Count == _chatOptions.MaxRetrievedSources)
                {
                    break;
                }
            }

            return retrieved;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("RAG retrieval is unavailable ({ErrorType}).", exception.GetType().Name);
            return [];
        }
    }

    private static string BuildRetrievedContext(IEnumerable<RetrievedProduct> products)
    {
        var context = new StringBuilder();
        foreach (var product in products)
        {
            context.AppendLine($"Sản phẩm {product.ProductID}: {product.ProductName}")
                .AppendLine($"SKU: {product.SKU}; thương hiệu: {product.BrandName}; danh mục: {product.CategoryName}")
                .AppendLine($"Giá hiện tại: {product.Price:N0} VNĐ; số lượng tồn kho hiện tại: {product.StockQuantity}")
                .AppendLine($"Mô tả: {Limit(product.Description, 1000)}")
                .AppendLine($"Thông số: {Limit(product.Specifications, 1500)}")
                .AppendLine();
        }

        return context.ToString();
    }

    private static string Limit(string? value, int length) =>
        string.IsNullOrWhiteSpace(value) ? "Chưa có dữ liệu." : value[..Math.Min(value.Length, length)];

    private static string CreateGuestToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return token.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string HashGuestToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static bool IsValidGuestToken(AiChatSession session, string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(session.GuestTokenHash))
        {
            return false;
        }

        var expected = Encoding.ASCII.GetBytes(session.GuestTokenHash);
        var actual = Encoding.ASCII.GetBytes(HashGuestToken(token));
        return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    private sealed record SessionResolution(AiChatSession Session, string? GuestToken);

    private sealed record RetrievedProduct(
        Guid ChunkID,
        int ProductID,
        string ContentHash,
        double Score,
        string ProductName,
        string SKU,
        string BrandName,
        string CategoryName,
        decimal Price,
        int StockQuantity,
        string? Description,
        string? Specifications,
        string? ImageUrl);
}
