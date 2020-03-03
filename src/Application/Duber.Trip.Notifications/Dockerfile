#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Application/Duber.Trip.Notifications/Duber.Trip.Notifications.csproj", "src/Application/Duber.Trip.Notifications/"]
RUN dotnet restore "src/Application/Duber.Trip.Notifications/Duber.Trip.Notifications.csproj"
COPY . .
WORKDIR "/src/src/Application/Duber.Trip.Notifications"
RUN dotnet build "Duber.Trip.Notifications.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Duber.Trip.Notifications.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Duber.Trip.Notifications.dll"]