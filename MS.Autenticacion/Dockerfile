FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["MS.Autenticacion/MS.Autenticacion.csproj", "MS.Autenticacion/"]
RUN dotnet restore "MS.Autenticacion/MS.Autenticacion.csproj"

# Copy everything else and build
COPY . ./
WORKDIR /src/MS.Autenticacion
RUN dotnet publish "MS.Autenticacion.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 5001
ENTRYPOINT ["dotnet", "MS.Autenticacion.dll"]
