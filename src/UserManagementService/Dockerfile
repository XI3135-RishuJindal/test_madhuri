# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["UserManagementService/UserManagementService.csproj", "UserManagementService/"]
RUN dotnet restore "UserManagementService/UserManagementService.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/UserManagementService"
RUN dotnet build "UserManagementService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "UserManagementService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy published app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "UserManagementService.dll"]
