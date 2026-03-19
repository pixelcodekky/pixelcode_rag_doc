FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Port the application listens on
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all the csproj files to respect Docker caching for dependencies
COPY ["src/RagBot.WebApi/RagBot.WebApi.csproj", "src/RagBot.WebApi/"]
COPY ["src/RagBot.Application/RagBot.Application.csproj", "src/RagBot.Application/"]
COPY ["src/RagBot.Domain/RagBot.Domain.csproj", "src/RagBot.Domain/"]
COPY ["src/RagBot.Infrastructure/RagBot.Infrastructure.csproj", "src/RagBot.Infrastructure/"]

# Restore everything via the main startup project
RUN dotnet restore "src/RagBot.WebApi/RagBot.WebApi.csproj"

# Copy the rest of the source code
COPY . .

# Build
WORKDIR "/src/src/RagBot.WebApi"
RUN dotnet build "RagBot.WebApi.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "RagBot.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RagBot.WebApi.dll"]
