FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY microservices-netcore-docker-servicefabric.sln ./
COPY src/Application/Duber.Invoice.API/Duber.Invoice.API.csproj src/Application/Duber.Invoice.API/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/src/Application/Duber.Invoice.API
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Duber.Invoice.API.dll"]
