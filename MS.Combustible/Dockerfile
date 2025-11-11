FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["MS.Combustible/MS.Combustible.csproj", "MS.Combustible/"]
RUN dotnet restore "MS.Combustible/MS.Combustible.csproj"

# Copy everything else and build
COPY . ./
WORKDIR /src/MS.Combustible
RUN dotnet publish "MS.Combustible.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 5136
ENTRYPOINT ["dotnet", "MS.Combustible.dll"]
