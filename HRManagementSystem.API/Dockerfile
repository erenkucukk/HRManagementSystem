# 1. Derleme için .NET SDK görüntüsü
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "HRManagementSystem.API.csproj" -c Release -o /app/publish

# 2. Çalýþtýrmak için .NET Runtime görüntüsü
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HRManagementSystem.API.dll"]