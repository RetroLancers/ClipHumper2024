FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ClipHunta2.csproj", "./"]
RUN dotnet restore "ClipHunta2.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "ClipHunta2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClipHunta2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClipHunta2.dll"]
