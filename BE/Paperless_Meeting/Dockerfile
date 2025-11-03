FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Paperless_Meeting.csproj ./
RUN dotnet restore Paperless_Meeting.csproj -r linux-x64
COPY . ./
RUN dotnet publish Paperless_Meeting.csproj -c Release -o /app -r linux-x64 --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 5000
ENTRYPOINT ["dotnet", "Paperless_Meeting.dll"]
