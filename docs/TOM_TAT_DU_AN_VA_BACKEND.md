# TÀI LIỆU TỔNG HỢP TOÀN DIỆN DỰ ÁN & PHẦN BACKEND (ÔN THI VẤN ĐÁP)

> **Dự án:** Website Thương mại Điện tử Thiết bị Công nghệ (**ElectroTech**)  
> **Học phần:** Chuyên đề Tổng hợp (kế thừa & hoàn thiện từ Quản lý Dự án)  
> **Thành viên phụ trách Backend:** **Đào Đăng Quang** (GitHub: `DDQuang1` | Email: `daodquang295@gmail.com`)  
> **Nhánh Git phụ trách:** `backend` (PR về `develop` & `Dquang_backend`)  
> **Phạm vi mã nguồn:** `backend/src/ECommerce.Api/`, `backend/tests/ECommerce.Api.Tests/`

---

## MỤC LỤC
1. [Tổng quan dự án: Mục tiêu, Đối tượng & Giá trị mang lại](#1-tổng-quan-dự-án)
2. [Cơ cấu nhóm & Phân chia trách nhiệm rõ ràng](#2-phân-công-thành-viên-trong-nhóm)
3. [Kiến trúc hệ thống & Công nghệ sử dụng](#3-kiến-trúc-hệ-thống--công-nghệ)
4. [Lộ trình phát triển & 3 Sprint (15 User Stories)](#4-lộ-trình-phát-triển-3-sprint)
5. [Chi tiết phần Backend của Đào Đăng Quang](#5-chi-tiết-phần-backend-của-đào-đăng-quang)
   - [5.1. Thiết kế Cơ sở Dữ liệu (14 Entities & Khóa chính)](#51-cơ-sở-dữ-liệu-14-entities)
   - [5.2. Danh sách toàn bộ 35+ API Endpoints chuẩn xác](#52-danh-sách-toàn-bộ-api-endpoints)
   - [5.3. Các luồng nghiệp vụ Backend trọng điểm](#53-các-luồng-nghiệp-vụ-backend-trọng-điểm)
   - [5.4. Kỹ thuật lập trình & Design Patterns áp dụng](#54-kỹ-thuật-lập-trình--design-patterns)
6. [Bộ câu hỏi vấn đáp thường gặp & Câu trả lời ghi điểm](#6-bộ-câu-hỏi-vấn-đáp-chuẩn-bị-sẵn)
7. [Hướng dẫn cài đặt & Khởi chạy dự án](#7-hướng-dẫn-chạy-dự-án)

---

## 1. TỔNG QUAN DỰ ÁN

### 1.1. Dự án ElectroTech là gì?
**ElectroTech** là một hệ thống website thương mại điện tử chuyên kinh doanh các sản phẩm công nghệ (Laptop, Điện thoại, Linh kiện, Phụ kiện...). Hệ thống được phát triển theo mô hình Agile/Scrum trong 6 tuần (3 Sprint, mỗi Sprint kéo dài 2 tuần), cung cấp đầy đủ hai phân hệ:
1. **Phân hệ Khách hàng (Customer & Guest):** Duyệt danh mục, tìm kiếm, lọc theo giá/thương hiệu, đăng ký/đăng nhập, giỏ hàng trực tuyến, thanh toán COD/QR Code, quản lý địa chỉ nhận hàng, theo dõi & hủy đơn hàng, tư vấn mua sắm với Trợ lý AI.
2. **Phân hệ Quản trị (Admin Portal):** Quản lý sản phẩm (CRUD, soft delete), quản lý danh mục, quản lý tồn kho (nhập/xuất/điều chỉnh có lịch sử giao dịch), quản lý người dùng, quản lý vòng đời đơn hàng, xem báo cáo doanh thu & dashboard phân tích.

### 1.2. Dự án giải quyết bài toán gì và giúp được cho ai?

| Đối tượng hưởng lợi | Giá trị và lợi ích thực tế mà hệ thống mang lại |
|---|---|
| **Khách hàng mua sắm (Customer)** | - Trải nghiệm mua hàng trực tuyến mượt mà, tìm kiếm sản phẩm nhanh theo nhiều tiêu chí (giá, danh mục, thương hiệu).<br>- Đồng bộ giỏ hàng trên Server (không bị mất khi đổi thiết bị).<br>- Quản lý nhiều địa chỉ nhận hàng tiện lợi.<br>- Tự do theo dõi tình trạng đơn và chủ động hủy đơn khi còn ở trạng thái chờ.<br>- Có Chatbot AI thông minh tư vấn cấu hình, ngân sách 24/7. |
| **Chủ cửa hàng / Quản trị viên (Admin)** | - Số hóa quy trình kinh doanh từ bán lẻ đến quản lý kho.<br>- Kiểm soát tồn kho chặt chẽ theo thời gian thực (tránh thất thoát, phát hiện sản phẩm sắp hết hàng).<br>- Giám sát dòng tiền, doanh thu theo ngày và nắm bắt top sản phẩm bán chạy qua Dashboard trực quan.<br>- Quản lý đơn hàng minh bạch, lưu vết lịch sử đổi trạng thái đơn (ai đổi, lúc nào). |
| **Nhóm phát triển sinh viên** | - Thực hành kiến trúc 3 lớp chuẩn công nghiệp trên **.NET 10** và **React 19**.<br>- Áp dụng quy trình Git Flow chuyên nghiệp (nhánh develop/main, PR review, CI test tự động).<br>- Rèn luyện kỹ năng xử lý Concurrency, Transaction, Security (JWT, BCrypt, IDOR prevention). |

---

## 2. PHÂN CÔNG THÀNH VIÊN TRONG NHÓM

Theo tài liệu thống nhất của môn Chuyên đề Tổng hợp (`PHAN_CONG_CONG_VIEC.md`):

| Thành viên | Vai trò | Trách nhiệm chính trong dự án |
|---|---|---|
| **Toản** | Team Leader | Quản lý Product Backlog, kiến trúc tổng thể, tích hợp hệ thống, code review, viết tài liệu và kịch bản demo. |
| **Anh Quang** | Frontend Developer | Xây dựng giao diện React + TypeScript + Vite: Màn hình Guest, Customer, Admin; Quản lý state, tích hợp API, Router, UI Chatbot. |
| **Đào Đăng Quang** | **Backend Developer** | **Phụ trách toàn bộ Core Backend:** Thiết kế Database 14 bảng, EF Core Migrations, JWT Auth, Catalog APIs, Giỏ hàng, Checkout Transaction, Quản lý kho, Quản lý đơn hàng, Báo cáo thống kê, Unit Tests. |
| **Bình** | QA / Test & CI/CD | Lập kế hoạch kiểm thử, xây dựng GitHub Actions CI tự động build/test, kiểm thử E2E và triển khai demo. |
| **Bá Nam** | Backend AI | Phụ trách riêng phân hệ AI: Endpoint `POST /api/chat`, tích hợp Google Gemini 1.5 Flash API, xử lý prompt tư vấn theo catalog sản phẩm. |

> ⭐ **Điểm cần nhấn mạnh khi bảo vệ:**  
> *"Em (Đào Đăng Quang) phụ trách toàn bộ phần Core Backend nghiệp vụ của hệ thống (từ Database, Auth, Catalog, Giỏ hàng, Đặt hàng, Kho, Admin đến Báo cáo). Riêng phần tích hợp AI Chatbot do bạn Bá Nam đảm nhận, và phần giao diện do bạn Anh Quang phát triển."*

---

## 3. KIẾN TRÚC HỆ THỐNG & CÔNG NGHỆ

### 3.1. Sơ đồ kiến trúc 3 lớp (Layered Monolith Architecture)
```
  [ Frontend: React 19 + TypeScript + Vite + Tailwind CSS ]
                           │  HTTP / HTTPS (JSON RESTful)
                           ▼
  [ Backend: ASP.NET Core Web API (.NET 10) ]
   ├── Controllers   : Nhận HTTP Request, trích xuất Claim UserId, gọi Service, trả HTTP Status
   ├── Services      : Xử lý Business Logic, Validation, Transaction, Mapping DTO
   ├── Data (EF Core): AppDbContext, Fluent API Configurations, ChangeTracker Auto-Auditing
   └── Middlewares   : ExceptionHandlingMiddleware (Bắt lỗi toàn cục, trả RFC 7807)
                           │  Entity Framework Core 10 (Code-First)
                           ▼
  [ Database: Microsoft SQL Server ] (14 bảng có khóa ngoại & index)
```

### 3.2. Bảng công nghệ sử dụng

| Tầng | Công nghệ / Thư viện | Mục đích sử dụng |
|---|---|---|
| **Runtime & Framework** | .NET 10 (C# 13) | Phiên bản .NET mới nhất, hiệu năng cao, hỗ trợ Minimal API & Controller-based. |
| **ORM / Data Access** | Entity Framework Core 10 | Code-First, Migration tự động, LINQ, Transaction management. |
| **Cơ sở dữ liệu** | Microsoft SQL Server | RDBMS mạnh mẽ, hỗ trợ ACID transaction, Foreign Keys, Indexes. |
| **Bảo mật & Auth** | JWT Bearer (`System.IdentityModel.Tokens.Jwt`) + `BCrypt.Net-Next` | Xác thực người dùng qua Access Token, mã hóa mật khẩu 1 chiều an toàn. |
| **API Documentation** | Swagger / OpenAPI (`Swashbuckle.AspNetCore`) | Sinh tài liệu tương tác trực quan cho Frontend tại `/swagger`. |
| **AI Integration** | Google Gemini 1.5 Flash API | Nhận diện ngữ cảnh và tư vấn sản phẩm tiếng Việt qua HttpClient. |

---

## 4. LỘ TRÌNH PHÁT TRIỂN: 3 SPRINT

Hệ thống được hoàn thiện qua 3 Sprint với 15 User Stories cốt lõi:

- **Sprint 1 — Danh mục sản phẩm & Xác thực (Tuần 1–2):**
  - *US-01 & US-02:* Khách xem danh sách sản phẩm (tìm kiếm, lọc, phân trang) & xem chi tiết.
  - *US-03 & US-04:* Đăng ký tài khoản (BCrypt) & Đăng nhập cấp JWT Token.
  - *US-05:* Quản trị viên thêm, sửa, xóa mềm sản phẩm & danh mục.
- **Sprint 2 — Mua sắm & Đặt hàng (Tuần 3–4):**
  - *US-06:* Quản lý giỏ hàng lưu trữ trên Server (thêm, cập nhật số lượng, kiểm tra tồn kho).
  - *US-07:* Đặt hàng (Checkout) với thanh toán COD hoặc Mock QR Transfer (Transaction trừ kho).
  - *US-08:* Khách hàng xem lịch sử đơn hàng & chi tiết từng đơn.
  - *US-09:* Quản lý hồ sơ cá nhân và sổ địa chỉ giao hàng (tối đa 5 địa chỉ, đặt mặc định).
  - *US-10:* Hủy đơn hàng an toàn (chỉ khi Pending, tự động hoàn trả tồn kho).
- **Sprint 3 — Quản trị, Kho hàng, Báo cáo & Hoàn thiện (Tuần 5–6):**
  - *US-11:* Quản lý tồn kho: nhập kho, xuất kho, điều chỉnh, ghi nhận lịch sử biến động kho.
  - *US-12:* Báo cáo & Thống kê: Dashboard tổng quan, doanh thu theo ngày, Top sản phẩm bán chạy.
  - *US-13:* Quản lý danh sách người dùng (kèm phân trang, tìm kiếm).
  - *US-14:* Trợ lý Chatbot AI tư vấn sản phẩm thông minh (Gemini).
  - *US-15:* Quên mật khẩu và đặt lại mật khẩu bằng Token an toàn.

---

## 5. CHI TIẾT PHẦN BACKEND CỦA ĐÀO ĐĂNG QUANG

### 5.1. Cơ sở dữ liệu: 14 Entities & Kiểu dữ liệu chính xác
> ⚠️ **LƯU Ý CỰC KỲ QUAN TRỌNG KHI VẤN ĐÁP:** Hệ thống sử dụng khóa chính dạng số nguyên tự tăng (`int` hoặc `long` IDENTITY), **KHÔNG DÙNG GUID**.

```
  Role (int) ──────1:N─────► User (int) ──────1:1─────► Cart (int) ──────1:N─────► CartItem (long)
                              │  │                                                      ▲
                              │  │ 1:N                                                  │
                              │  ├────────► Address (int)                               │
                              │  │ 1:N                                                  │
                              │  ├────────► RefreshToken (long)                         │
                              │  │ 1:N                                                  │
                              │  ├────────► PasswordResetToken (long)                   │
                              │  │ 1:N                                                  │
                              │  └────────► Order (long) ──────1:N─────► OrderDetail (long)
                              │                │ 1:N                            │
                              │                ▼                                │
                              │         OrderStatusHistory (long)               ▼
                              ▼                                            Product (int)
                     InventoryTransaction (long) ◄──────1:N─────────────────────┤
                                                                                ├─1:N─► ProductImage (int)
                               Category (int) ◄──────N:1────────────────────────┤
                               Brand (int)    ◄──────N:1────────────────────────┘
```

| Tên Bảng (Entity) | Khóa chính (PK) | Các trường cốt lõi & Khóa ngoại (FK) | Ý nghĩa nghiệp vụ |
|---|---|---|---|
| **Roles** | `RoleID` (`int`) | `RoleName`, `Description` | Phân quyền: `Admin`, `Customer`. |
| **Users** | `UserID` (`int`) | `RoleID` (FK), `Email`, `PasswordHash`, `FullName`, `Phone`, `IsActive` | Tài khoản người dùng hệ thống. |
| **Categories** | `CategoryID` (`int`) | `CategoryName`, `ParentID` (FK tự trỏ), `IsActive` | Danh mục sản phẩm đa cấp. |
| **Brands** | `BrandID` (`int`) | `BrandName`, `Description`, `IsActive` | Hãng sản xuất / Thương hiệu. |
| **Products** | `ProductID` (`int`) | `CategoryID` (FK), `BrandID` (FK), `SKU`, `ProductName`, `Price`, `StockQuantity`, `IsActive` | Sản phẩm. Xóa là **Soft Delete** (`IsActive = false`). |
| **ProductImages** | `ImageID` (`int`) | `ProductID` (FK), `ImageURL`, `IsPrimary`, `DisplayOrder` | Hình ảnh sản phẩm (ảnh chính & ảnh phụ). |
| **Addresses** | `AddressID` (`int`) | `UserID` (FK), `ReceiverName`, `ReceiverPhone`, `Province`, `District`, `Ward`, `FullAddress`, `IsDefault` | Sổ địa chỉ giao hàng của khách. |
| **Carts** | `CartID` (`int`) | `UserID` (FK) — Quan hệ 1-1 với User | Giỏ hàng lưu trên Server. |
| **CartItems** | `CartItemID` (`long`) | `CartID` (FK), `ProductID` (FK), `Quantity` | Từng dòng sản phẩm trong giỏ. |
| **Orders** | `OrderID` (`long`) | `UserID` (FK), `ReceiverName`, `ShippingAddress`, `SubTotal`, `ShippingFee`, `TotalAmount`, `PaymentMethod`, `PaymentStatus`, `OrderStatus` | Đơn hàng. Trạng thái: `PENDING`, `CONFIRMED`, `SHIPPING`, `COMPLETED`, `CANCELLED`. |
| **OrderDetails** | `OrderDetailID` (`long`)| `OrderID` (FK), `ProductID` (FK), `ProductName`, `SKU`, `Quantity`, `UnitPrice`, `TotalPrice` | Snapshot thông tin sản phẩm tại thời điểm mua (không phụ thuộc giá hiện tại của Product). |
| **OrderStatusHistories** | `HistoryID` (`long`)| `OrderID` (FK), `OldStatus`, `NewStatus`, `Note`, `ChangedBy` (FK User), `ChangedAt` | Lịch sử đổi trạng thái đơn hàng. |
| **InventoryTransactions**| `InventoryTransactionID` (`long`)| `ProductID` (FK), `TransactionType`, `Quantity`, `PreviousStock`, `NewStock`, `Note`, `CreatedBy` | Lịch sử kho: `Import`, `Export`, `Adjustment`, `CANCEL_ORDER`. |
| **RefreshTokens** | `RefreshTokenID` (`long`)| `UserID` (FK), `Token`, `ExpiresAt`, `RevokedAt` | Phục vụ làm mới phiên đăng nhập. |
| **PasswordResetTokens** | `PasswordResetTokenID` (`long`)| `UserID` (FK), `Token`, `ExpiresAt`, `UsedAt` | Token cấp khi quên mật khẩu (hạn 1 giờ). |

---

### 5.2. Danh sách toàn bộ API Endpoints chuẩn xác (Khớp 100% Code)

#### 🔐 1. Xác thực tài khoản (`api/auth` - `AuthController.cs`)
- `POST /api/auth/register`: Đăng ký tài khoản khách hàng mới.
- `POST /api/auth/login`: Đăng nhập, trả về JWT Token và thông tin User.
- `PUT /api/auth/change-password` `[Authorize]`: Đổi mật khẩu (kiểm tra mật khẩu cũ).
- `POST /api/auth/forgot-password`: Yêu cầu mã Token để reset mật khẩu.
- `POST /api/auth/reset-password`: Đặt lại mật khẩu mới kèm mã Token hợp lệ.

#### 📦 2. Danh mục & Sản phẩm (`api/products` & `api/categories`)
- `GET /api/products`: Tìm kiếm, lọc (keyword, categoryId, minPrice, maxPrice, brand), phân trang.
- `GET /api/products/{id}`: Chi tiết sản phẩm kèm Category, Brand, Images.
- `POST /api/products` `[Admin]`: Thêm mới sản phẩm.
- `PUT /api/products/{id}` `[Admin]`: Cập nhật thông tin sản phẩm.
- `DELETE /api/products/{id}` `[Admin]`: **Xóa mềm** sản phẩm (`IsActive = false`).
- `GET /api/categories`: Danh sách danh mục.
- `GET /api/categories/{id}`: Chi tiết danh mục.
- `POST /api/categories` `[Admin]`: Tạo danh mục mới.
- `PUT /api/categories/{id}` `[Admin]`: Sửa danh mục.
- `DELETE /api/categories/{id}` `[Admin]`: Xóa danh mục.

#### 🛒 3. Giỏ hàng trên Server (`api/carts` - `CartsController.cs`) `[Authorize]`
- `GET /api/carts`: Lấy giỏ hàng của user hiện tại (tự động tạo giỏ nếu chưa có).
- `POST /api/carts/items`: Thêm sản phẩm vào giỏ (kiểm tra tồn kho `StockQuantity`).
- `PUT /api/carts/items/{cartItemId}`: Cập nhật số lượng item (dùng `cartItemId: long`).
- `DELETE /api/carts/items/{cartItemId}`: Xóa 1 item khỏi giỏ hàng.
- `DELETE /api/carts`: Xóa sạch giỏ hàng.

#### 📋 4. Đơn hàng & Thanh toán (`api/orders` - `OrdersController.cs`) `[Authorize(Roles="Customer")]`
- `POST /api/orders`: Đặt hàng từ giỏ hàng (Chạy Database Transaction).
- `GET /api/orders`: Danh sách đơn hàng cá nhân (phân trang).
- `GET /api/orders/{id}`: Chi tiết đơn hàng của user.
- `GET /api/orders/{id}/mock-payment`: Lấy thông tin thanh toán QR giả lập.
- `POST /api/orders/{id}/mock-payment/confirm`: Xác nhận đã quét QR chuyển khoản (`PaymentStatus = Paid`).
- `POST /api/orders/{id}/cancel`: **Hủy đơn hàng** (HTTP POST, hoàn trả số lượng vào kho).

#### 📍 5. Sổ địa chỉ (`api/addresses` - `AddressesController.cs`) `[Authorize]`
- `GET /api/addresses`: Danh sách địa chỉ nhận hàng của user.
- `GET /api/addresses/{id}`: Chi tiết địa chỉ.
- `POST /api/addresses`: Thêm địa chỉ mới (nếu đánh dấu default sẽ tự bỏ default cũ).
- `PUT /api/addresses/{id}`: Sửa địa chỉ.
- `DELETE /api/addresses/{id}`: Xóa địa chỉ.

#### 👤 6. Hồ sơ cá nhân (`api/profile` - `ProfileController.cs`) `[Authorize]`
- `GET /api/profile`: Xem thông tin tài khoản hiện tại.
- `PUT /api/profile`: Cập nhật Họ tên, SĐT, Avatar URL.

#### 🛡️ 7. Quản trị hệ thống (`api/admin` - `AdminController.cs`) `[Authorize(Roles="Admin")]`
- `GET /api/admin/users`: Danh sách toàn bộ người dùng (phân trang, tìm kiếm keyword).

#### 🏭 8. Quản lý kho (`api/inventory` - `InventoryController.cs`) `[Authorize(Roles="Admin")]`
- `GET /api/inventory`: Báo cáo danh sách tồn kho theo sản phẩm & SKU.
- `PUT /api/inventory/{productId}`: Cập nhật tồn kho (Import / Export / Adjustment).
- `GET /api/inventory/{productId}/transactions`: Xem lịch sử biến động kho của sản phẩm.

#### 📊 9. Báo cáo & Thống kê (`api/reports` - `ReportsController.cs`) `[Authorize(Roles="Admin")]`
- `GET /api/reports/summary`: Thống kê Dashboard (Doanh thu, Tổng đơn, Tổng khách, Hàng sắp hết kho).
- `GET /api/reports/revenue`: Doanh thu theo từng ngày trong khoảng thời gian `from` → `to`.
- `GET /api/reports/top-products`: Danh sách Top N sản phẩm bán chạy nhất.

#### 🤖 10. Chatbot AI (`api/chat` - `ChatController.cs`)
- `POST /api/chat`: Tư vấn sản phẩm bằng tiếng Việt qua Gemini AI.

#### ❤️ 11. Kiểm tra trạng thái (`api/health` - `HealthController.cs`)
- `GET /api/health`: Health check endpoint kiểm tra API hoạt động.

---

### 5.3. Các luồng nghiệp vụ Backend trọng điểm

#### Luồng 1: Transactional Checkout (Đặt hàng an toàn & toàn vẹn)
```
Client gửi Request Checkout
   │
   ▼
1. Validate dữ liệu đầu vào (AddressID hợp lệ, PaymentMethod phải là COD hoặc QR).
2. Kiểm tra quyền sở hữu địa chỉ (thuộc đúng UserID đang đăng nhập).
3. Mở DATABASE TRANSACTION (context.Database.BeginTransactionAsync).
4. Kiểm tra giỏ hàng: Có item nào không? Có item nào hết hàng hoặc Quantity > StockQuantity không?
5. Snapshot dữ liệu: Copy ProductName, SKU, UnitPrice tại thời điểm đặt vào OrderDetail.
6. Tính tổng tiền SubTotal + ShippingFee hoàn toàn ở Server (chống gian lận sửa giá từ client).
7. Trừ kho: product.StockQuantity -= item.Quantity cho từng sản phẩm.
8. Ghi lịch sử kho: Thêm bản ghi InventoryTransaction (Loại: OrderReserve / Sale).
9. Xóa sạch giỏ hàng của khách (clear CartItems).
10. Ghi OrderStatusHistory (Trạng thái khởi tạo: PENDING).
   │
   ├── Thành công: COMMIT TRANSACTION -> Trả về HTTP 201 Created kèm OrderDetailDto.
   └── Có lỗi: ROLLBACK TRANSACTION -> Khôi phục dữ liệu nguyên trạng, ném Exception.
```

#### Luồng 2: Hủy đơn hàng và Chống hoàn kho lặp (Cancel Order Logic)
```
Client gọi POST /api/orders/{id}/cancel
   │
   ▼
1. Trích xuất UserID từ token JWT.
2. Tìm đơn hàng theo OrderID và UserID (chặn truy cập đơn của người khác - chống IDOR).
3. Kiểm tra trạng thái đơn:
   - NẾU đơn đã ở trạng thái CANCELLED, SHIPPING, hoặc COMPLETED:
     ==> Ném DomainConflictException (Chặn đứng việc hủy đơn và CHỐNG HOÀN KHO LẶP).
   - NẾU đơn đang PENDING: Được phép hủy.
4. Mở TRANSACTION:
   ├── Chuyển order.OrderStatus = "CANCELLED", ghi nhận CancelledAt = UtcNow.
   ├── Duyệt từng OrderDetail trong đơn:
   │    ├── product.StockQuantity += detail.Quantity (Hoàn lại kho đúng số lượng đã đặt).
   │    └── Thêm bản ghi InventoryTransactions (Loại: "CANCEL_ORDER", Note: Cancel order #ID).
   └── Commit Transaction và trả về đơn hàng đã hủy thành công.
```

---

### 5.4. Kỹ thuật lập trình & Design Patterns áp dụng

1. **Dependency Injection (IoC Container):**
   - Đăng ký toàn bộ Service theo vòng đời `Scoped` (`builder.Services.AddScoped<IOrderService, OrderService>()`).
   - Controllers chỉ phụ thuộc vào Interface, không phụ thuộc class cụ thể (Loose Coupling).
2. **DTO Pattern (Data Transfer Object):**
   - Phân tách hoàn toàn lớp Dữ liệu (Entity/Database) và lớp Trình diễn (API Response).
   - Không làm lộ mật khẩu, trường nhạy cảm, tránh lỗi Over-posting.
3. **Global Exception Handling Middleware:**
   - Class `ExceptionHandlingMiddleware` bắt mọi lỗi ngoại lệ phát sinh trong pipeline.
   - Chuẩn hóa thông báo lỗi về JSON chuẩn RFC 7807 (ProblemDetails), giấu stack trace trong môi trường Production để bảo mật.
4. **Auto Audit Fields (`SaveChangesAsync` Override):**
   - Override phương thức `SaveChangesAsync` trong `AppDbContext`.
   - Dùng `ChangeTracker` tự động quét các entity được thêm mới để gán `CreatedAt = DateTime.UtcNow`, và entity bị sửa đổi để gán `UpdatedAt = DateTime.UtcNow`.
5. **Data-Level Security (Chống lỗi IDOR / BOLA):**
   - Mọi thao tác Cart, Address, Order đều lấy `userId` trực tiếp từ `User.GetUserId()` (từ Claims Token), không bao giờ tin tưởng `userId` gửi từ body hoặc query string của Client.
6. **Soft Delete Pattern:**
   - Với sản phẩm (`Product`), khi xóa ta chỉ set `IsActive = false` để bảo vệ tính toàn vẹn dữ liệu cho các đơn hàng cũ đã từng chứa sản phẩm đó.

---

## 6. BỘ CÂU HỎI VẤN ĐÁP CHUẨN BỊ SẴN

### Câu 1: Em hãy giải thích quy trình đăng ký, đăng nhập và bảo mật với JWT trong dự án?
> **Trả lời:**
> - **Đăng ký:** Client gửi email, mật khẩu, họ tên. Server kiểm tra trùng email trong DB. Sau đó, mật khẩu được băm bằng thư viện `BCrypt.Net` với salt ngẫu nhiên rồi mới lưu vào `Users.PasswordHash`.
> - **Đăng nhập:** Server tìm User theo Email, dùng `BCrypt.Verify(password, user.PasswordHash)` để đối soát. Nếu đúng, Server tạo **JWT Token** bằng thuật toán HMAC-SHA256, ký bằng `Jwt:Key` bí mật.
> - **Claims trong Token:** Chứa `Sub` (UserID), `Email`, `Name`, `Role` ("Customer" hoặc "Admin").
> - **Ủy quyền:** Client đính kèm token vào Header `Authorization: Bearer <token>`. Tại Backend, middleware `JwtBearer` giải mã, kiểm tra hạn dùng và tính hợp lệ. Các action được phân quyền bằng `[Authorize]` hoặc `[Authorize(Roles = "Admin")]`.

### Câu 2: Tại sao trong hàm Checkout (Đặt hàng) em phải sử dụng Database Transaction?
> **Trả lời:**
> Quá trình đặt hàng gồm chuỗi nhiều hành động phụ thuộc lẫn nhau: (1) Kiểm tra giỏ hàng, (2) Kiểm tra tồn kho, (3) Tạo Order, (4) Tạo các OrderDetails, (5) Trừ tồn kho sản phẩm, (6) Ghi lịch sử kho, (7) Xóa giỏ hàng.
> Nếu không dùng Transaction, giả sử bước tạo Order thành công nhưng đến bước trừ kho gặp lỗi hệ thống, dữ liệu sẽ bị bất đồng bộ (khách có đơn mà kho không trừ, giỏ hàng vẫn còn). Việc dùng `await using var transaction = await context.Database.BeginTransactionAsync()` đảm bảo tính toàn vẹn dữ liệu **ACID**: **hoặc tất cả các bước thành công (Commit), hoặc không có gì thay đổi cả (Rollback)**.

### Câu 3: Em làm thế nào để đảm bảo khách hàng A không thể xem hoặc hủy đơn hàng của khách hàng B?
> **Trả lời:**
> Đây là vấn đề phòng chống lỗ hổng **IDOR (Insecure Direct Object Reference)**. Em áp dụng bảo mật 2 lớp:
> 1. Controller gắn `[Authorize(Roles = "Customer")]` bắt buộc phải có Token hợp lệ.
> 2. Trong hàm xử lý, em **không lấy UserId từ tham số Client gửi lên** mà trích xuất UserId từ Token đã ký của phiên đăng nhập (`User.GetUserId()`).
> 3. Trong câu lệnh truy vấn LINQ:  
>    `context.Orders.SingleOrDefaultAsync(x => x.OrderID == orderId && x.UserID == userId)`  
>    Nếu đơn hàng có tồn tại nhưng thuộc về người khác, câu lệnh trả về `null` và hệ thống ném ra `ResourceNotFoundException` ngay lập tức.

### Câu 4: Khi hủy đơn hàng, cơ chế hoàn kho và chống hoàn kho lặp hoạt động ra sao?
> **Trả lời:**
> - Hệ thống chỉ cho phép hủy khi đơn ở trạng thái `PENDING`. Nếu đơn đã ở trạng thái `SHIPPING`, `COMPLETED` hoặc `CANCELLED`, hệ thống lập tức chặn lại và ném lỗi `DomainConflictException`.
> - Nhờ kiểm tra `order.OrderStatus == OrderConstants.Cancelled` trước khi thực hiện, khách hàng không thể gọi API hủy đơn 2 lần liên tiếp để được hoàn tồn kho lặp lại.
> - Khi hủy hợp lệ, hệ thống mở Transaction: cập nhật trạng thái đơn thành `CANCELLED`, duyệt qua các `OrderDetails` để cộng trả lại `StockQuantity` cho từng `Product`, và ghi nhận một bản ghi `InventoryTransaction` có loại là `"CANCEL_ORDER"` để kiểm toán sau này.

### Câu 5: Tại sao trong bảng `OrderDetail` em lại lưu cả `ProductName`, `SKU`, `UnitPrice` mà không trỏ Foreign Key sang bảng `Product` để lấy?
> **Trả lời:**
> Đây là kỹ thuật **Snapshot Data** bắt buộc trong E-Commerce:
> Giá cả và tên của sản phẩm có thể thay đổi trong tương lai (ví dụ hôm nay bán 20 triệu, ngày mai tăng lên 25 triệu, hoặc sản phẩm bị đổi tên).
> Nếu chỉ lưu ProductID và join bảng, khi sản phẩm đổi giá thì giá trị của các đơn hàng trong quá khứ cũng bị tính sai lệch theo. Lưu snapshot giúp bảo toàn lịch sử hóa đơn tài chính chính xác tại đúng thời điểm khách hàng bấm đặt mua.

### Câu 6: Phân biệt Soft Delete và Hard Delete trong dự án của em?
> **Trả lời:**
> - **Soft Delete (Xóa mềm):** Áp dụng cho bảng `Products`. Khi gọi `DELETE /api/products/{id}`, em chỉ đặt cờ `product.IsActive = false`. Sản phẩm sẽ không xuất hiện trên giao diện bán hàng nữa, nhưng dữ liệu vẫn nằm trong database để các đơn hàng cũ tham chiếu tới mà không bị lỗi ràng buộc khóa ngoại.
> - **Hard Delete (Xóa cứng):** Áp dụng cho `CartItems` (khi khách xóa khỏi giỏ hoặc đã thanh toán xong) và `Addresses` (khi khách xóa một địa chỉ giao hàng không còn dùng).

### Câu 7: Dependency Injection trong .NET hoạt động như thế nào và em đăng ký các Service theo Lifetime nào?
> **Trả lời:**
> - Em sử dụng **Scoped Lifetime** (`AddScoped`) cho toàn bộ các Service (`IOrderService`, `IProductService`, `IAuthService`...) và `AppDbContext`.
> - **Scoped** có nghĩa là một instance mới của Service được tạo ra cho mỗi HTTP Request và được tái sử dụng trong suốt vòng đời của request đó, sau đó tự động giải phóng (Dispose) khi request kết thúc. Điều này đảm bảo an toàn luồng và đồng bộ trạng thái `DbContext` trong cùng một request.

---

## 7. HƯỚNG DẪN CHẠY DỰ ÁN

### 7.1. Khởi chạy Backend (.NET 10 API)
Mở PowerShell tại thư mục `backend/src/ECommerce.Api`:
```powershell
# 1. Khôi phục packages & Build
dotnet restore
dotnet build

# 2. Cấu hình chuỗi kết nối SQL Server & JWT Key qua User Secrets
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:ECommerce" "Server=localhost;Database=ECommerce;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "ElectroTech_Super_Secret_Jwt_Signing_Key_2026_AtLeast32Chars!"

# 3. Chạy Migration cập nhật Database
dotnet ef database update

# 4. Khởi chạy API
dotnet run --launch-profile http
```
- API Base URL: `http://localhost:5296`
- Swagger UI tài liệu: `http://localhost:5296/swagger`

### 7.2. Khởi chạy Frontend (React Vite)
Mở Terminal tại thư mục `frontend`:
```powershell
npm install
npm run dev
```
- Ứng dụng chạy tại: `http://localhost:5173`
