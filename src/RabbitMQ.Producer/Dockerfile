FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/RabbitMQ.Model/RabbitMQ.Model.csproj src/RabbitMQ.Model/
COPY src/RabbitMQ.Producer/RabbitMQ.Producer.csproj src/RabbitMQ.Producer/
RUN dotnet restore src/RabbitMQ.Producer/RabbitMQ.Producer.csproj

COPY src/ src/
RUN dotnet publish src/RabbitMQ.Producer/RabbitMQ.Producer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish/ ./
ENTRYPOINT ["dotnet", "RabbitMQ.Producer.dll"]
