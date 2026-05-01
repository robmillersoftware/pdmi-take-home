FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CryptidCare.Api/CryptidCare.Api.csproj src/CryptidCare.Api/
RUN dotnet restore src/CryptidCare.Api/CryptidCare.Api.csproj

COPY src/CryptidCare.Api/ src/CryptidCare.Api/
RUN dotnet publish src/CryptidCare.Api/CryptidCare.Api.csproj -c Release -o /app/publish

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CryptidCare.Api.dll"]
