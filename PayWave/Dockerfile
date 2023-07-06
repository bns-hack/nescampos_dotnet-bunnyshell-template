# Imagen base
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

# Directorio de trabajo
WORKDIR /app

# Copiar archivos de proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar todo el código fuente y compilar
COPY . ./
RUN dotnet publish -c Release -o out

# Imagen de producción
FROM mcr.microsoft.com/dotnet/aspnet:6.0

# Directorio de trabajo
WORKDIR /app

# Copiar archivos publicados de la etapa anterior
COPY --from=build-env /app/out .

ENV CircleAPIKey=${CircleAPIKey}

# Puerto en el que escucha la aplicación
EXPOSE 80
EXPOSE 443

# Comando de inicio de la aplicación
ENTRYPOINT ["dotnet", "PayWave.dll"]