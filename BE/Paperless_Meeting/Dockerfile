# backend/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj
COPY *.csproj ./
RUN dotnet restore

# Copy source
COPY . ./

# Publish
RUN dotnet publish -c Release -o /app --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

EXPOSE 5000
HEALTHCHECK CMD curl --fail http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "Meeting.dll"]
