# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and restore dependencies
COPY *.sln ./
COPY src/WhereIsTheTrain.API/*.csproj ./src/WhereIsTheTrain.API/
COPY src/WhereIsTheTrain.Application/*.csproj ./src/WhereIsTheTrain.Application/
COPY src/WhereIsTheTrain.Domain/*.csproj ./src/WhereIsTheTrain.Domain/
COPY src/WhereIsTheTrain.Infrastructure/*.csproj ./src/WhereIsTheTrain.Infrastructure/
RUN dotnet restore

# Copy the remaining source code and publish the app
COPY src/ ./src/
RUN dotnet publish src/WhereIsTheTrain.API/WhereIsTheTrain.API.csproj -c Release -o /app/publish

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Expose port 8080 (the default for .NET 8 in Docker)
EXPOSE 8080

# Environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Create a directory for the SQLite database so it can be persisted using a Docker volume
RUN mkdir /app/data

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Define the entry point for the application
ENTRYPOINT ["dotnet", "WhereIsTheTrain.API.dll"]
