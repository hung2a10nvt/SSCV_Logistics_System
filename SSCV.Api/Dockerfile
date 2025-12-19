FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SSCV.Api/SSCV.Api.csproj", "SSCV.Api/"]
COPY ["SSCV.Domain/SSCV.Domain.csproj", "SSCV.Domain/"]
COPY ["SSCV.Infrastructure/SSCV.Infrastructure.csproj", "SSCV.Infrastructure/"]
RUN dotnet restore "SSCV.Api/SSCV.Api.csproj"

COPY . .
WORKDIR "/src/SSCV.Api"
RUN dotnet build "SSCV.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SSCV.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SSCV.Api.dll"]