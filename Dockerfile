# Etapa 1: restaura y publica la API.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj, para que Docker cachee el restore mientras no cambien las dependencias.
COPY Magazing.sln ./
COPY Magazing.Api/Magazing.Api.csproj Magazing.Api/
COPY Magazing.Application/Magazing.Application.csproj Magazing.Application/
COPY Magazing.Domain/Magazing.Domain.csproj Magazing.Domain/
COPY Magazing.Infrastructure/Magazing.Infrastructure.csproj Magazing.Infrastructure/
RUN dotnet restore Magazing.Api/Magazing.Api.csproj

COPY . .
RUN dotnet publish Magazing.Api/Magazing.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Etapa 2: imagen liviana solo con el runtime de ASP.NET.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# QuestPDF (comprobantes PDF) usa SkiaSharp, que en Linux necesita fontconfig.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    PORT=8080
EXPOSE 8080

USER $APP_UID

# Render asigna el puerto en PORT; en local se usa 8080.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} exec dotnet Magazing.Api.dll"]
