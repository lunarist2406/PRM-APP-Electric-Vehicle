# ======================
# 🚧 Build stage
# ======================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ======================
# 🚀 Runtime stage
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy publish folder from build stage
COPY --from=build /app/publish ./

# Expose port
EXPOSE 5003  # bạn có thể đổi theo port CompanyService

# ENTRYPOINT
ENTRYPOINT ["dotnet", "CompanyService.dll"]
