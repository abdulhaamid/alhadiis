# Build a�amas�
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# .csproj dosyas�n� kopyala
COPY ["Alhadis/Alhadis.csproj", "Alhadis/"]
RUN dotnet restore "Alhadis/Alhadis.csproj"

# T�m dosyalar� kopyala
COPY . .
WORKDIR "/src/Alhadis"
RUN dotnet publish "Alhadis.csproj" -c Release -o /app/publish

# �al��ma (runtime) a�amas�
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Alhadis.dll"]
