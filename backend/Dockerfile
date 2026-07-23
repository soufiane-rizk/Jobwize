# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Api/JobWize.Api/JobWize.Api.csproj", "src/Api/JobWize.Api/"]
COPY ["src/Modules/Identity/JobWize.Modules.Identity/JobWize.Modules.Identity.csproj", "src/Modules/Identity/JobWize.Modules.Identity/"]
COPY ["src/Shared/JobWize.Shared/JobWize.Shared.csproj", "src/Shared/JobWize.Shared/"]
COPY ["src/Runtime/JobWize.Runtime.Contracts/JobWize.Runtime.Contracts.csproj", "src/Runtime/JobWize.Runtime.Contracts/"]
COPY ["src/Runtime/JobWize.Runtime/JobWize.Runtime.csproj", "src/Runtime/JobWize.Runtime/"]
COPY ["src/Shared/JobWize.Shared.Contracts/JobWize.Shared.Contracts.csproj", "src/Shared/JobWize.Shared.Contracts/"]
COPY ["src/Modules/Identity/JobWize.Modules.Identity.Contracts/JobWize.Modules.Identity.Contracts.csproj", "src/Modules/Identity/JobWize.Modules.Identity.Contracts/"]
RUN dotnet restore "./src/Api/JobWize.Api/JobWize.Api.csproj"
COPY . .
WORKDIR "/src/src/Api/JobWize.Api"
RUN dotnet build "./JobWize.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./JobWize.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobWize.Api.dll"]