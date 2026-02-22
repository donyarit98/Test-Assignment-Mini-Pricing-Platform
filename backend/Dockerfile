FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PricingPlatform.API/PricingPlatform.API.csproj", "PricingPlatform.API/"]
COPY ["PricingPlatform.Application/PricingPlatform.Application.csproj", "PricingPlatform.Application/"]
COPY ["PricingPlatform.Core/PricingPlatform.Core.csproj", "PricingPlatform.Core/"]
COPY ["PricingPlatform.Infrastructure/PricingPlatform.Infrastructure.csproj", "PricingPlatform.Infrastructure/"]

RUN dotnet restore "PricingPlatform.API/PricingPlatform.API.csproj"

COPY . .

RUN dotnet build "PricingPlatform.API/PricingPlatform.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PricingPlatform.API/PricingPlatform.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create data directory for rules.json
RUN mkdir -p /app/Data

ENTRYPOINT ["dotnet", "PricingPlatform.API.dll"]