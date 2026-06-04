FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制解决方案文件（现在 .sln 在 src/ 根目录）
COPY ["Linkyou.System.sln", "."]

# 复制各层项目文件（利用 Docker 层缓存）
COPY ["Linkyou.System.Domain.Shared/Linkyou.System.Domain.Shared.csproj", "Linkyou.System.Domain.Shared/"]
COPY ["Linkyou.System.Domain/Linkyou.System.Domain.csproj", "Linkyou.System.Domain/"]
COPY ["Linkyou.System.Application.Contracts/Linkyou.System.Application.Contracts.csproj", "Linkyou.System.Application.Contracts/"]
COPY ["Linkyou.System.Application/Linkyou.System.Application.csproj", "Linkyou.System.Application/"]
COPY ["Linkyou.System.EntityFrameworkCore/Linkyou.System.EntityFrameworkCore.csproj", "Linkyou.System.EntityFrameworkCore/"]
COPY ["Linkyou.System.HttpApi/Linkyou.System.HttpApi.csproj", "Linkyou.System.HttpApi/"]
COPY ["Linkyou.System.HttpApi.Client/Linkyou.System.HttpApi.Client.csproj", "Linkyou.System.HttpApi.Client/"]
COPY ["Linkyou.System.Host/Linkyou.System.Host.csproj", "Linkyou.System.Host/"]

# 还原 NuGet 包
RUN dotnet restore "Linkyou.System.Host/Linkyou.System.Host.csproj"

# 复制所有源码
COPY . .

# 构建
WORKDIR "/src/Linkyou.System.Host"
RUN dotnet build "Linkyou.System.Host.csproj" -c Release -o /app/build

# 发布
FROM build AS publish
WORKDIR "/src/Linkyou.System.Host"
RUN dotnet publish "Linkyou.System.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 最终运行镜像
FROM base AS final
WORKDIR /app
RUN mkdir -p /app/Logs
COPY --from=publish /app/publish .
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
ENTRYPOINT ["dotnet", "Linkyou.System.Host.dll"]
