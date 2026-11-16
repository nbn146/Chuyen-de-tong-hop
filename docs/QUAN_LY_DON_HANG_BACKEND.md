# Backend quản lý đơn hàng Admin — phần mã nguồn kế thừa

Ngày cập nhật: 26/10/2026. Phụ trách tiếp nhận: Toản.

Nguồn: dự án QLDA, commit `32589220e70f348efeedc33c39d0786a430db285`, qua bản bàn giao `feature/TL-01-baseline`.

## Phạm vi giữ lại

- `Controllers/AdminController.cs`: chỉ giữ GET danh sách, GET chi tiết và PUT cập nhật trạng thái đơn hàng, yêu cầu vai trò Admin.
- `Services/Admin/AdminService.cs`, `IAdminService.cs`: truy vấn đơn hàng, lọc trạng thái, phân trang, cập nhật trạng thái và ghi lịch sử; bỏ phương thức quản lý người dùng.
- `DTOs/Admin/AdminDtos.cs`: DTO đơn hàng, chi tiết và kết quả phân trang; bỏ DTO người dùng.
- `DTOs/Orders/UpdateOrderStatusDto.cs`, `Domain/OrderConstants.cs`.
- Entity `Order`, `OrderDetail`, `OrderStatusHistory` và ba cấu hình EF Core tương ứng.

Đã gỡ Frontend, CI và các chức năng Backend khác khỏi bản hiện tại của nhánh `toanLead`. Commit nhập full code vẫn còn trong lịch sử Git; lần này dùng commit điều chỉnh, không viết lại lịch sử chung.

## Tích hợp vào backend chung

Đây là module nguồn, chưa phải ứng dụng chạy độc lập. Backend chung cần cung cấp:

- ASP.NET Core, EF Core, SQL Server và xác thực JWT với vai trò Admin.
- `AppDbContext` với tập Orders và cấu hình quan hệ; entity dùng chung `User`, `Product`.
- `ClaimsPrincipalExtensions.GetRequiredUserId()`; các exception `DomainValidationException`, `DomainConflictException`, `ResourceNotFoundException` và middleware xử lý lỗi.
- Đăng ký DI: `services.AddScoped<IAdminService, AdminService>();` cùng namespace tương ứng.
- Áp dụng ba cấu hình entity vào DbContext và dùng migration của backend chung; không tự chạy migration riêng từ module này.

## Giới hạn kế thừa cần xử lý tiếp

Logic hiện có kiểm tra trạng thái hợp lệ, không cho đổi từ trạng thái kết thúc hoặc đổi sang cùng trạng thái. Chưa kiểm soát đầy đủ thứ tự chuyển trạng thái theo BPMN; chưa có xử lý hoàn kho khi hủy. Không ghi nhận các phần này đã hoàn thành.

Kiểm tra lần thu gọn: rà soát phạm vi file và loại các phương thức quản lý người dùng. Kết quả build/test 90 Backend và 67 Frontend của lần nhập trước chỉ áp dụng bản đầy đủ, không chứng minh module riêng chạy được. Cần build/test lại sau khi tích hợp vào backend chung.