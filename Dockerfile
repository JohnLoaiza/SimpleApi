# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia los archivos de solución y de proyecto
COPY *.sln ./
COPY */*.csproj ./

# Restaura dependencias
RUN dotnet restore SimpleApi.sln

# Copia el resto del código
COPY . ./

# Publica el proyecto principal (ajusta el path si está en una subcarpeta)
RUN dotnet publish ./SimpleApi/SimpleApi.csproj -c Release -o out

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

EXPOSE 80

ENTRYPOINT ["dotnet", "SimpleApi.dll"]
