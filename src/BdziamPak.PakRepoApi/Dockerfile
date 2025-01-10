FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
VOLUME /app/data
EXPOSE 8083

ARG API_KEY
ENV Auth__ApiKey=$API_KEY

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
WORKDIR "/src/BdziamPak.PakRepoApi"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BdziamPak.PakRepoApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BdziamPak.PakRepoApi.dll"]