FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Presentation/Company.NameProject.WebApi/Company.NameProject.WebApi.csproj", "src/Presentation/Company.NameProject.WebApi/"]
COPY ["src/Core/Company.NameProject.Application/Company.NameProject.Application.csproj", "src/Core/Company.NameProject.Application/"]
COPY ["src/Core/Company.NameProject.Domain/Company.NameProject.Domain.csproj", "src/Core/Company.NameProject.Domain/"]
COPY ["src/Core/Company.NameProject.Shared/Company.NameProject.Shared.csproj", "src/Core/Company.NameProject.Shared/"]
COPY ["src/Infrastructure/Company.NameProject.Infrastructure/Company.NameProject.Infrastructure.csproj", "src/Infrastructure/Company.NameProject.Infrastructure/"]
COPY ["src/Infrastructure/Company.NameProject.Persistence/Company.NameProject.Persistence.csproj", "src/Infrastructure/Company.NameProject.Persistence/"]
RUN dotnet restore "src/Presentation/Company.NameProject.WebApi/Company.NameProject.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Presentation/Company.NameProject.WebApi"
RUN dotnet build "Company.NameProject.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Company.NameProject.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Company.NameProject.WebApi.dll"]
