# Chatbot RAG (AI-01)

## Thành phần

- `POST /api/chat` giữ nguyên route và tiếp tục nhận `message` cùng `history` như widget hiện tại.
- Module `Features/AI` ở trong Web API hiện có, dùng chung `AppDbContext`; không tạo service hay project riêng.
- Semantic Kernel gọi Gemini để tổng hợp câu trả lời. Ollama với model `bge-m3` tạo embedding; Qdrant lưu và truy vấn vector.
- SQL Server giữ session, message, source, nội dung chunk, hash và hàng đợi index. Giá và tồn kho chỉ lấy từ SQL khi chat.
- `AiIndexWorker` tự tạo job ban đầu cho sản phẩm đang bán; thêm/sửa/ngừng bán sản phẩm và đổi danh mục sẽ xếp job trong cùng lần `SaveChanges`.

## Cấu hình

Các giá trị mặc định trong `appsettings.json`:

| Cấu hình | Mặc định |
| --- | --- |
| `AI:Ollama:Endpoint` | `http://localhost:11434` |
| `AI:Ollama:EmbeddingModel` | `bge-m3` |
| `AI:Qdrant:Endpoint` | `http://localhost:6334` (gRPC) |
| `AI:Qdrant:Collection` | `ecommerce_products_v1` |
| `Gemini:Model` | `gemini-3.8-flash` |

Không lưu Gemini key trong `appsettings.json`. Đặt key bằng User Secrets:

```powershell
dotnet user-secrets set "Gemini:ApiKey" "<Gemini API key>" --project backend/src/ECommerce.Api
```

Web API hiện đọc connection string từ `ConnectionStrings:ECommerce`. Ollama và Qdrant phải truy cập được từ tiến trình .NET. Nếu API cũng chạy trong container, thay `localhost` bằng hostname dịch vụ Docker tương ứng.

## Khởi chạy dịch vụ và API

Qdrant có cấu hình local riêng, không đóng gói Web API vào Docker:

```powershell
docker compose -f docker-compose.ai.yml up -d qdrant
```

Tải BGE-M3 vào container Ollama đang dùng:

```powershell
docker exec -it <ollama-container> ollama pull bge-m3
```

Sau khi cấu hình SQL Server và Gemini key, áp migration rồi chạy API:

```powershell
dotnet ef database update --project backend/src/ECommerce.Api --startup-project backend/src/ECommerce.Api
dotnet run --project backend/src/ECommerce.Api
```

Sau lần khởi chạy đầu, worker sẽ tạo job cho các sản phẩm active theo từng lô. Cần để Ollama và Qdrant chạy trong giai đoạn này. Quản trị viên có thể yêu cầu lập chỉ mục lại bằng `POST /api/ai/index/rebuild` với JWT role `Admin`.

## Hợp đồng chat

Widget tiếp tục gửi lịch sử hội thoại; API nhận thêm `sessionId` và `sessionToken` sau lượt đầu:

```json
{
  "message": "Tư vấn laptop dùng học tập",
  "history": [],
  "sessionId": null,
  "sessionToken": null
}
```

Response tiếp tục có `reply` và `products`, đồng thời trả `sessionId`, `sessionToken` cho khách chưa đăng nhập và `sources`. Frontend lưu token guest ở trình duyệt; SQL chỉ lưu SHA-256 hash. Session đã có được xác thực theo user đăng nhập hoặc guest token. Nếu session cũ/hết hạn/không thuộc người gửi, API mở session mới và không đọc lịch sử session cũ.

Ở session hiện có, lịch sử SQL là nguồn hội thoại chuẩn; `history` client tiếp tục được nhận để giữ hợp đồng frontend. Lịch sử gửi trong request đầu có thể được nhập vào session mới.

## Phạm vi dữ liệu và vận hành

- Hiện tạo một chunk cho mỗi sản phẩm từ tên, SKU, thương hiệu, danh mục, mô tả và thông số. Giá và tồn kho không được nhúng vào vector.
- Qdrant chỉ trả candidate. API đọc lại SQL để kiểm tra sản phẩm còn active và hash chunk khớp nội dung hiện tại trước khi gửi cho Gemini hoặc lưu source.
- Job lỗi tự retry tối đa 5 lần. Sau đó dùng endpoint rebuild để xếp lại các sản phẩm active.
- Session hết hạn sau 30 ngày; việc dọn dữ liệu lịch sử hết hạn chưa được tự động hóa.
- Mỗi sản phẩm hiện là một chunk; chưa có lexical/hybrid search, streaming, bộ lọc ngân sách có cấu trúc hay dashboard theo dõi job.
