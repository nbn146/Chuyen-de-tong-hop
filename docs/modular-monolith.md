# Modular Monolith

> Trạng thái: **đề xuất, chờ team duyệt** (nhánh `refactor/modular-monolith`). Theo `AGENTS.md`, thay đổi kiến trúc cần được team đồng ý trước khi merge vào `develop`.

Hệ thống vẫn là **một ứng dụng ASP.NET Core, một database SQL Server, một `AppDbContext`**. Không có microservices. Điểm khác so với Layered Architecture thuần là code được chia theo **module nghiệp vụ**, và module chỉ gọi nhau qua **Contracts**.

## Cấu trúc backend

```text
backend/src/ECommerce.Api/
  Program.cs                 # chỉ ghép các module (composition root)
  Data/                      # AppDbContext, PersistenceBoundary
  Migrations/                # một chuỗi migration duy nhất
  Shared/                    # Exceptions, Extensions, Middleware, PagedResult, HealthController
  Modules/
    Identity/      Auth, Profile, Addresses, người dùng, vai trò
    Catalog/       Products, Categories, Brands
    Inventory/     tồn kho, lịch sử nhập/xuất
    ShoppingCart/  giỏ hàng
    Orders/        đặt hàng, mock QR, quản trị đơn hàng
    Admin/         facade back-office, báo cáo
    AI/            chatbot RAG, indexing
```

Mỗi module có các thư mục `Controllers/`, `Services/`, `Dtos/`, `Entities/`, `Persistence/` (EF configurations), `Contracts/` (giao diện công khai) và file `XxxModule.cs` chứa `AddXxxModule()` để tự đăng ký DI. Namespace là `ECommerce.Api.Modules.<Module>` (riêng `Contracts` là `...<Module>.Contracts`).

## Quy tắc

1. Module A **không** dùng `Services`, `Controllers`, `Dtos`, `Persistence` của module B. Chỉ được dùng `B.Contracts` (và `B.Entities` cho quan hệ EF dùng chung model).
2. Module A **không** truy cập `DbSet` thuộc module B. Cần dữ liệu hoặc thao tác của B thì thêm interface vào `B/Contracts`.
3. `Shared/` không phụ thuộc module nào.
4. `Program.cs` chỉ gọi `AddXxxModule()`.
5. Migration vẫn dùng một `AppDbContext` và một chuỗi migration. Đổi namespace entity không làm đổi schema.

Quy tắc 1, 2, 3 được kiểm tra tự động bởi `backend/tests/ECommerce.Api.Tests/ModuleBoundaryTests.cs`.

## Contracts hiện có

| Module | Contract | Dùng bởi |
|---|---|---|
| Inventory | `IStockLedger` (hoàn kho khi hủy đơn) | Orders |
| Identity | `IAddressLookup`, `IUserDirectory` | Orders, Admin |
| ShoppingCart | `ICheckoutCart` | Orders |
| Orders | `IOrderAdministration`, `OrderConstants`, DTO quản trị đơn hàng | Admin |

## Nợ kỹ thuật đã ghi nhận

`ModuleBoundaryTests.AllowedDbSetAccess` liệt kê các truy cập chéo còn lại. Danh sách này chỉ được **giảm**; test sẽ báo lỗi nếu một mục không còn cần thiết.

- `Inventory -> Products`: số lượng tồn lưu trên bảng `Products`.
- `ShoppingCart -> Products`: kiểm tra sản phẩm khi thêm vào giỏ.
- `AI -> Products`: indexing đọc catalog.
- `Admin -> Orders, OrderDetails, Products, Users`: `ReportService` là read model.

Bước tiếp theo đề xuất: thêm `Catalog.Contracts.IProductCatalog` cho Cart, Inventory và AI; tách `ReportService` thành các truy vấn đọc theo từng module.

## Frontend

```text
frontend/src/
  App.tsx, main.tsx, routes/AppRouter.tsx
  features/{auth,profile,catalog,cart,orders,admin,chat}/
    pages/ components/ services/ types/ context/ (tùy feature)
  shared/{components,layouts,services,context,config}
```

Mục tiêu: feature không import nội bộ của feature khác; thành phần dùng chung đặt trong `shared/`. Hiện chưa đạt hoàn toàn: `shared/components/Header` và `shared/services/apiClient` còn import từ `features/auth`, `features/cart`, `features/catalog`. Chưa có test kiểm tra phía frontend.

## Ghi chú vận hành

- Migration `AddAIChatRagCore` trùng tên được gộp thành bản `20261106180022`. Database nào đã áp dụng bản `20261021155203` cần xử lý thủ công trong `__EFMigrationsHistory`.
- `ChatService` cũ và test của nó đã bị thay bằng module `AI`.
