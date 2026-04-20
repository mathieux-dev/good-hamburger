FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY GoodHamburger.slnx .
COPY src/Domain/GoodHamburger.Domain.csproj src/Domain/
COPY src/Application/GoodHamburger.Application.csproj src/Application/
COPY src/Infrastructure/GoodHamburger.Infrastructure.csproj src/Infrastructure/
COPY src/Api/GoodHamburger.Api.csproj src/Api/

RUN dotnet restore src/Api/GoodHamburger.Api.csproj

COPY src/ src/
RUN dotnet publish src/Api/GoodHamburger.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "GoodHamburger.Api.dll"]
