using ECommerce.Api.Features.AI.Chat;
using ECommerce.Api.Features.AI.Indexing;
using ECommerce.Api.Features.AI.VectorStore;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Qdrant.Client;

namespace ECommerce.Api.Features.AI;

public static class AiModule
{
    public static IServiceCollection AddAiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiIndexingOptions>(options =>
        {
            options.EmbeddingModel = configuration["AI:Ollama:EmbeddingModel"] ?? "bge-m3";
            options.BackfillBatchSize = configuration.GetValue("AI:Indexing:BackfillBatchSize", 200);
            options.MaxAttempts = configuration.GetValue("AI:Indexing:MaxAttempts", 5);
        });
        services.Configure<AiQdrantOptions>(configuration.GetSection("AI:Qdrant"));
        services.Configure<AiChatOptions>(options =>
        {
            options.MaxRetrievedSources = configuration.GetValue("AI:Chat:MaxRetrievedSources", 5);
            options.HistoryMessageLimit = configuration.GetValue("AI:Chat:HistoryMessageLimit", 20);
            options.SessionLifetimeDays = configuration.GetValue("AI:Chat:SessionLifetimeDays", 30);
        });

        var ollamaEndpoint = new Uri(
            configuration["AI:Ollama:Endpoint"] ?? "http://localhost:11434",
            UriKind.Absolute);
        var embeddingModel = configuration["AI:Ollama:EmbeddingModel"] ?? "bge-m3";
        services.AddOllamaEmbeddingGenerator(embeddingModel, ollamaEndpoint);
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(provider =>
            provider.GetRequiredKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(serviceKey: null));

        services.AddSingleton(provider =>
        {
            var qdrantOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiQdrantOptions>>().Value;
            return new QdrantClient(
                new Uri(qdrantOptions.Endpoint, UriKind.Absolute),
                qdrantOptions.ApiKey,
                grpcTimeout: TimeSpan.FromSeconds(10),
                loggerFactory: provider.GetRequiredService<ILoggerFactory>());
        });
        services.AddTransient(provider => new Kernel(provider));

        var geminiApiKey = configuration["Gemini:ApiKey"];
        if (!string.IsNullOrWhiteSpace(geminiApiKey))
        {
#pragma warning disable SKEXP0070
            services.AddGoogleAIGeminiChatCompletion(
                modelId: configuration["Gemini:Model"] ?? "gemini-3.8-flash",
                apiKey: geminiApiKey,
                apiVersion: GoogleAIVersion.V1);
#pragma warning restore SKEXP0070
        }

        services.AddScoped<IAiIndexQueue, AiIndexQueue>();
        services.AddScoped<IAiIndexAdminService, AiIndexAdminService>();
        services.AddScoped<IAiVectorStore, QdrantAiVectorStore>();
        services.AddScoped<IAiProductIndexer, AiProductIndexer>();
        services.AddScoped<IAiIndexJobProcessor, AiIndexJobProcessor>();
        services.AddScoped<IGeminiAnswerGenerator, GeminiAnswerGenerator>();
        services.AddScoped<IChatService, AiChatService>();
        services.AddHostedService<AiIndexWorker>();

        return services;
    }
}
