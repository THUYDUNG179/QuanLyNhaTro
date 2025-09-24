# 🏠 Hệ Thống Quản Lý Nhà Trọ

Website hỗ trợ chủ trọ quản lý thông tin nhà trọ, phòng, hợp đồng thuê, hóa đơn, thông báo và người thuê một cách hiệu quả.  
Xây dựng bằng **ASP.NET Core MVC**, **Entity Framework Core** và **SQL Server**.

---

## 🚀 Tính năng chính
- Quản lý người thuê (Tenant)
- Quản lý phòng trọ
- Quản lý hợp đồng thuê phòng
- Quản lý hóa đơn tiền phòng, tiền điện nước
- Quản lý sự cố / thông báo cho người thuê
- Quản lý tài khoản (Chủ trọ, Quản lý, Người thuê)

---

# Hướng dẫn chạy dự án bằng Visual Studio

## 1. Yêu cầu môi trường
- Visual Studio 2022 (hoặc mới hơn)  
  - Khi cài đặt chọn workload:
    - **ASP.NET and web development** (nếu dự án là Web)  
    - **.NET desktop development** (nếu dự án là WinForms/WPF)  
- .NET SDK phù hợp với version của dự án (ví dụ `.NET 6.0` hoặc `.NET 8.0`)  
- SQL Server + SQL Server Management Studio (SSMS)  

---

## 2. Tải và mở dự án
1. Tải code từ link được cung cấp.  
2. Giải nén file `.zip` (nếu có).  
3. Mở Visual Studio → **File** → **Open** → **Project/Solution**.  
4. Chọn file `.sln` (solution) trong thư mục gốc của dự án.  

---

## 3. Chạy dự án
1. Mở **SQL Server Management Studio (SSMS)**.  
2. Kết nối đến SQL Server của bạn.  
3. Mở file `QLyNhaTro.sql` (nằm trong thư mục dự án, cuối cùng).  
4. Chạy toàn bộ script để tạo database và các bảng.  
5. Mở file `appsettings.json` (VS Studio)   
6. Chỉnh chuỗi kết nối (`ConnectionStrings`) cho đúng tên server + database, ví dụ:  
---
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=DESKTOP-ABC123\\SQLEXPRESS;Database=QuanLyXYZ;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
---
### 4. Chạy dự án
Nhấn Ctrl + F5 hoặc bấm nút ▶️ Start Without Debugging để chạy web.
 
