# Multi-stage build for ASP.NET Core 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CardCompass.Web/CardCompass.Web.csproj", "CardCompass.Web/"]
RUN dotnet restore "CardCompass.Web/CardCompass.Web.csproj"

COPY . .
WORKDIR "/src/CardCompass.Web"
RUN dotnet publish "CardCompass.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CardCompass.Web.dll"]
