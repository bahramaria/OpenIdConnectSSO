# Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY OpenIdConnectSSO.AuthServer/*.csproj ./OpenIdConnectSSO.AuthServer/
COPY OpenIdConnectSSO.Client/*.csproj ./OpenIdConnectSSO.Client/
COPY OpenIdConnectSSO.Common/*.csproj ./OpenIdConnectSSO.Common/
COPY OpenIdConnectSSO.Client.Tests/*.csproj ./OpenIdConnectSSO.Client.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build the application
WORKDIR /src/OpenIdConnectSSO.AuthServer
RUN dotnet build --configuration Release --no-restore

WORKDIR /src/OpenIdConnectSSO.Client
RUN dotnet build --configuration Release --no-restore

# Publish the application
WORKDIR /src/OpenIdConnectSSO.AuthServer
RUN dotnet publish -c Release -o /app/authserver --no-restore

WORKDIR /src/OpenIdConnectSSO.Client
RUN dotnet publish -c Release -o /app/client --no-restore

# Auth Server runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS authserver-runtime
WORKDIR /app
COPY --from=build /app/authserver .
EXPOSE 8080 8081
ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENTRYPOINT ["dotnet", "OpenIdConnectSSO.AuthServer.dll"]

# Client runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS client-runtime
WORKDIR /app
COPY --from=build /app/client .
EXPOSE 8082 8083
ENV ASPNETCORE_URLS=http://+:8082;https://+:8083
ENTRYPOINT ["dotnet", "OpenIdConnectSSO.Client.dll"]
