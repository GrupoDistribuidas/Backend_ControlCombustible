FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["MS.Rutas/MS.Rutas.csproj", "MS.Rutas/"]
RUN dotnet restore "MS.Rutas/MS.Rutas.csproj"

# Copy everything else and build
COPY . ./
WORKDIR /src/MS.Rutas
RUN dotnet publish "MS.Rutas.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 5174
ENTRYPOINT ["dotnet", "MS.Rutas.dll"]
