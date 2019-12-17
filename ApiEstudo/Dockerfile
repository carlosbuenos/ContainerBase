FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 83

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["ApiEstudo/ApiEstudo.csproj", "ApiEstudo/"]
RUN dotnet restore "ApiEstudo/ApiEstudo.csproj"
COPY . .
WORKDIR "/src/ApiEstudo"
RUN dotnet build "ApiEstudo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiEstudo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiEstudo.dll"]