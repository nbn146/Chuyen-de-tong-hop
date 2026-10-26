# PHÂN CÔNG CÔNG VIỆC

## 1. Giới thiệu dự án

Đề tài: Website bán hàng trực tuyến.

Mục tiêu: Xây dựng website giúp khách hàng xem, tìm kiếm sản phẩm, quản lý giỏ hàng và đặt hàng; hỗ trợ quản trị viên quản lý sản phẩm, đơn hàng và tồn kho. Dự kiến bổ sung chatbot tư vấn sản phẩm.

Tài liệu môn Quản lý dự án (QLDA) được dùng để tham khảo yêu cầu và cách tổ chức dự án. Phạm vi triển khai sẽ được nhóm thống nhất theo yêu cầu của môn Chuyên đề Tổng hợp.

## 2. Thành viên và phân công

| Thành viên | Vai trò | Công việc phụ trách |
|---|---|---|
| Toản | Team leader | Xác định phạm vi, lập kế hoạch, phân công, quản lý Git và tổng hợp báo cáo tiến độ |
| Anh Quang | Frontend | Xây dựng giao diện khách hàng, quản trị và kết nối API |
| Đăng Quang | Backend | Thiết kế cơ sở dữ liệu, xây dựng API và xử lý nghiệp vụ bán hàng |
| Bình | Test CI/CD | Lập kịch bản kiểm thử, ghi nhận lỗi và thiết lập quy trình build/test tự động |
| Bá Nam | Backend AI | Tìm hiểu và xây dựng chatbot tư vấn sản phẩm, phối hợp tích hợp với Backend và Frontend |

## 3. Kế hoạch thực hiện

| Giai đoạn | Công việc dự kiến | Phụ trách |
|---|---|---|
| 1. Chuẩn bị | Thống nhất đề tài, yêu cầu, phân công và cách sử dụng Git | Toản và cả nhóm |
| 2. Thiết kế | Thiết kế giao diện, cơ sở dữ liệu, API và phương án chatbot | Anh Quang, Đăng Quang, Bá Nam; Bình chuẩn bị kịch bản kiểm thử |
| 3. Triển khai | Xây dựng các chức năng, tích hợp và kiểm thử từng phần | Cả nhóm theo vai trò |
| 4. Hoàn thiện | Sửa lỗi, kiểm thử tổng thể, chuẩn bị demo và báo cáo | Cả nhóm; Toản tổng hợp |

Thời gian cụ thể sẽ được cập nhật theo lịch môn học và thống nhất của nhóm.

## 4. Công việc ban đầu của Team leader — TL-01

- Giới thiệu đề tài và mục tiêu dự án.
- Chốt danh sách thành viên và vai trò.
- Lập bảng phân công và kế hoạch ban đầu.
- Tạo nhánh `toanLead`, đưa tài liệu quản lý lên GitHub để giáo viên theo dõi.

## 5. Tiến độ hiện tại

- Đã thống nhất phân công 5 thành viên.
- Đã lập kế hoạch ban đầu và đưa tài liệu Team leader lên nhánh `toanLead`.
- Ngày 26/10/2026: tiếp nhận mã nguồn QLDA (Frontend, Backend, CI) từ nhánh `feature/TL-01-baseline`, commit nguồn `32589220e70f348efeedc33c39d0786a430db285`. Đây là mã nguồn kế thừa, chưa nghiệm thu chức năng cho môn học mới. Xem `docs/TL-01_BASELINE.md`.
- Kết quả phân tích và biểu đồ buổi 1 được giữ trong báo cáo riêng; không coi đó là các chức năng phần mềm đã hoàn thành.

## 6. Cập nhật tiến độ trên Git

Mỗi thành viên làm việc trên nhánh riêng và commit bằng tài khoản của mình. Khi có thay đổi, push lên GitHub, ghi rõ nội dung công việc và mở Pull Request để nhóm kiểm tra trước khi tích hợp.

Cuối mỗi tuần, nhóm cập nhật công việc đã làm, việc chưa hoàn thành và kế hoạch tiếp theo theo [mẫu báo cáo tuần](docs/tien-do/MAU_BAO_CAO_TUAN.md). Báo cáo chỉ ghi kết quả thực tế, kèm link commit hoặc Pull Request khi có.
