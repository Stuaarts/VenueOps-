FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VenueOps.sln ./
COPY backend/src/VenueOps.Api/VenueOps.Api.csproj backend/src/VenueOps.Api/
RUN dotnet restore backend/src/VenueOps.Api/VenueOps.Api.csproj

COPY backend/src/VenueOps.Api backend/src/VenueOps.Api
WORKDIR /src/backend/src/VenueOps.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VenueOps.Api.dll"]
