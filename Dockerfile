# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ArabClinic.sln", "Directory.Build.props", "./"]
COPY ["src/ArabClinic.Web/ArabClinic.Web.csproj", "src/ArabClinic.Web/"]
COPY ["src/ArabClinic.Application/ArabClinic.Application.csproj", "src/ArabClinic.Application/"]
COPY ["src/ArabClinic.Domain/ArabClinic.Domain.csproj", "src/ArabClinic.Domain/"]
COPY ["src/ArabClinic.Infrastructure/ArabClinic.Infrastructure.csproj", "src/ArabClinic.Infrastructure/"]
COPY ["tests/ArabClinic.UnitTests/ArabClinic.UnitTests.csproj", "tests/ArabClinic.UnitTests/"]
RUN dotnet restore ArabClinic.sln --locked-mode

COPY . .
RUN dotnet publish src/ArabClinic.Web/ArabClinic.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ArabClinic.Web.dll"]
