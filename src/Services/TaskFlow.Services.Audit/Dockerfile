FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/TaskFlow.Services.Audit/TaskFlow.Services.Audit.csproj
RUN dotnet publish src/Services/TaskFlow.Services.Audit/TaskFlow.Services.Audit.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 5005 5010
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.Services.Audit.dll"]
