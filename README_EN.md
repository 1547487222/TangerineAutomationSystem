# Tangerine Automation System - Frontend-Backend Separation Deployment Guide

## Project Architecture

This project adopts a frontend-backend separation architecture:

- **Frontend**: TangerineBlazorApp (Blazor WebAssembly)
- **Backend**: gRPC Service

## Configuration

### Frontend Configuration

The backend service URL configuration file is located at `TangerineBlazorApp/wwwroot/appsettings.json`

#### Development Environment (appsettings.Development.json)
```json
{
  "BackendApi": {
    "GrpcEndpoint": "https://localhost:7197"
  }
}
```

#### Production Environment (appsettings.Production.json)
```json
{
  "BackendApi": {
    "GrpcEndpoint": "https://api.example.com"
  }
}
```

**Note**: Please modify the `GrpcEndpoint` address according to your actual deployment environment.

## Running in Development

### Run Frontend
```bash
cd TangerineBlazorApp
dotnet run
```

The frontend will start at http://localhost:5212

### Run Backend
The backend gRPC service needs to start at https://localhost:7197 (or modify the address in the frontend configuration file)

## Production Deployment

### Frontend Deployment

1. Modify the production configuration file:
   ```bash
   # Edit TangerineBlazorApp/wwwroot/appsettings.Production.json
   # Set GrpcEndpoint to your actual backend service address
   ```

2. Publish the frontend application:
   ```bash
   cd TangerineBlazorApp
   dotnet publish -c Release -o ./publish
   ```

3. Deploy to web server:
   - Deploy all files from the `publish/wwwroot` directory to your web server
   - Can use Nginx, IIS, or any static file hosting service

### Backend Deployment

The backend gRPC service requires:
1. Configure CORS to allow frontend domain access
2. Configure HTTPS certificates
3. Ensure gRPC-Web support is enabled

### Nginx Configuration Example

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

## Configuration Management Best Practices

1. **Development**: Use `appsettings.Development.json` for local backend address
2. **Staging**: Create `appsettings.Staging.json` for staging environment address
3. **Production**: Use `appsettings.Production.json` for production environment address

## Troubleshooting

### Frontend Cannot Connect to Backend

1. Check if the `GrpcEndpoint` in `appsettings.json` is correct
2. Verify the backend service is running properly
3. Check network firewall settings
4. Verify CORS configuration is correct

### gRPC Connection Errors

1. Ensure backend has gRPC-Web support enabled
2. Check if HTTPS certificate is valid
3. Verify backend Kestrel configuration supports HTTP/2

## Tech Stack

- Frontend: Blazor WebAssembly + AntDesign
- Communication: gRPC-Web
- Backend: ASP.NET Core gRPC
