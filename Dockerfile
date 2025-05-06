# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copiar todos los archivos del proyecto
COPY . ./

# Restaurar paquetes directamente desde el csproj
RUN dotnet restore ./SimpleApi.csproj

# Publicar
RUN dotnet publish ./SimpleApi.csproj -c Release -o /app/out

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 80
ENTRYPOINT ["dotnet", "SimpleApi.dll"]
