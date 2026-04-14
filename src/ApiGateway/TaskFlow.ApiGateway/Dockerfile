FROM node:20-alpine AS client-builder
WORKDIR /client
COPY client/taskflow-orchestrator-frontend/package*.json ./
RUN npm ci
COPY client/taskflow-orchestrator-frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/ApiGateway/TaskFlow.ApiGateway/TaskFlow.ApiGateway.csproj
RUN dotnet publish src/ApiGateway/TaskFlow.ApiGateway/TaskFlow.ApiGateway.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
COPY --from=client-builder /client/dist ./wwwroot

ENTRYPOINT ["dotnet", "TaskFlow.ApiGateway.dll"]