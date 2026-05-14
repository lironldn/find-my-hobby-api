# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FindMyHobbyApi/FindMyHobbyApi.csproj FindMyHobbyApi/
RUN dotnet restore FindMyHobbyApi/FindMyHobbyApi.csproj

COPY . .
RUN dotnet publish FindMyHobbyApi/FindMyHobbyApi.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FindMyHobbyApi.dll"]