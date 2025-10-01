# ⚡ PRM-APP-Electric-Vehicle

## 📌 Giới thiệu

**PRM-APP-Electric-Vehicle** là một ứng dụng **.NET Web API** sử dụng **SQLite** làm cơ sở dữ liệu.
Dự án được phát triển theo kiến trúc **Microservice (đơn giản)** và hướng tới việc kết nối với **Flutter** (mobile app).

---

## 🚀 Công nghệ sử dụng

* **.NET 9.0** – Backend API
* **ASP.NET Core Web API** – Xây dựng REST API
* **Entity Framework Core (EF Core)** – ORM để giao tiếp database
* **SQLite** – Database lightweight, tự động tạo file `.db`

---

## 🧩 Các tính năng chính

* Kết nối SQLite (không cần cài thủ công, EF Core tự tạo database file)
* Quản lý dữ liệu xe điện (CRUD cơ bản)
* Tích hợp Swagger UI để test API trực tiếp
* Hỗ trợ kết nối dễ dàng với frontend (Flutter, React, Angular…)

---

## ⚙️ Cài đặt môi trường

Trước khi chạy, đảm bảo đã cài:

* [.NET SDK 9.0+](https://dotnet.microsoft.com/en-us/download)
* Visual Studio 2022 hoặc Visual Studio Code
* SQLite (được tích hợp sẵn trong EF Core, không bắt buộc cài ngoài)

Kiểm tra cài đặt:

```sh
dotnet --version
```

---

## ▶️ Cách chạy dự án

1. Clone repo về máy:

   ```sh
   git clone <repo-url>
   cd PRM-APP-Electric-Vehicle
   ```
2. Restore package:

   ```sh
   dotnet restore
   ```
3. Tạo database (EF Core migration):

   ```sh
   dotnet ef database update
   ```
4. Chạy project:

   ```sh
   dotnet run
   ```
5. Truy cập API tại:
   👉 `https://localhost:5001/swagger`

---

## 🧪 Kiểm thử API

* Dùng **Swagger UI** (có sẵn khi chạy app)
* Hoặc dùng **Postman** để gửi request CRUD
* API chính: `/api/vehicle` (quản lý xe điện)

---

## 📱 Kết nối với Flutter

* Flutter gọi API qua `http` package
* Base URL: `https://localhost:5001/api/`
* Có thể triển khai API này lên server để app mobile truy cập trực tiếp

---
