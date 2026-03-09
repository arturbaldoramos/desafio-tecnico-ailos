FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Caixa/Caixa.csproj Caixa/
RUN dotnet restore Caixa/Caixa.csproj
COPY Caixa/ Caixa/
RUN dotnet publish Caixa/Caixa.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Caixa.dll"]
