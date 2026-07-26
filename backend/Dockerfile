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
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Lms.Api.dll"]
