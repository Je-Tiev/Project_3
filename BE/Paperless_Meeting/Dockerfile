FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj trước để cache restore
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ source
COPY . ./

# Publish
RUN dotnet publish -c Release -o /app --self-contained false

# ---- RUNTIME ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Tạo thư mục Uploads và phân quyền sở hữu cho non-root user 'app' (UID 1654 trong ASP.NET 8)
RUN mkdir -p /app/Uploads && chown -R app:app /app

USER app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Paperless_Meeting.dll"]
