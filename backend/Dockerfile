# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore (leverages layer caching on the project files).
COPY Lms.sln ./
COPY src/Lms.Domain/Lms.Domain.csproj src/Lms.Domain/
COPY src/Lms.Application/Lms.Application.csproj src/Lms.Application/
COPY src/Lms.Infrastructure/Lms.Infrastructure.csproj src/Lms.Infrastructure/
COPY src/Lms.Api/Lms.Api.csproj src/Lms.Api/
RUN dotnet restore src/Lms.Api/Lms.Api.csproj

# Copy the rest and publish.
COPY src/ src/
RUN dotnet publish src/Lms.Api/Lms.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# The base image already listens on 8080; the entrypoint switches to $PORT when a managed
# host (Railway, Fly.io, Cloud Run, ...) injects one.
EXPOSE 8080
COPY --from=build /app/publish ./
COPY docker-entrypoint.sh ./
RUN chmod +x ./docker-entrypoint.sh
ENTRYPOINT ["./docker-entrypoint.sh"]
