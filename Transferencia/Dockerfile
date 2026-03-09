FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Transferencia/Transferencia.csproj Transferencia/
RUN dotnet restore Transferencia/Transferencia.csproj
COPY Transferencia/ Transferencia/
RUN dotnet publish Transferencia/Transferencia.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Transferencia.dll"]
