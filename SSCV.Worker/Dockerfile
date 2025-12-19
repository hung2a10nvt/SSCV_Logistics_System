FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SSCV.Worker/SSCV.Worker.csproj", "SSCV.Worker/"]
COPY ["SSCV.Domain/SSCV.Domain.csproj", "SSCV.Domain/"]
COPY ["SSCV.Infrastructure/SSCV.Infrastructure.csproj", "SSCV.Infrastructure/"]

RUN dotnet restore "SSCV.Worker/SSCV.Worker.csproj"

COPY . .

WORKDIR "/src/SSCV.Worker"
RUN dotnet build "SSCV.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SSCV.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SSCV.Worker.dll"]