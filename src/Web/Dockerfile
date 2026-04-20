FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["src/Web/GoodHamburger.Web.csproj", "src/Web/"]
RUN dotnet restore "src/Web/GoodHamburger.Web.csproj"

COPY . .
WORKDIR "/app/src/Web"
RUN dotnet publish "GoodHamburger.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GoodHamburger.Web.dll"]
