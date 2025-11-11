FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["MS.Choferes/MS.Choferes.csproj", "MS.Choferes/"]
RUN dotnet restore "MS.Choferes/MS.Choferes.csproj"

# Copy everything else and build
COPY . ./
WORKDIR /src/MS.Choferes
RUN dotnet publish "MS.Choferes.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 5133
ENTRYPOINT ["dotnet", "MS.Choferes.dll"]
