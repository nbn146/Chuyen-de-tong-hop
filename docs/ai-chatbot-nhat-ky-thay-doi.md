# Nhật ký thay đổi — Chatbot RAG (nhánh `feature/ai-chatbot-rag`)

> **Phạm vi:** các chỉnh sửa thực hiện ngày 07/11/2026, sau khi review nhánh chatbot RAG.
> **Ứng với plan v2:** Giai đoạn 0 (sửa lỗi chặn và bảo mật), cộng với sửa migration và tách hạ tầng AI theo yêu cầu.
> **Trạng thái:** chưa commit. "Trước" là trạng thái của working tree trước khi sửa; phần lớn code AI nằm trong `Features/AI/`, thư mục này chưa được git theo dõi.

## Tóm tắt

| # | Vấn đề | Mức độ | File chính |
|---|---|---|---|
| 1 | `POST /api/chat` treo vĩnh viễn | Chặn | `Features/AI/AiModule.cs` |
| 2 | Mọi request chat trả HTTP 500 | Chặn | `Features/AI/Contracts/ChatDtos.cs` |
| 3 | Chat hơn khoảng 10 lượt thì frontend chỉ còn báo lỗi (HTTP 400) | Cao | `frontend/src/components/ChatBot.tsx` |
| 4 | Token đăng nhập hết hạn thì mỗi tin nhắn lại mở session mới | Trung bình | `frontend/src/components/ChatBot.tsx` |
| 5 | Gemini API key nằm trong file được commit | Bảo mật | `appsettings.json` |
| 6 | Worker index chạy cả khi test/CI | Thấp | `AiIndexWorker.cs`, `appsettings.Testing.json` |
| 7 | `AIMessageSources` không ghi được nguồn cho sản phẩm không có chunk vector | Thiết kế DB | Entity, configuration, migration |
| 8 | Qdrant/Ollama dùng chung container và cổng với dự án khác | Hạ tầng | `docker-compose.ai.yml`, cấu hình cổng |

Không đổi route hay JSON của `/api/chat`. Thay đổi về database chỉ nằm trong bảng `AIMessageSources`, thuộc migration AI chưa được áp ở đâu khác.

---

## 1. `POST /api/chat` treo vĩnh viễn

**File:** `backend/src/ECommerce.Api/Features/AI/AiModule.cs`

**Nguyên nhân:** `AddOllamaEmbeddingGenerator` với service key `null` đã đăng ký `IEmbeddingGenerator` như một service thường. Dòng đăng ký thêm ngay sau đó lại resolve chính nó, nên việc tạo `ChatController` → `AiChatService` không bao giờ hoàn tất. Đã kiểm chứng bằng log `Executing controller factory`: request treo hơn 150 giây.

```diff
-        services.AddOllamaEmbeddingGenerator(embeddingModel, ollamaEndpoint);
-        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(provider =>
-            provider.GetRequiredKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(serviceKey: null));
+        // Registers IEmbeddingGenerator<string, Embedding<float>> without a service key, so it is
+        // resolvable directly. Do not add a forwarding registration: it would resolve itself and hang.
+        services.AddOllamaEmbeddingGenerator(embeddingModel, ollamaEndpoint);
```

## 2. Mọi request chat trả HTTP 500

**File:** `backend/src/ECommerce.Api/Features/AI/Contracts/ChatDtos.cs`

**Nguyên nhân:** với record có primary constructor, ASP.NET Core MVC không chấp nhận attribute validate đặt trên property (`[property: …]`). Mỗi request ném lỗi `InvalidOperationException: Record type 'ChatRequestDto' has validation metadata defined on property…`.

```diff
 public record ChatRequestDto(
-    [property: Required, StringLength(2000, MinimumLength = 1)] string Message,
-    [property: MaxLength(20)] List<ChatMessageDto>? History,
+    [Required, StringLength(2000, MinimumLength = 1)] string Message,
+    [MaxLength(20)] List<ChatMessageDto>? History,
     Guid? SessionId = null,
-    [property: StringLength(128)] string? SessionToken = null);
+    [StringLength(128)] string? SessionToken = null);

 public record ChatMessageDto(
-    [property: Required, StringLength(16)] string Role,
-    [property: Required, StringLength(2000)] string Content);
+    [Required, StringLength(16)] string Role,
+    [Required, StringLength(2000)] string Content);
```

## 3–4. Frontend: lỗi 400 khi chat dài và mất session khi token hết hạn

**File:** `frontend/src/components/ChatBot.tsx`

**Nguyên nhân:**
- Widget gửi **toàn bộ** hội thoại mỗi lần, trong khi API giới hạn 20 tin nhắn và mỗi tin 2000 ký tự. Backend lại chỉ dùng `history` khi tạo session mới.
- Khi còn JWT trong `localStorage`, widget bỏ qua guest token mà server trả về. Nếu JWT đã hết hạn, server coi người dùng là khách, nên lượt sau không xác thực được session và lại mở session mới.

```diff
+const MAX_HISTORY_MESSAGES = 20;
+const MAX_MESSAGE_LENGTH = 2000;
 ...
-    const history = messages
-      .filter((m) => !m.isLoading)
-      .slice(1) // skip welcome message
-      .map((m) => ({ role: m.role, content: m.content }));
+    const sessionForRequest = chatSession ?? undefined;
+
+    // The server keeps the canonical history once a session exists, so the local history is only
+    // sent to seed a new session, trimmed to the API limits (20 messages, 2000 characters each).
+    const history = sessionForRequest
+      ? undefined
+      : messages
+          .filter((m) => !m.isLoading)
+          .slice(1) // skip welcome message
+          .slice(-MAX_HISTORY_MESSAGES)
+          .map((m) => ({ role: m.role, content: m.content.slice(0, MAX_MESSAGE_LENGTH) }));

     try {
-      const hasUserToken = Boolean(localStorage.getItem('token'));
-      const sessionForRequest = hasUserToken || chatSession?.sessionToken ? chatSession ?? undefined : undefined;
       const response = await chatService.sendMessage(trimmed, history, sessionForRequest);
+      // The server returns a token for guest sessions (including when an expired login is treated
+      // as a guest) and none for sessions bound to a signed-in user.
       const nextSession: ChatSession = {
         sessionId: response.sessionId,
-        sessionToken: hasUserToken ? undefined : response.sessionToken ?? sessionForRequest?.sessionToken,
+        sessionToken: response.sessionToken ?? undefined,
       };
 ...
-    } catch (err: any) {
+    } catch {
```

Frontend vẫn tự xoá thẻ `[ID:x]` khi hiển thị như trước; phần đó không đổi.

## 5. Gemini API key nằm trong file được commit

**File:** `backend/src/ECommerce.Api/appsettings.json`

```diff
     "Gemini": {
-        "Model": "gemini-3.8-flash",
-        "ApiKey": "AQ.Ab8…"
+        "Model": "gemini-3.5-flash"
     },
```

- Key được chuyển sang .NET User Secrets của máy local bằng lệnh `dotnet user-secrets set "Gemini:ApiKey" …`, nên chatbot trên máy vẫn chạy.
- `Gemini:Model` = `gemini-3.5-flash` là **thay đổi do người dùng thực hiện**, vì lúc kiểm tra `gemini-3.8-flash` bị Google trả 503 quá tải.
- ⚠️ **Việc còn tồn đọng:** phải thu hồi cả hai key đã lộ: `AIzaSy…` (trong lịch sử git, commit `b42fd7c`) và `AQ.Ab8…`. Sau đó đặt key mới bằng User Secrets.

## 6. Tắt worker index khi test/CI

Trước đây, khi chạy test, worker vẫn kết nối SQL Server, Ollama và Qdrant.

**`Features/AI/Indexing/AiProductIndexer.cs`** (class `AiIndexingOptions`):

```diff
     public int MaxAttempts { get; set; } = 5;
+    public bool Enabled { get; set; } = true;
```

**`Features/AI/AiModule.cs`**:

```diff
             options.MaxAttempts = configuration.GetValue("AI:Indexing:MaxAttempts", 5);
+            options.Enabled = configuration.GetValue("AI:Indexing:Enabled", true);
```

**`Features/AI/Indexing/AiIndexWorker.cs`**:

```diff
+using Microsoft.Extensions.Options;
+
 public sealed class AiIndexWorker(
     IServiceScopeFactory scopeFactory,
+    IOptions<AiIndexingOptions> options,
     ILogger<AiIndexWorker> logger) : BackgroundService
 {
     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
     {
+        if (!options.Value.Enabled)
+        {
+            logger.LogInformation("AI indexing worker is disabled by configuration (AI:Indexing:Enabled).");
+            return;
+        }
```

**File mới `backend/src/ECommerce.Api/appsettings.Testing.json`**:

```json
{
  "AI": {
    "Indexing": {
      "Enabled": false
    }
  }
}
```

## 7. `AIMessageSources`: ghi được nguồn không có chunk vector

**Lý do:** plan v2 cần lưu mọi sản phẩm đã gợi ý để hiểu câu hỏi nối tiếp ("rẻ hơn", "cái thứ 2"). Có những sản phẩm không có chunk vector: kết quả tìm bằng SQL khi Ollama/Qdrant lỗi, sản phẩm chưa index xong, hoặc sản phẩm lấy thẳng từ catalog khi so sánh. Trước đây cột `ChunkID` bắt buộc nên không ghi được các trường hợp này.

**`Features/AI/Entities/AiMessageSource.cs`**:

```diff
 public sealed class AiMessageSource
 {
     public long SourceID { get; set; }
     public long MessageID { get; set; }
-    public Guid ChunkID { get; set; }
+    public string SourceType { get; set; } = AiSourceTypes.Vector;
+    public Guid? ChunkID { get; set; }          // chỉ có với nguồn vector
     public int ProductID { get; set; }
     public int Rank { get; set; }
     public double Score { get; set; }
-    public string SourceVersion { get; set; } = null!;
+    public string? SourceVersion { get; set; }  // hash chunk, chỉ có với nguồn vector
     public DateTime CreatedAt { get; set; }

     public AiChatMessage Message { get; set; } = null!;
-    public AiKnowledgeChunk Chunk { get; set; } = null!;
+    public AiKnowledgeChunk? Chunk { get; set; }
 }
+
+public static class AiSourceTypes
+{
+    public const string Vector = "vector";   // tìm qua vector của chunk đã index
+    public const string Lexical = "lexical"; // tìm bằng từ khoá/bộ lọc SQL
+    public const string Catalog = "catalog"; // lấy thẳng từ catalog, ví dụ khi so sánh
+}
```

**`Features/AI/Data/Configurations/AiMessageSourceConfiguration.cs`**:

```diff
-        builder.ToTable("AIMessageSources");
+        builder.ToTable("AIMessageSources", table =>
+        {
+            table.HasCheckConstraint(
+                "CK_AIMessageSources_SourceType",
+                "[SourceType] IN ('vector', 'lexical', 'catalog')");
+            table.HasCheckConstraint(
+                "CK_AIMessageSources_Chunk",
+                "([SourceType] = 'vector' AND [ChunkID] IS NOT NULL AND [SourceVersion] IS NOT NULL) OR " +
+                "([SourceType] <> 'vector' AND [ChunkID] IS NULL AND [SourceVersion] IS NULL)");
+        });
         builder.HasKey(source => source.SourceID);
         builder.Property(source => source.SourceID).UseIdentityColumn();
-        builder.Property(source => source.SourceVersion).IsRequired().HasMaxLength(64).IsUnicode(false);
+        builder.Property(source => source.SourceType).IsRequired().HasMaxLength(20).IsUnicode(false);
+        builder.Property(source => source.SourceVersion).HasMaxLength(64).IsUnicode(false);
```

**`Features/AI/Chat/AiChatService.cs`** (chỗ ghi nguồn hiện tại):

```diff
             dbContext.AiMessageSources.Add(new AiMessageSource
             {
                 Message = assistantMessage,
+                SourceType = AiSourceTypes.Vector,
                 ChunkID = product.ChunkID,
```

**Migration** (`backend/src/ECommerce.Api/Migrations/`):

| Trước | Sau |
|---|---|
| `20261021155203_AddAIChatRagCore.cs` + `.Designer.cs` | Đã xoá |
| — | `20261106180022_AddAIChatRagCore.cs` + `.Designer.cs` (tạo lại) |
| `AppDbContextModelSnapshot.cs` | Tạo lại theo model mới |

So với migration cũ, chỉ bảng `AIMessageSources` khác:

```diff
+    SourceType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
-    ChunkID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
+    ChunkID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
-    SourceVersion = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
+    SourceVersion = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
+    table.CheckConstraint("CK_AIMessageSources_Chunk", "…");
+    table.CheckConstraint("CK_AIMessageSources_SourceType", "[SourceType] IN ('vector', 'lexical', 'catalog')");
```

Bốn bảng còn lại (`AIChatSessions`, `AIChatMessages`, `AIKnowledgeChunks`, `AIIndexJobs`) giữ nguyên. Migration chỉ tạo bảng và index mới, không `ALTER`, `DROP`, `DELETE` hay `UPDATE` bảng có sẵn.

**Đã kiểm chứng trước khi áp:**
- `dotnet ef migrations has-pending-model-changes` báo không còn thay đổi nào chưa có trong migration.
- Áp vào một database tạm thành công; database tạm đã xoá sau đó.
- 7 trường hợp thử ràng buộc CHECK cho đúng kết quả:
  - được nhận: nguồn vector có chunk, nguồn lexical không chunk, nguồn catalog không chunk;
  - bị từ chối: nguồn vector thiếu chunk, nguồn lexical có chunk, `SourceType` không hợp lệ, trùng `Rank` trong cùng một tin nhắn.
- Migration đã được áp vào `ElectronicsStoreDB`.

## 8. Tách Qdrant/Ollama khỏi dự án khác

**Lý do:** trước đây cổng mặc định (6333/6334/11434) trùng với dự án `hocmaynangcao`, và Ollama mượn container của dự án đó.

**`docker-compose.ai.yml`**: thay đổi chính:

```diff
+# AI services for the ECommerce chatbot only. Host ports differ from the Qdrant/Ollama defaults
+# (6333/6334/11434) so these containers can run alongside other projects on the same machine.
 services:
   qdrant:
     image: qdrant/qdrant:v1.19.2
     ports:
-      - "6333:6333"
-      - "6334:6334"
+      - "127.0.0.1:6433:6333"
+      - "127.0.0.1:6434:6334"
     volumes:
       - qdrant_storage:/qdrant/storage
+
+  ollama:
+    image: ollama/ollama:0.40.3
+    restart: unless-stopped
+    ports:
+      - "127.0.0.1:11435:11434"
+    volumes:
+      - ollama_models:/root/.ollama

 volumes:
   qdrant_storage:
+  ollama_models:
```

**Endpoint mặc định đổi theo cổng mới:**

| File | Trước | Sau |
|---|---|---|
| `appsettings.json` → `AI:Ollama:Endpoint` | `http://localhost:11434` | `http://localhost:11435` |
| `appsettings.json` → `AI:Qdrant:Endpoint` | `http://localhost:6334` | `http://localhost:6434` |
| `Features/AI/AiModule.cs` (giá trị mặc định khi thiếu cấu hình) | `http://localhost:11434` | `http://localhost:11435` |
| `Features/AI/VectorStore/QdrantAiVectorStore.cs` (`AiQdrantOptions.Endpoint`) | `http://localhost:6334` | `http://localhost:6434` |

**Hạ tầng trên máy local** (không nằm trong code):
- Container `quan_ly_du_an-qdrant-1` (volume `quan_ly_du_an_qdrant_storage`, giữ nguyên dữ liệu vector) và `quan_ly_du_an-ollama-1` (volume `quan_ly_du_an_ollama_models`).
- Model `bge-m3` tải vào volume riêng, digest `790764642607` giống bản cũ, nên vector đã index vẫn dùng được.
- Container `hocmaynangcao-ollama-1` đã được tắt lại như ban đầu.

---

## Test mới

**`backend/tests/ECommerce.Api.Tests/ChatEndpointTests.cs`** (6 test):

| Test | Bảo vệ cho |
|---|---|
| `EmbeddingGeneratorAndChatServiceResolveWithoutHanging` | Mục 1: resolve DI phải xong trong 10 giây |
| `ValidRequestReachesChatServiceAndReturnsOk` | Mục 2: request hợp lệ trả 200. Đã thử đưa lỗi `[property:]` quay lại thì test báo 500 |
| `BlankMessageReturnsBadRequest` (2 trường hợp) | Message rỗng hoặc chỉ có khoảng trắng → 400 |
| `MessageLongerThan2000CharactersReturnsBadRequest` | Message dài hơn 2000 ký tự → 400 |
| `HistoryLongerThan20MessagesReturnsBadRequest` | History hơn 20 tin → 400 |

**`frontend/src/components/ChatBot.test.tsx`** (2 test):
- Chỉ gửi `history` ở lượt đầu; các lượt sau gửi `sessionId` và `sessionToken` mà server trả về.
- Vẫn giữ guest token của server khi `localStorage` còn JWT (trường hợp token đăng nhập đã hết hạn).

## Tài liệu

`docs/ai-chatbot.md` được cập nhật:
- Lệnh khởi chạy `docker compose -f docker-compose.ai.yml up -d` và lệnh tải `bge-m3`.
- Bảng cổng 6433/6434/11435.
- Cờ `AI:Indexing:Enabled`.

## Kết quả kiểm tra

| Lệnh | Kết quả |
|---|---|
| `dotnet build backend/ECommerce.slnx -c Release` | Thành công, không lỗi |
| `dotnet test backend/ECommerce.slnx -c Release` | 10/10 pass |
| `npm --prefix frontend test` | 4/4 pass |
| `npm --prefix frontend run build` | Thành công |
| Chạy thử API với cấu hình mới | `/api/chat` trả 200; câu "tai nghe chống ồn" ra 5 tai nghe JBL đúng loại |

## Đợt 2 — Timeout và tìm kiếm có bộ lọc (Bước 1 + Bước 2)

Mục tiêu: câu như "laptop dưới 20 triệu RAM 16GB" phải trả **đúng** sản phẩm thỏa điều kiện, và request không còn bị treo lâu khi Gemini/Ollama lỗi. Hợp đồng `POST /api/chat` và schema DB **không đổi**.

### Luồng mới

```
AiChatService
  → CatalogVocabularyProvider (tên danh mục/hãng từ SQL, cache 5 phút)
  → ChatQueryParser (tất định, không gọi LLM): giá, danh mục, hãng, RAM, bộ nhớ, Hz, còn hàng, từ khóa còn lại
  → ProductRetrievalService
      1. SQL: sản phẩm active + danh mục active + lọc giá/danh mục/hãng/tồn kho
      2. ProductSpecParser: lọc RAM/bộ nhớ/Hz từ JSON Specifications
      3. Xếp hạng lexical (từ khóa khớp tên/SKU/hãng/danh mục ×3, thông số/mô tả ×1)
      4. Xếp hạng vector (Ollama bge-m3 → Qdrant top 100, chỉ nhận chunk đúng hash + đúng model)
      5. Gộp bằng RRF (k = 60); hòa điểm xếp theo ProductID
      Ollama/Qdrant lỗi hoặc quá 10s → chế độ lexical_only
  → Không có kết quả → câu mẫu nêu các bộ lọc đã áp dụng, KHÔNG gọi Gemini
  → Gemini (1 lần, tối đa 20s, ThinkingLevel=low) → lỗi/quá giờ → câu mẫu + thẻ sản phẩm
```

### File mới — `Features/AI/Search/`

| File | Nội dung |
|---|---|
| `TextNormalizer.cs` | Viết thường, bỏ dấu tiếng Việt (`đ` → `d`), tách token |
| `ProductSpecParser.cs` | Đọc `RAM`, `Ổ cứng`/`Bộ nhớ trong`, `Tần số quét`/`Màn hình` từ JSON → GB/Hz; không đọc được thì `null` (không thỏa bộ lọc) |
| `ProductSearchModels.cs` | `ProductSearchCriteria`, `ProductCandidate`, `VectorHit`, `RankedProduct`, `ProductSearchResult`, `ProductSearchModes` |
| `ProductRanker.cs` | Hàm thuần: lọc thông số, xếp hạng lexical, RRF. Có bộ lọc cứng thì sản phẩm không được ranker nào chấm điểm vẫn được trả (xếp sau, còn hàng trước) |
| `ProductRetrievalService.cs` | Truy vấn SQL + vector search có timeout và kiểm tra hash/model; fallback `lexical_only` |
| `CatalogVocabulary.cs` | Từ đồng nghĩa danh mục ("lap", "điện thoại", "tai nghe", "máy tính bảng"…), từ "yếu" ("màn hình", "sạc", "cáp") chỉ dùng khi không có danh mục khác; `wd` → Western Digital |
| `ChatQueryParser.cs` | Parser giá: "dưới/không quá/tối đa/đổ lại/ngân sách X", "trên/từ X trở lên", "từ X đến Y", "X-Ytr", "tầm/khoảng X" và "X triệu" trơn (= X ± 20%); đơn vị `triệu/tr/củ`, `k/nghìn`, số đủ `20.000.000đ`; "dưới 20" hiểu là 20 triệu. Không nhầm `16GB`, `144Hz`, `15.6 inch`, `2k`, `i5-1335U` thành giá |

### File sửa

| File | Thay đổi |
|---|---|
| `Chat/AiChatService.cs` | Bỏ `RetrieveProductsAsync` (chỉ vector) và phụ thuộc trực tiếp vào embedding/Qdrant; dùng parser + `IProductRetrievalService`; ghi log `SearchMode`; câu mẫu khi không có kết quả nêu bộ lọc; context gửi Gemini có dòng "Bộ lọc khách yêu cầu"; `AIMessageSources.SourceType` = `vector` (kèm ChunkID/hash), `lexical` hoặc `catalog`; `ModelName` chỉ ghi khi Gemini thực sự trả lời |
| `Chat/GeminiAnswerGenerator.cs` | Timeout 20s bằng linked `CancellationTokenSource`; `Temperature 0.3`, `MaxTokens 1024`, `ThinkingConfig.ThinkingLevel` lấy từ `Gemini:ThinkingLevel`; thêm quy tắc prompt về bộ lọc |
| `Chat/GeminiAnswerGenerator.cs` (`AiChatOptions`) | Thêm `GenerationTimeoutSeconds = 20`, `EmbeddingTimeoutSeconds = 10` |
| `Indexing/AiProductIndexer.cs` | Chỉ bỏ qua index khi hash **và** `EmbeddingModel` đều khớp |
| `Indexing/AiIndexJobProcessor.cs` | Backfill thêm sản phẩm có chunk của model embedding khác mà chưa được xếp job từ lần index đó → đổi model sẽ tự index lại |
| `Indexing/AiProductDocumentBuilder.cs` | Thêm overload `Build(tên, SKU, hãng, danh mục, mô tả, thông số)` để kiểm tra hash từ dòng SQL đã project |
| `AiModule.cs` | Đọc 2 timeout; đăng ký `AddMemoryCache`, `ICatalogVocabularyProvider`, `IProductRetrievalService` |
| `appsettings.json` | `Gemini:ThinkingLevel = "low"`; `AI:Chat:GenerationTimeoutSeconds = 20`, `AI:Chat:EmbeddingTimeoutSeconds = 10` |

### Test mới

- `ChatQueryParserTests.cs` (48 trường hợp): giá (16 cách nói), 6 câu không được nhầm thành giá, RAM/bộ nhớ (8), danh mục (9), hãng (4), "lga1700" không thành hãng LG, Hz, từ khóa còn lại, câu không có bộ lọc.
- `ProductRankingTests.cs` (15 trường hợp): đọc JSON thông số thật, JSON hỏng/rỗng; lọc RAM loại sản phẩm thiếu thông số; RRF ưu tiên sản phẩm có cả 2 ranker; không bộ lọc thì không trả sản phẩm không liên quan; có bộ lọc thì vẫn trả; loại trừ ID; hòa điểm xếp theo ID.

### Kết quả kiểm tra đợt 2

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build backend/ECommerce.slnx -c Release` | Thành công |
| `dotnet test backend/ECommerce.slnx -c Release` | 73/73 pass (10 cũ + 63 mới) |
| Chạy thật, Docker tắt (`lexical_only`) | "laptop dưới 20 triệu RAM 16GB" → 5 laptop 14,5–20 triệu, đều 16GB; "laptop dell dưới 5 triệu" → câu mẫu nêu bộ lọc trong 0,1s, không gọi Gemini |
| Chạy thật, Docker bật (`hybrid`) | "màn hình 144hz tầm 5 triệu" → MSI G274F 180Hz 4,3 triệu; Gemini nhắc lại đúng điều kiện "từ 4 đến 6 triệu" |
| `AIMessageSources` | 26 nguồn `vector` (đủ ChunkID), 10 `lexical`, 11 `catalog` — đều qua CHECK constraint |
| Chỉ mục | 510/510 chunk `Indexed`, model `bge-m3` |

### Hạn chế còn lại của đợt 2

- **Gemini `gemini-3.5-flash` chậm:** 15–27s mỗi câu kể cả `ThinkingLevel=low`; khoảng 1/3 câu vượt 20s và rơi về câu mẫu. `ThinkingLevel=none` bị API từ chối (lỗi 400). Lựa chọn: tăng `AI:Chat:GenerationTimeoutSeconds` lên 30, dùng model nhẹ hơn, hoặc làm streaming (ngoài phạm vi plan).
- Tỷ lệ ±20% cho "tầm X" và danh sách từ đồng nghĩa danh mục cần PO chốt.
- Chưa hiểu câu hỏi nối tiếp ("rẻ hơn", "cái thứ 2"), so sánh, chào hỏi — thuộc Bước 3.

## Đợt 3 — Ý định, hỏi nối tiếp, so sánh và thẻ `[ID:x]` (Bước 3)

Hợp đồng `POST /api/chat` và schema DB **không đổi**. `AIChatMessages.AnswerType` (varchar(32)) có thêm giá trị mới; `AIMessageSources` giờ lưu **các sản phẩm được hiển thị** theo thứ tự hiển thị.

### Luồng mới

```
AiChatService
  → đọc lịch sử SQL + sản phẩm của các câu trả lời trước (AIMessageSources)
  → ChatTurnPlanner (tất định, không gọi LLM, không I/O) quyết định:
      Greeting     "xin chào", "cảm ơn", "ok"          → câu mẫu, không tìm, không gọi Gemini
      OutOfScope   đơn hàng, ship, đổi trả, trả góp…   → câu mẫu (chỉ khi câu không nhắc danh mục/hãng)
      Compare      "so sánh A với B", "A vs B",
                   "so sánh cái 1 và cái 3", "so sánh các sản phẩm trên"
      ProductDetail "cái thứ 2 nặng bao nhiêu", "con này có bảo hành không"
      Clarify      tham chiếu không rõ / số thứ tự vượt quá / "rẻ hơn" khi chưa có gợi ý → câu hỏi lại
      Search       tìm kiếm; câu không nêu danh mục được gộp với câu hỏi trước
                   ("dưới 10 triệu thôi" sau "điện thoại samsung")
                   + "rẻ hơn" / "đắt hơn" / "hãng khác" / "con khác"
  → ProductRetrievalService (Search, hoặc tìm từng sản phẩm khi so sánh theo tên) / GetByIdsAsync
  → Gemini 1 lần với "Nhiệm vụ" phù hợp (tư vấn / bảng so sánh / trả lời về 1 sản phẩm)
  → Search: thẻ = các [ID:x] Gemini nhắc, theo thứ tự nhắc, chỉ trong danh sách đã đưa; không nhắc ID nào → 3 sản phẩm đầu
```

### Quy tắc hỏi nối tiếp

- **Gộp tiêu chí:** câu hiện tại không nêu danh mục → lấy câu gần nhất (trong 3 câu trước) có danh mục làm gốc, chồng các câu sau lên; câu mới ghi đè giá/hãng/thông số, từ khóa được cộng dồn, "còn hàng" giữ nguyên. Câu nêu danh mục mới → tìm mới.
- **"rẻ hơn"** → giá tối đa = giá thấp nhất lượt trước − 1đ, bỏ giá tối thiểu. **"đắt hơn"** → giá tối thiểu = giá cao nhất lượt trước + 1đ.
- **"hãng khác"** → loại các hãng của lượt trước (`ExcludeBrandIds`, lọc bằng SQL). Mọi thao tác nối tiếp đều loại các sản phẩm vừa gợi ý.
- **Số thứ tự** ("cái thứ 2", "con số 1", "cái đầu", "cái cuối", "2 cái đầu") trỏ vào **danh sách gần nhất có từ 2 sản phẩm**, kể cả khi lượt ngay trước chỉ trả lời về 1 sản phẩm. **"con này / nó"** trỏ vào **lượt gần nhất có sản phẩm**; nếu lượt đó có nhiều sản phẩm thì hỏi lại kèm danh sách đánh số.
- Số thứ tự đi kèm đơn vị ("con 5 triệu", "máy 2 sim") không bị hiểu nhầm là tham chiếu.

### File mới

| File | Nội dung |
|---|---|
| `Features/AI/Chat/ChatTurnPlanner.cs` | `ChatIntent`, `ChatPlan`, `PreviousProduct`, các câu mẫu và toàn bộ quy tắc ở trên |
| `Features/AI/Chat/ChatAnswerSupport.cs` | `ChatAnswerTypes` (`grounded_answer`, `no_information`, `clarify`, `greeting`, `out_of_scope`, `generation_unavailable`, `generation_error`); `ProductReferences.SelectCited` đọc `[ID:x]` |

### File sửa

| File | Thay đổi |
|---|---|
| `Chat/AiChatService.cs` | Dùng `ChatTurnPlanner`; tách `AnswerAsync`, `ResolveCompareProductsAsync`, `LoadPreviousProductsAsync` (sản phẩm của các câu trả lời trong lịch sử); sources/thẻ = sản phẩm hiển thị; context có dòng "Nhiệm vụ" và mỗi sản phẩm mở đầu bằng `[ID:x]`; log ghi intent, loại câu trả lời và mã HTTP khi Gemini lỗi |
| `Chat/GeminiAnswerGenerator.cs` | Prompt: làm đúng "Nhiệm vụ", ghi `[ID:x]` sau tên sản phẩm, chỉ dùng mã có trong dữ liệu |
| `Search/ChatQueryParser.cs` | `DescribeFilters(criteria, vocabulary)` public (dùng cho tiêu chí đã gộp), thêm mô tả "khác thương hiệu …"; thêm stop word cho từ nối tiếp/so sánh |
| `Search/CatalogVocabulary.cs` | `CategoryName(id)`, `BrandName(id)` |
| `Search/ProductSearchModels.cs` | `ExcludeBrandIds` |
| `Search/ProductRetrievalService.cs` | Lọc `ExcludeBrandIds` bằng SQL; thêm `GetByIdsAsync` (chỉ sản phẩm active, giữ thứ tự) |

Frontend không cần sửa: `ChatBot.tsx` đã tự ẩn `[ID:x]` khi hiển thị.

### Test mới

- `ChatTurnPlannerTests.cs` (38 trường hợp): chào/cảm ơn; ngoài phạm vi và trường hợp có sản phẩm thì vẫn tìm; gộp tiêu chí và ghi đè; danh mục mới thì tìm mới; rẻ hơn/đắt hơn/hãng khác; nối tiếp khi chưa có gợi ý; 5 cách nói số thứ tự; "con 5 triệu" không phải tham chiếu; số thứ tự vượt quá; "con này" với 1 và nhiều sản phẩm; "nó" trong câu đầu; so sánh theo tên, kế thừa danh mục, theo số thứ tự/"các sản phẩm trên"; số thứ tự sau lượt chi tiết dùng danh sách trước đó.
- `ProductReferencesTests` (3 trường hợp): thứ tự trích dẫn, bỏ ID lạ/lặp, fallback 3 sản phẩm, chấp nhận `[id: 12]`.

### Kết quả kiểm tra đợt 3

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build backend/ECommerce.slnx -c Release` | Thành công |
| `dotnet test backend/ECommerce.slnx -c Release` | 114/114 pass |
| Hội thoại thật (Docker bật, `hybrid`) | "laptop dưới 20 triệu RAM 16GB" → 5 thẻ theo thứ tự Gemini nhắc → "cái thứ 2 nặng bao nhiêu" → HP Pavilion 15, 1,75 kg → "so sánh cái 1 và cái 3" → bảng Asus Vivobook 15 vs Lenovo IdeaPad Slim 5 → "con này còn hàng không" → hỏi lại kèm 2 lựa chọn |
| | "laptop … RAM 16GB" → "có con nào rẻ hơn không" → 2 laptop 14,5 triệu (dưới giá thấp nhất lượt trước) |
| | "tai nghe sony" → "có hãng khác không" → Anker/Samsung, không còn Sony |
| | "điện thoại samsung" → "dưới 10 triệu thôi" → Samsung 4,7–10 triệu |
| | "so sánh iPhone 15 Pro Max với Samsung Galaxy S24 Ultra" → bảng 2 sản phẩm đúng |
| | "xin chào", "cảm ơn bạn", "đơn hàng của tôi đang ở đâu" → câu mẫu trong ~0,1s, không gọi Gemini |

### Hạn chế còn lại của đợt 3

- **Gemini giới hạn tần suất:** khi gửi nhiều câu liên tiếp, Gemini trả **HTTP 429** và bot rơi về câu mẫu kèm thẻ. Cần key có hạn mức cao hơn khi demo, hoặc giãn nhịp hỏi.
- **Gộp tiêu chí chỉ dựa trên chữ của các câu trước:** hiệu ứng của "rẻ hơn"/"hãng khác" ở lượt trước không được nhớ sang lượt sau (ví dụ "rẻ hơn" rồi "RAM 32GB" sẽ tìm lại theo giá của câu gốc). Muốn nhớ phải lưu tiêu chí vào DB — cần đổi schema.
- Câu có cả danh mục mới lẫn tham chiếu ("tai nghe hợp với cái thứ 2") được hiểu là hỏi về sản phẩm thứ 2.
- So sánh theo tên chọn sản phẩm khớp tên nhất; với nhiều biến thể màu/dung lượng có thể không đúng biến thể khách nghĩ tới.

## Đợt 4 — Trả lời đúng câu hỏi (sửa theo 3 ảnh chụp lỗi)

### Chẩn đoán

| Ảnh | Câu hỏi | Nguyên nhân |
|---|---|---|
| 1 | "hello" | API đang chạy là bản build trước Bước 3 (chưa có intent chào hỏi) nên "hello" bị đem đi tìm sản phẩm; Gemini lỗi nên ra câu dự phòng chung chung |
| 3 | "laptop … học sinh sinh viên khoảng 10 triệu" | Tìm kiếm **đúng** (2 laptop 11,5–12 triệu) nhưng Gemini lỗi 429/timeout, câu dự phòng "chưa thể tổng hợp" không trả lời gì |
| 2 | "tôi nên mua sản phẩm nào trong 2 sản phẩm bạn gợi ý" | Bot tìm lại theo nguyên câu (ra AirPods, chuột…); chưa hiểu đây là "chọn giữa các sản phẩm vừa gợi ý"; prompt để Gemini lộ chữ "hệ thống… danh sách được truy xuất" |

Kết luận: prompt mẫu là cần nhưng không đủ — phải sửa thêm luồng xử lý và câu dự phòng.

### Thay đổi

| File | Thay đổi |
|---|---|
| `Chat/TemplateAnswerBuilder.cs` (mới) | Câu trả lời dựng từ SQL khi Gemini không trả lời được: **Search** liệt kê sản phẩm kèm giá, tồn kho, 3 thông số chính; **Compare** mỗi tiêu chí một dòng (chỉ các thông số có ở ≥2 sản phẩm) + gợi ý theo giá/RAM/bộ nhớ; **Recommend** đưa gợi ý lên dòng đầu; **Detail** liệt kê thông số. Không dùng bảng vì widget chỉ hiển thị `**đậm**` và xuống dòng |
| `Chat/ChatTurnPlanner.cs` | Intent mới `Recommend`: "nên mua/chọn/lấy…", "cái nào đáng mua/tốt hơn", "chọn giúp", "trong 2 sản phẩm bạn gợi ý" → chọn trong danh sách gợi ý gần nhất (N lấy từ câu, mặc định tối đa 3). Thêm câu mẫu `VagueQuestionReply`. **Sửa lỗi**: "cái thứ 2 không" — chữ "k" của "không" từng bị hiểu là đơn vị "k" (nghìn) nên bỏ qua số thứ tự |
| `Chat/AiChatService.cs` | Nhánh `Recommend`; dùng `TemplateAnswerBuilder` cho `generation_unavailable`/`generation_error`; context có dòng "Nhu cầu khách đã nêu" (3 câu trước + câu hiện tại); dòng điều kiện không còn chữ "hệ thống"; nhiệm vụ so sánh không dùng bảng; chặn câu lạc đề (dưới đây) |
| `Chat/GeminiAnswerGenerator.cs` | **Prompt mẫu cố định** (`SystemPrompt`): câu đầu trả lời thẳng; tối đa 3 sản phẩm dạng `• **Tên** [ID:x] — giá — lý do`; ~120 từ; không bảng/`#`/`*`; cấm nhắc "hệ thống/truy xuất/danh sách/catalog/bộ lọc"; khi không khớp thì nói rõ và gợi ý điều chỉnh. `MaxTokens` 1024 → 600 |
| `Search/ProductRanker.cs`, `ProductSearchModels.cs` | `RankedProduct.NameMatch`: từ khóa khớp tên/SKU/hãng/danh mục (không tính khớp lặt vặt trong mô tả); `ProductSearchResult.TopVectorScore` |
| `Search/ProductRetrievalService.cs` | Trả điểm cosine cao nhất; log ở mức Debug |
| `Search/ProductSpecParser.cs` | `ReadPairs` trả các cặp thông số gốc theo thứ tự JSON (dùng chung cho bộ lọc và câu dự phòng) |
| `AiModule.cs`, `appsettings.json` | `AI:Chat:MinVectorScore = 0.45` |

**Chặn câu lạc đề:** câu không có bộ lọc, không khớp tên/hãng/danh mục nào và điểm cosine cao nhất < `MinVectorScore` → hỏi lại "Mình chưa rõ bạn đang tìm sản phẩm nào…". Ngưỡng đo trên dữ liệu thật với bge-m3:

| Câu | Cosine cao nhất |
|---|---|
| "viết cho tôi một bài thơ" / "bạn tên là gì" / "thời tiết hôm nay thế nào" / "abc xyz" | 0,37 / 0,39 / 0,40 / 0,43 → hỏi lại |
| "quà tặng bạn gái" / "thiết bị pin trâu" / "đồ cho dân văn phòng" / "máy chơi game tốt" / "tai nghe chống ồn" | 0,47 / 0,51 / 0,53 / 0,58 / 0,64 → tìm bình thường |

### Test

134/134 pass (thêm 20): `ChatTurnPlannerTests` — "hello"/"Hello!", 6 cách nói Recommend, Recommend sau lượt chi tiết dùng danh sách trước, "có nên mua cái thứ 2 không" là câu hỏi chi tiết, câu có danh mục mới vẫn là tìm kiếm, không có gợi ý thì không Recommend; `TemplateAnswerBuilderTests` (7) — định dạng giá, tồn kho, thông số chính, chỉ thông số chung, không có ký tự bảng, gợi ý đặt đầu khi Recommend, sản phẩm tương đương; `ProductRankerTests` — `NameMatch`.

### Kiểm chứng thật (Docker bật)

Với Gemini **tắt hẳn** (`Gemini__ApiKey` rỗng) và với Gemini **bị 429**, cả 3 kịch bản trong ảnh đều trả lời đúng ý:
- "hello" → câu chào (không tìm, không gọi Gemini).
- "laptop … khoảng 10 triệu" → "Mình tìm được 2 sản phẩm phù hợp (danh mục Laptops, giá từ 8.000.000đ đến 12.000.000đ):" + 2 dòng có giá, tồn kho, CPU/RAM/ổ cứng.
- "tôi nên mua sản phẩm nào trong 2 sản phẩm bạn gợi ý" → intent `Recommend`, dòng đầu "Gợi ý nhanh: ưu tiên giá thì chọn **Dell Vostro 3520…**", sau đó so sánh từng tiêu chí của đúng 2 laptop đó, 0,3 giây.

**Chưa kiểm chứng được câu trả lời của Gemini với prompt mới**: suốt đợt test, key hiện tại trả HTTP 429 (hết hạn mức) cho mọi lời gọi.

## Đợt 5 — Trả lời đúng trọng tâm nhu cầu (ngành học, mục đích sử dụng)

### Vấn đề

- "💻 Gợi ý laptop giá rẻ" ra laptop 19,5–34 triệu: chữ "rẻ" bị bỏ qua, không có ưu tiên giá (laptop rẻ nhất trong DB là 11,5 triệu).
- "sản phẩm nào phù hợp với học sinh theo ngành xây dựng" ra bảng so sánh 3 máy cũ, không nhắc gì tới xây dựng: bot không có khái niệm nhu cầu sử dụng và còn hiểu nhầm thành "chọn trong danh sách cũ".

### Cách làm (đã chọn: bộ quy tắc + Gemini, 3 sản phẩm mỗi câu)

**Hồ sơ nhu cầu** — `Features/AI/Search/UsageProfiles.cs` (mới, để PO duyệt nội dung tư vấn):

| Hồ sơ | Ví dụ từ kích hoạt | Cấu hình nên có (điểm) |
|---|---|---|
| Ngành xây dựng, kiến trúc, kỹ thuật (AutoCAD, Revit, SketchUp) | xây dựng, kiến trúc, autocad, revit, sketchup, nội thất, cơ khí | RAM ≥16GB (+2/−2), card rời (+3/−2), CPU dòng H/HX/HS hoặc Apple M Pro (+2), SSD ≥512GB (+1), màn ≥15.6" (+1), cảnh báo nếu >2.3 kg |
| Thiết kế đồ họa, dựng video | đồ họa, photoshop, premiere, dựng phim, edit video | RAM ≥16 (+2/−2), card rời (+2/−1), màn OLED/2.8K/Retina (+2), CPU H (+1) |
| Chơi game | gaming, chơi game, game | card rời (+3/−3), màn ≥120Hz (+2/−1), RAM ≥16 (+1/−1), CPU H (+1) |
| Lập trình | lập trình, code, ngành IT, công nghệ thông tin | RAM ≥16 (+2/−1), SSD ≥512 (+1), CPU H (+1), ≤1.7 kg (+1) |
| Mỏng nhẹ, hay di chuyển | mỏng nhẹ, đi công tác, mang đi | ≤1.4 kg (+3), cảnh báo nếu >1.8 kg |
| Học tập, văn phòng | học tập, văn phòng, word, excel, học sinh, sinh viên | giá ≤20 triệu (+2), ≤1.7 kg (+1), RAM ≥8 (+1/−1) |

- Câu nhắc nhiều nhu cầu chọn hồ sơ cụ thể nhất ("học sinh ngành xây dựng" → xây dựng). Hồ sơ chỉ áp cho laptop: câu không nêu danh mục thì tự thêm Laptops; "chuột gaming" không gán hồ sơ.
- "giá rẻ, rẻ nhất, tiết kiệm, bình dân, giá mềm, giá tốt, học sinh, sinh viên" → `PreferLowPrice` ("rẻ hơn" vẫn là thao tác nối tiếp).
- `ProductFitScorer` chấm điểm và sinh **lý do có số liệu thật**, ví dụ "card rời RTX 4050 dựng mô hình 3D trên Revit/SketchUp mượt", "chỉ có đồ họa tích hợp Intel Iris Xe Graphics, chạy AutoCAD 2D được nhưng dựng 3D trên Revit/SketchUp sẽ chậm". Thiếu dữ liệu thì bỏ qua tiêu chí, không trừ điểm.

**Xếp hạng** — `ProductRanker`:
- Có hồ sơ: điểm hợp nhu cầu giảm dần; nếu ưu tiên giá rẻ thì **máy đạt đủ yêu cầu cốt lõi, rẻ nhất trước**.
- Chỉ ưu tiên giá rẻ + có bộ lọc ("laptop giá rẻ"): giá tăng dần, còn hàng trước.

**Planner**: câu nêu nhu cầu mới sau một danh sách → tìm kiếm theo nhu cầu (gộp danh mục của lượt trước); chỉ "chọn trong danh sách" khi trỏ rõ ("trong 3 cái đó …") hoặc hỏi "nên mua cái nào" không kèm nhu cầu mới. Hồ sơ và ưu tiên giá được nhớ qua các lượt; Recommend/Compare/Detail mang theo hồ sơ của cuộc trò chuyện.

**Câu trả lời**:
- Gemini nhận thêm "Nhu cầu sử dụng: … phần mềm … cấu hình nên có …" và mỗi sản phẩm một dòng "Đánh giá theo nhu cầu — Ưu: …; Hạn chế: …"; prompt yêu cầu câu đầu nói nhu cầu cần cấu hình gì và lý do mỗi máy gắn với nhu cầu.
- Câu dự phòng (Gemini lỗi): nêu yêu cầu cấu hình của nhu cầu, 3 máy kèm 2 ưu điểm và 1 lưu ý; Recommend trả lời thẳng "nên chọn **X** vì …"; Compare tối đa 5 dòng tiêu chí, ưu tiên GPU/RAM/CPU khi có hồ sơ.
- `AI:Chat:MaxRetrievedSources` 5 → 3.

### File

Mới: `Search/UsageProfiles.cs` (`UsageProfile`, `FitCriterion`, `UsageProfiles`, `ProductFit`, `ProductFitScorer`). Sửa: `Search/ProductSpecParser.cs` (`ProductHardware`, `ReadHardware`: card rời/tích hợp, lớp CPU, trọng lượng, màn hình), `Search/ProductSearchModels.cs` (`Profile`, `PreferLowPrice`, `CategoryImplied`, `RankedProduct.Fit`), `Search/ChatQueryParser.cs`, `Search/ProductRanker.cs`, `Chat/ChatTurnPlanner.cs`, `Chat/AiChatService.cs`, `Chat/GeminiAnswerGenerator.cs`, `Chat/TemplateAnswerBuilder.cs`, `AiModule.cs`, `appsettings.json`.

### Test

183/183 pass (thêm 49): `UsageProfileTests` (nhận diện 9 cách nói, ưu tiên hồ sơ cụ thể, không áp cho chuột, "nhé" không thành mỏng nhẹ, ưu tiên giá rẻ), `ProductHardwareTests` (6 giá trị GPU thật trong DB, 8 loại CPU, trọng lượng/màn hình, thiếu dữ liệu), `ProductFitScorerTests`, ranker (giá rẻ, hồ sơ + ngân sách), planner (nhu cầu mới → tìm kiếm, "trong 3 cái đó cái nào hợp ngành xây dựng" → Recommend có hồ sơ, hồ sơ nhớ qua lượt), câu dự phòng theo nhu cầu.

### Kiểm chứng thật (Gemini 429 → câu dự phòng)

1. "💻 Gợi ý laptop giá rẻ" → Dell Vostro 3520 11,5 triệu, MSI Modern 14 12 triệu, MSI Modern 14 16GB 14,5 triệu.
2. "sản phẩm nào phù hợp với học sinh theo ngành xây dựng" → "Với nhu cầu ngành xây dựng, kiến trúc, kỹ thuật (AutoCAD, Revit, SketchUp), laptop nên có RAM từ 16GB, card đồ họa rời (NVIDIA RTX), CPU dòng H/HX và SSD từ 512GB…" + MSI Thin GF63 RTX 4050 18 triệu, Asus TUF F15 RTX 3050 18,5 triệu, Lenovo LOQ 15 RTX 4050 21 triệu (lưu ý nặng 2,4 kg), mỗi máy có lý do.
3. "trong 3 cái đó nên chọn cái nào" → "bạn nên chọn **MSI Thin GF63…** (18.000.000đ) vì RAM 16GB…, card rời RTX 4050…, CPU AMD Ryzen 5 7640HS…" và lý do/hạn chế của 2 máy còn lại.

Hạn chế: chưa xem được câu Gemini viết theo hồ sơ (key vẫn 429); lượt đầu sau khi khởi động API có thể chạy `lexical_only` vì Ollama nạp model lâu hơn 10 giây; nội dung tư vấn trong `UsageProfiles.cs` cần PO duyệt.

## Chưa làm (theo plan v2)

- ~~Giai đoạn 1~~ và ~~Giai đoạn 2~~: đã làm ở đợt 2 (trừ cảnh báo khởi động khi Ollama thiếu model `bge-m3`).
- ~~Giai đoạn 3~~: đã làm ở đợt 3.
- **Giai đoạn 4:** nhận job index nguyên tử; giảm job index thừa; Qdrant store dạng Singleton.
- **Giai đoạn 5:** unit test cho parser, golden set.
- **Thủ tục:** thu hồi các key đã lộ; xin PO và Tùng duyệt (Backlog, Docker, dependency, migration); commit.
