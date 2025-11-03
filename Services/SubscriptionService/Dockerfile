FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Services/SubscriptionService/SubscriptionService.csproj", "Services/SubscriptionService/"]
RUN dotnet restore "Services/SubscriptionService/SubscriptionService.csproj"

COPY . .
WORKDIR "/src/Services/SubscriptionService"
RUN dotnet build "SubscriptionService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SubscriptionService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubscriptionService.dll"]

