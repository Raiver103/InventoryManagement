# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем файлы проекта
COPY InventoryManagement.sln ./
COPY InventoryManagement.WEB/ InventoryManagement.WEB/
COPY InventoryManagement.Application/ InventoryManagement.Application/
COPY InventoryManagement.Domain/ InventoryManagement.Domain/
COPY InventoryManagement.Infrastructure/ InventoryManagement.Infrastructure/
COPY InventoryManagement.Tests/ InventoryManagement.Tests/ 

# Восстанавливаем зависимости
RUN dotnet restore

# Собираем проект
RUN dotnet publish InventoryManagement.WEB/InventoryManagement.WEB.csproj -c Release -o out

# Используем более легкий .NET runtime для финального контейнера
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем собранное приложение
COPY --from=build /app/out ./

# Запускаем API
ENTRYPOINT ["dotnet", "InventoryManagement.WEB.dll"]