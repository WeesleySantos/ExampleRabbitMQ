FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/RabbitMQ.Model/RabbitMQ.Model.csproj src/RabbitMQ.Model/
COPY src/RabbibMQ.Consumer/RabbibMQ.Consumer.csproj src/RabbibMQ.Consumer/
RUN dotnet restore src/RabbibMQ.Consumer/RabbibMQ.Consumer.csproj

COPY src/ src/
RUN dotnet publish src/RabbibMQ.Consumer/RabbibMQ.Consumer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish/ ./
ENTRYPOINT ["dotnet", "RabbibMQ.Consumer.dll"]
