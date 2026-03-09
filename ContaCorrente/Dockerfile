FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ContaCorrente/ContaCorrente.csproj ContaCorrente/
RUN dotnet restore ContaCorrente/ContaCorrente.csproj
COPY ContaCorrente/ ContaCorrente/
RUN dotnet publish ContaCorrente/ContaCorrente.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContaCorrente.dll"]
