# Build aþamasý
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# .csproj dosyasýný kopyala
COPY ["Alhadis/Alhadis.csproj", "Alhadis/"]
RUN dotnet restore "Alhadis/Alhadis.csproj"

# Tüm dosyalarý kopyala
COPY . .
WORKDIR "/src/Alhadis"
RUN dotnet publish "Alhadis.csproj" -c Release -o /app/publish

# Çalýþma (runtime) aþamasý
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Alhadis.dll"]
