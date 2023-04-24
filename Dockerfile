# Build image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy project files
COPY *.sln ./
COPY FleetManagement/*.csproj ./FleetManagement/
COPY FleetManagement/ ./FleetManagement/

# Restore NuGet packages
RUN dotnet restore

# Build the project
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port the app runs on
EXPOSE 5000

# Start the app
ENTRYPOINT ["dotnet", "FleetManagement.dll"]