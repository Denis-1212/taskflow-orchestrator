FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Services/TaskFlow.Services.Notification/TaskFlow.Services.Notification.csproj
RUN dotnet publish src/Services/TaskFlow.Services.Notification/TaskFlow.Services.Notification.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 5004 5009
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.Services.Notification.dll"]