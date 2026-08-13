FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 0

# ===================================================
# INSTALACIÓN DE LIBRERÍAS REQUERIDAS POR PUPPETEER (CHROMIUM)
# ===================================================
RUN apt-get update && apt-get install -y --no-install-recommends \
    chromium \
    fonts-liberation \
    tzdata \
    && rm -rf /var/lib/apt/lists/*
# ===================================================
ENV PUPPETEER_SKIP_DOWNLOAD=true
ENV CHROMIUM_PATH=/usr/bin/chromium

# Zona horaria del contenedor (configurable con --build-arg TZ=America/Guayaquil)
ARG TZ=UTC
ENV TZ=${TZ}
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Copia tu publicación
COPY published/ ./

# COPIA el folder env (muy importante)
#COPY env/ ./env/

# Con esto ASP.NET encontrará appsettings.{ASPNETCORE_ENVIRONMENT}.json dentro del contenedor
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:0
ENTRYPOINT ["dotnet", "Company.NameProject.WebApi.dll"]
