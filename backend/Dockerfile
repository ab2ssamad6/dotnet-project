# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Lms.sln ./
COPY src/Lms.Domain/Lms.Domain.csproj src/Lms.Domain/
COPY src/Lms.Application/Lms.Application.csproj src/Lms.Application/
COPY src/Lms.Infrastructure/Lms.Infrastructure.csproj src/Lms.Infrastructure/
COPY src/Lms.Api/Lms.Api.csproj src/Lms.Api/
RUN dotnet restore src/Lms.Api/Lms.Api.csproj

COPY src/ src/
RUN dotnet publish src/Lms.Api/Lms.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish ./
COPY docker-entrypoint.sh ./
RUN chmod +x ./docker-entrypoint.sh
ENTRYPOINT ["./docker-entrypoint.sh"]
