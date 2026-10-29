# cicadar-api - Docker 镜像（多阶段：SDK 发布 → ASP.NET 运行）
# 目标框架与 global.json / cicadar-api.csproj 的 net9.0 对齐

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY cicadar-api.sln cicadar-api.csproj global.json ./
RUN dotnet restore cicadar-api.csproj

COPY . .
RUN dotnet publish cicadar-api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "cicadar-api.dll"]
