FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PromoCodeFactory.Core/PromoCodeFactory.Core.csproj",  "PromoCodeFactory.Core/"]
COPY ["PromoCodeFactory.DataAccess/PromoCodeFactory.DataAccess.csproj", "PromoCodeFactory.DataAccess/"]
COPY ["PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj", "PromoCodeFactory.WebHost/"]
# юнит-тест копировать - лишнее

RUN dotnet restore "PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj"
COPY . .

# если добавить флаг no-restore, будет ошибка, после копирования ресторим нугеты
RUN dotnet build "PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj" -c RELEASE -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime 
WORKDIR /app
COPY --from=build  /app/publish .

# этот порт будем предоставлять для доступа извне
EXPOSE 8080
ENTRYPOINT ["dotnet", "PromoCodeFactory.WebHost.dll"]
