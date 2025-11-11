FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["MS.Vehiculos/MS.Vehiculos.csproj", "MS.Vehiculos/"]
RUN dotnet restore "MS.Vehiculos/MS.Vehiculos.csproj"

# Copy everything else and build
COPY . ./
WORKDIR /src/MS.Vehiculos
RUN dotnet publish "MS.Vehiculos.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 5135
ENTRYPOINT ["dotnet", "MS.Vehiculos.dll"]
