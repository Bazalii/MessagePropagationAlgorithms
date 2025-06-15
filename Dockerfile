# Стадия сборки
FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /publish

# Стадия выполнения
FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "DistributedSystemsProject.dll"]