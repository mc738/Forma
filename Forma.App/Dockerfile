FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Forma.App/Forma.App.fsproj", "Forma.App/"]
RUN dotnet restore "Forma.App/Forma.App.fsproj"
COPY . .
WORKDIR "/src/Forma.App"
RUN dotnet build "Forma.App.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Forma.App.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Forma.App.dll"]
