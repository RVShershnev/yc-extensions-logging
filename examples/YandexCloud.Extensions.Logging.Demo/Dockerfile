#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["YandexCloud.Extensions.Logging.Demo/YandexCloud.Extensions.Logging.Demo.csproj", "YandexCloud.Extensions.Logging.Demo/"]
RUN dotnet restore "YandexCloud.Extensions.Logging.Demo/YandexCloud.Extensions.Logging.Demo.csproj"
COPY . .
WORKDIR "/src/YandexCloud.Extensions.Logging.Demo"
RUN dotnet build "YandexCloud.Extensions.Logging.Demo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YandexCloud.Extensions.Logging.Demo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YandexCloud.Extensions.Logging.Demo.dll"]