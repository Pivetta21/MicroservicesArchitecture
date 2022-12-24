FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Game/Game.csproj", "Game/"]
RUN dotnet restore "Game/Game.csproj"
COPY . .
WORKDIR "/src/Game"
RUN dotnet build "Game.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Game.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Game.dll"]
