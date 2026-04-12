FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj",  "PromoCodeFactory.WebHost/"]

RUN dotnet restore "PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj"

# в этом месте добавим все остальное, считая, что от папки bin и ниже нугеты имеют место быть
# перезапись PromoCodeFactory.WebHost пройдет быстро, и теперь все будет говото к общему билду
COPY . .

# если добавить флаг no-restore, будет ошибка, после копирования ресторим нугеты
RUN dotnet publish "PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj" -c RELEASE -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime 
WORKDIR /app
COPY --from=build  /app/publish .

# этот порт будем предоставлять для доступа извне
EXPOSE 8080
ENTRYPOINT ["dotnet", "PromoCodeFactory.WebHost.dll"]
