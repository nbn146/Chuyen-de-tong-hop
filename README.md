# Chuyên đề Tổng hợp — Website bán hàng trực tuyến

- [Phân công 5 vai trò và quy trình cập nhật Git](PHAN_CONG_CONG_VIEC.md)
- [Mẫu báo cáo tiến độ hằng tuần](docs/tien-do/MAU_BAO_CAO_TUAN.md)
- [Báo cáo tiến độ buổi 1](BAO_CAO_TIEN_DO_BUOI_1.md)

Mã nguồn kế thừa từ môn Quản lý dự án đã được nhập vào `backend/` và `frontend/`. Xem [biên bản tiếp nhận TL-01](docs/TL-01_BASELINE.md) để truy vết nguồn và kết quả kiểm tra. Các nhiệm vụ phát triển tiếp theo chưa được nghiệm thu.

## Chạy dự án

Yêu cầu: .NET 10 SDK, Node.js 24, SQL Server và công cụ `dotnet-ef` phiên bản 10.x để cập nhật database.

```powershell
git clone https://github.com/nbn146/Chuyen-de-tong-hop.git
Set-Location Chuyen-de-tong-hop
git switch toanLead
dotnet restore backend/ECommerce.slnx
npm --prefix frontend ci
```

Thiết lập cấu hình trên máy của bạn bằng User Secrets. Thay các giá trị trong dấu `<...>`; dùng database riêng cho việc học và kiểm thử.

```powershell
dotnet user-secrets set "ConnectionStrings:ECommerce" "<chuoi ket noi SQL Server toi database rieng>" --project backend/src/ECommerce.Api
dotnet user-secrets set "Jwt:Key" "<khoa ngau nhien rieng toi thieu 32 byte>" --project backend/src/ECommerce.Api
dotnet user-secrets set "BootstrapAdmin:Email" "admin@example.test" --project backend/src/ECommerce.Api
dotnet user-secrets set "BootstrapAdmin:Password" "<mat khau admin cuc bo>" --project backend/src/ECommerce.Api
dotnet user-secrets set "BootstrapAdmin:FullName" "Development Admin" --project backend/src/ECommerce.Api
# Tuy chon; khong co key thi chatbot dung fallback tim san pham.
dotnet user-secrets set "Gemini:ApiKey" "<API key cua ban>" --project backend/src/ECommerce.Api
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project backend/src/ECommerce.Api --startup-project backend/src/ECommerce.Api
dotnet run --project backend/src/ECommerce.Api --launch-profile http
```

Nếu chưa có `dotnet-ef`, cài một phiên bản 10.x phù hợp với EF Core của dự án trước khi chạy migration. UserSecretsId đã có trong csproj, không cần chạy `user-secrets init`. Không lưu các giá trị thật vào Git. Chỉ dùng BootstrapAdmin để tạo tài khoản Admin cục bộ, rồi có thể xóa cấu hình này bằng `dotnet user-secrets remove`.

Mở terminal thứ hai tại thư mục repo:

```powershell
Copy-Item frontend/.env.example frontend/.env.local
npm --prefix frontend run dev
```

Frontend: `http://localhost:5173`; API health: `http://localhost:5296/api/health`. Frontend dùng `/api` qua Vite proxy. Nếu port thay đổi, xem cấu hình `frontend/vite.config.ts` và `backend/src/ECommerce.Api/Properties/launchSettings.json`.

## Kiểm tra baseline

```powershell
dotnet build backend/ECommerce.slnx --configuration Release --no-restore
dotnet test backend/ECommerce.slnx --configuration Release --no-build --no-restore
npm --prefix frontend test
npm --prefix frontend run build
```

Các bài test không thay thế kiểm thử mua hàng trên SQL Server thật. Kết quả cụ thể và hạn chế được ghi trong biên bản TL-01.
