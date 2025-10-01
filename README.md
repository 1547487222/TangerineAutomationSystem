# Tangerine Automation System - 前后端分离部署指南

## 项目架构

本项目采用前后端分离架构:

- **前端**: TangerineBlazorApp (Blazor WebAssembly)
- **后端**: gRPC 服务

## 配置说明

### 前端配置

前端应用的后端服务地址配置文件位于 `TangerineBlazorApp/wwwroot/appsettings.json`

#### 开发环境配置 (appsettings.Development.json)
```json
{
  "BackendApi": {
    "GrpcEndpoint": "https://localhost:7197"
  }
}
```

#### 生产环境配置 (appsettings.Production.json)
```json
{
  "BackendApi": {
    "GrpcEndpoint": "https://api.example.com"
  }
}
```

**注意**: 请根据实际部署环境修改 `GrpcEndpoint` 地址。

## 开发环境运行

### 运行前端
```bash
cd TangerineBlazorApp
dotnet run
```

前端将在 http://localhost:5212 启动

### 运行后端
后端 gRPC 服务需要在 https://localhost:7197 启动（或修改前端配置文件中的地址）

## 生产环境部署

### 前端部署

1. 修改生产环境配置文件:
   ```bash
   # 编辑 TangerineBlazorApp/wwwroot/appsettings.Production.json
   # 将 GrpcEndpoint 设置为实际的后端服务地址
   ```

2. 发布前端应用:
   ```bash
   cd TangerineBlazorApp
   dotnet publish -c Release -o ./publish
   ```

3. 部署到Web服务器:
   - 将 `publish/wwwroot` 目录下的所有文件部署到Web服务器
   - 可以使用 Nginx, IIS, 或任何静态文件托管服务

### 后端部署

后端 gRPC 服务需要:
1. 配置 CORS 允许前端域名访问
2. 配置 HTTPS 证书
3. 确保 gRPC-Web 支持已启用

### Nginx 配置示例

```nginx
server {
    listen 80;
    server_name your-frontend-domain.com;
    
    root /var/www/tangerine-frontend;
    index index.html;
    
    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

## 配置管理最佳实践

1. **开发环境**: 使用 `appsettings.Development.json` 配置本地后端地址
2. **测试环境**: 创建 `appsettings.Staging.json` 配置测试环境地址
3. **生产环境**: 使用 `appsettings.Production.json` 配置生产环境地址

## 故障排查

### 前端无法连接后端

1. 检查 `appsettings.json` 中的 `GrpcEndpoint` 是否正确
2. 确认后端服务是否正常运行
3. 检查网络防火墙设置
4. 验证 CORS 配置是否正确

### gRPC 连接错误

1. 确保后端启用了 gRPC-Web 支持
2. 检查 HTTPS 证书是否有效
3. 验证后端的 Kestrel 配置是否支持 HTTP/2

## 技术栈

- 前端: Blazor WebAssembly + AntDesign
- 通信协议: gRPC-Web
- 后端: ASP.NET Core gRPC
