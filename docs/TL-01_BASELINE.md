# TL-01 — Tiếp nhận baseline QLDA

- Phụ trách: Toản — Team leader.
- Ngày tiếp nhận: 15/10/2026.
- Nguồn: https://github.com/truongtoanr0602/Quan_Ly_Du_An
- Commit nguồn: `32589220e70f348efeedc33c39d0786a430db285`.
- Nhánh bàn giao: `feature/TL-01-baseline` tại repo https://github.com/nbn146/Chuyen-de-tong-hop.

## Phạm vi tiếp nhận

Nhập snapshot đã commit gồm `backend/`, `frontend/`, `.github/`, `.editorconfig`, `.gitignore` và tài liệu kỹ thuật: architecture, database ERD, coding conventions, git workflow, Sprint1/2, Sprint backlog và acceptance evidence. Giữ nguyên file thiết kế UI trong frontend vì thuộc snapshot nguồn. Giữ nguyên biểu đồ, báo cáo buổi 1 và bổ sung phân công 5 thành viên cùng mẫu báo cáo tuần đã thống nhất.

Không nhập `.git`, `.worktrees`, dependency cài sẵn, build output, bản đóng gói/video, cấu hình máy cá nhân và thay đổi chưa commit tại QLDA. Các thay đổi cục bộ ở HealthController, appsettings, package-lock và HomePage của QLDA không thuộc snapshot này và vẫn giữ nguyên tại nguồn.

Đây là mã nguồn kế thừa, không ghi nhận thành chức năng mới do Toản hoặc các thành viên vừa phát triển. Tài liệu Sprint cũ giữ vai trò lịch sử; phân công hiện hành xem `PHAN_CONG_CONG_VIEC.md`.

## Điều chỉnh khi tiếp nhận

- Xóa giá trị connection string và JWT signing key khỏi `appsettings.json`; yêu cầu cấu hình bằng User Secrets hoặc biến môi trường.
- Loại `appsettings.Development.json` của nguồn, bổ sung ignore để tránh đưa cấu hình máy cá nhân lên Git.
- README mới mô tả cách cài, cấu hình, migration, chạy và kiểm tra repo môn học.
- Không thay đổi API, schema, migration hay logic nghiệp vụ. Workflow CI được kế thừa; cấu hình branch protection và triển khai thuộc QA-02/QA-03.

## Kiểm tra

Môi trường: Windows, .NET SDK 10.0.402, Node.js v24.19.0.

| Lệnh | Kết quả |
|---|---|
| `dotnet restore backend/ECommerce.slnx` | Thành công |
| `dotnet build backend/ECommerce.slnx --configuration Release --no-restore` | Thành công, 0 lỗi |
| `dotnet test backend/ECommerce.slnx --configuration Release --no-build --no-restore` | 90/90 test đạt |
| `npm --prefix frontend ci` | Thành công |
| `npm --prefix frontend test` | 67/67 test đạt, 27 file test |
| `npm --prefix frontend run build` | Thành công |

Cảnh báo kế thừa: NuGet NU1902/NU1903 cho `System.Security.Cryptography.Pkcs` 9.0.11; npm báo 2 lỗ hổng mức high khi cài dependency. Chưa thay dependency trong TL-01; chuyển QA-01 lập danh sách và phối hợp người phụ trách xử lý.

Đã kiểm tra cấu hình JSON để bảo đảm không có giá trị JWT key, Gemini key hoặc connection string trong bản bàn giao; kiểm tra mẫu token phổ biến không có kết quả. Kiểm thử tích hợp với SQL Server, luồng mua hàng trên trình duyệt và gọi provider AI thật chưa thực hiện trong TL-01.

`git diff --cached --check` trên toàn snapshot báo khoảng trắng cuối dòng/dòng trống cuối file có sẵn từ nguồn. Giữ nguyên định dạng baseline để dễ đối chiếu; kiểm tra riêng các tài liệu mới và `.gitignore` đạt. Không đưa dependency, output build, file cấu hình cá nhân hoặc file trên 50 MB vào commit.

## Nghiệm thu

TL-01 được push để review. Chỉ chuyển Done sau khi PR được review/merge và nhóm xác nhận tiêu chí nghiệm thu. Không tự ghi nhận các chức năng kế thừa đã được nghiệm thu trong môn học mới.

## Đưa baseline vào nhánh toanLead — 26/10/2026

Tiếp nhận lại snapshot từ `feature/TL-01-baseline` vào `toanLead` theo yêu cầu. Đã đối chiếu nội dung backend, frontend, workflow CI và cấu hình Git trùng bản bàn giao. README chuyển hướng dẫn sang nhánh `toanLead`; bảng phân công cập nhật trạng thái tiếp nhận.

Kiểm tra lại ngày 26/10/2026:
- `dotnet build backend/ECommerce.slnx --configuration Release`: thành công, 0 lỗi; còn cảnh báo NU1902/NU1903 đã nêu ở trên.
- `dotnet test backend/ECommerce.slnx --configuration Release --no-build --no-restore`: 90/90 test đạt.
- `npm --prefix frontend run build`: thành công.
- `npm --prefix frontend test`: 67/67 test đạt, 27 file test.

Chưa chạy thử với SQL Server hoặc chụp minh chứng giao diện trong lần tiếp nhận này. Đây là công việc bàn giao mã nguồn, chưa phải báo cáo kết quả phát triển mới sau 3 tuần.
