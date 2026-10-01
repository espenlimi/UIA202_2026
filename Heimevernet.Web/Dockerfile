FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY Heimevernet.Web/Heimevernet.Web.csproj Heimevernet.Web/
RUN dotnet restore Heimevernet.Web/Heimevernet.Web.csproj

COPY Heimevernet.Web/ Heimevernet.Web/
WORKDIR /src/Heimevernet.Web
RUN dotnet publish Heimevernet.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Heimevernet.Web.dll"]
