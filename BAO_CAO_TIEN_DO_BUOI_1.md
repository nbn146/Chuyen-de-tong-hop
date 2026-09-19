# BÁO CÁO TIẾN ĐỘ BUỔI 1

## 1. Thông tin chung

- **Đề tài:** Website bán hàng trực tuyến (E-Commerce MVP)
- **Nội dung thực hiện:** Phân tích kiến trúc hệ thống và xây dựng các biểu đồ mô hình hóa nghiệp vụ
- **Thời điểm báo cáo:** 19/09/2026
- **Căn cứ thực hiện:** Tài liệu `project_overview.md` và các sản phẩm trong thư mục `Biều đồ`

## 2. Mục tiêu buổi 1

Trong buổi 1, nhóm tập trung nghiên cứu tài liệu tổng quan dự án, thống nhất phạm vi hệ thống, nhận diện các tác nhân và nghiệp vụ chính, sau đó phân chia công việc vẽ biểu đồ cho từng thành viên. Các biểu đồ được xây dựng nhằm mô tả cả góc nhìn chức năng, quy trình nghiệp vụ và luồng tương tác kỹ thuật của website bán hàng trực tuyến.

## 3. Kiến trúc hệ thống đã thống nhất

Hệ thống sử dụng kiến trúc **Client–Server**, giao tiếp qua **REST API** và dữ liệu JSON:

```text
React + TypeScript + Vite
          │
          │ HTTP/JSON + JWT Bearer
          ▼
ASP.NET Core Web API
          │
          ├── Controller: tiếp nhận và phản hồi HTTP
          ├── Service: xử lý nghiệp vụ, ánh xạ Entity/DTO
          └── Entity Framework Core / AppDbContext
                              │
                              ▼
                     Microsoft SQL Server
```

Các nguyên tắc kiến trúc chính:

- Frontend không truy cập trực tiếp cơ sở dữ liệu.
- Controller chỉ xử lý các vấn đề liên quan đến HTTP; nghiệp vụ được đặt trong Service.
- Backend sử dụng DTO để trao đổi dữ liệu, hạn chế trả trực tiếp Entity.
- JWT được sử dụng để xác thực và phân quyền Customer/Admin.
- Các thay đổi dữ liệu được quản lý qua Entity Framework Core và SQL Server.
- Nghiệp vụ chính gồm xác thực, danh mục/sản phẩm, giỏ hàng, đặt hàng, quản lý đơn hàng, tồn kho và báo cáo.

## 4. Phân chia công việc

| Thành viên | Công việc được phân công | Sản phẩm dự kiến |
|---|---|---|
| Bá Nam | Use Case tổng quan hệ thống; BFD; BPMN quy trình mua hàng | Use Case gồm 25 ca sử dụng và 3 tác nhân; BFD hệ thống; quy trình duyệt sản phẩm → giỏ hàng → checkout → đặt hàng |
| Toản | Use Case phân hệ Admin; BFD; BPMN quản lý đơn hàng | 7 Use Case chi tiết của Admin; BFD phân hệ; quy trình xem đơn → cập nhật trạng thái → ghi lịch sử |
| Đăng Quang | BPMN đăng ký và đăng nhập | Hai luồng đăng ký tài khoản và đăng nhập hệ thống |
| Quang | BPMN quản lý tồn kho | Luồng nhập hàng, điều chỉnh tồn kho và xem lịch sử giao dịch kho |
| An Bình | Use Case phân hệ Customer với một số ca sử dụng tiêu biểu | Các nghiệp vụ tìm kiếm/xem sản phẩm, giỏ hàng, đặt hàng, xem và hủy đơn |

## 5. Kết quả thực hiện

### 5.1. Sản phẩm đã có

| Thành viên | Tệp sản phẩm | Nội dung ghi nhận | Trạng thái |
|---|---|---|---|
| Toản | `Toản/Biểu đồ UseCase Admin.png` | Mô tả 7 nghiệp vụ Admin: danh mục, sản phẩm, đơn hàng, tồn kho, người dùng, báo cáo và dashboard | Đã có |
| Toản | `Toản/BPMN Quy trình quản lý đơn hàng.png` | Thể hiện các lane Admin, React Frontend, Backend API và Database; có kiểm tra chuyển trạng thái, hoàn kho khi hủy và ghi `OrderStatusHistory` | Đã có |
| Đăng Quang | `ĐQuang/bpmn-registration.svg` | Quy trình đăng ký tài khoản | Đã có |
| Đăng Quang | `ĐQuang/bpmn-login.svg` | Quy trình đăng nhập và xác thực người dùng | Đã có |
| Quang | `Quang/BPMN_QuyTrinhQuanLyTonKho.png` | Mô tả nhập hàng, điều chỉnh tồn kho, xem lịch sử, cập nhật số lượng và lưu lịch sử giao dịch | Đã có |
| An Bình | `Bình/01-use-case-customer.png` | Sáu nhóm Use Case tiêu biểu của Customer và các quan hệ bao gồm trong đặt hàng COD | Đã có |
| An Bình | `Bình/02-bpmn-dat-hang-cod.png` | Quy trình đặt hàng COD giữa Customer và hệ thống, gồm kiểm tra giỏ/địa chỉ/tồn kho, tạo đơn, trừ kho và rollback khi lỗi | Bổ sung ngoài phân công |
| An Bình | `Bình/03-sequence-dat-hang-cod.png` | Luồng tương tác Customer → React UI → Controller → Service → EF Core → SQL Server khi đặt hàng | Bổ sung ngoài phân công |

### 5.2. Sản phẩm chưa ghi nhận trong thư mục biểu đồ

- Chưa thấy Use Case tổng quan hệ thống, BFD và BPMN mua hàng của Bá Nam.
- Chưa thấy tệp BFD của Toản.
- Tên thư mục `Bình` nên được thống nhất thành `An Bình` hoặc tên quy ước chung của nhóm trong lần cập nhật tiếp theo.

## 6. Đánh giá tiến độ

- Nhóm đã hoàn thành bước nghiên cứu kiến trúc và xác định đúng ba tác nhân chính: **Guest**, **Customer** và **Admin**.
- Các quy trình trọng tâm đã được mô hình hóa gồm đăng ký/đăng nhập, đặt hàng COD, quản lý đơn hàng và quản lý tồn kho.
- Biểu đồ của Toản bám sát kiến trúc phân lớp và dữ liệu `OrderStatusHistory`; biểu đồ của Quang thể hiện rõ việc tạo `InventoryTransaction` và cập nhật `StockQuantity`.
- Biểu đồ của An Bình liên kết được nghiệp vụ Customer với luồng kỹ thuật `OrdersController` → `OrderService` → `AppDbContext` → SQL Server.
- Tiến độ hiện tại chưa đủ toàn bộ sản phẩm theo phân công vì còn thiếu nhóm biểu đồ của Bá Nam và các BFD.

## 7. Kế hoạch buổi tiếp theo

1. Bổ sung các biểu đồ còn thiếu: Use Case tổng quan, BPMN mua hàng và các BFD đã phân công.
2. Rà soát ký pháp UML/BPMN, tên tác nhân, tên Use Case và chiều mũi tên giữa các biểu đồ.
3. Thống nhất thuật ngữ trạng thái đơn hàng: `PENDING`, `CONFIRMED`, `SHIPPING`, `COMPLETED`, `CANCELLED`.
4. Kiểm tra tính nhất quán giữa biểu đồ với REST API, Service, Entity và cơ sở dữ liệu trong tài liệu tổng quan.
5. Chuẩn hóa tên thư mục, tên tệp và định dạng xuất ảnh để thuận tiện tổng hợp vào báo cáo cuối kỳ.

## 8. Kết luận

Buổi 1 đã hoàn thành việc thống nhất kiến trúc, phân chia nhiệm vụ và tạo phần lớn các biểu đồ nghiệp vụ quan trọng. Sản phẩm hiện có đã thể hiện được mối liên hệ giữa người dùng, giao diện React, ASP.NET Core Web API và SQL Server. Nhóm cần tiếp tục bổ sung các biểu đồ còn thiếu và chuẩn hóa hình thức trình bày trước khi nghiệm thu bộ tài liệu mô hình hóa.
