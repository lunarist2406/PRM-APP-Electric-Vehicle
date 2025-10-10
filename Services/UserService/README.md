# UserService

Identity / User Service cho hệ thống EV microservice.  
Dùng MongoDB + .NET 7 + JWT + BCrypt password hash + Swagger UI.

---

## 1️⃣ Yêu cầu

- .NET 7 SDK
- MongoDB (local hoặc Docker)
- Visual Studio / VS Code / dotnet CLI

---

## 2️⃣ Cài đặt MongoDB local

Nếu chưa cài MongoDB, bạn có thể dùng Docker:

```bash
docker run -d -p 27017:27017 --name mongodb mongo:6
cd UserService

# Tạo webapi project
dotnet new webapi -n UserService
cd UserService

# Cài MongoDB + JWT + BCrypt
dotnet add package MongoDB.Driver
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.IdentityModel.Tokens
dotnet add package BCrypt.Net-Next

# Cài Swagger để test API
dotnet add package Swashbuckle.AspNetCore