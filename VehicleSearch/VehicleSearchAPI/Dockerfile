# Use the official .NET 8 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all files and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET 8 runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8080 (or 5000 if you prefer)
EXPOSE 8080

# Start the API
ENTRYPOINT ["dotnet", "VehicleSearchAPI.dll"]
