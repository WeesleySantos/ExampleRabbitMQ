# ExampleRabbitMQ

Exemplo simples de mensageria com **RabbitMQ** usando **.NET**.

O repositório contém:

- **`RabbitMQ.Producer`**: publica mensagens (pedidos) em um **exchange direct** (`pedido.exchange`) usando a **routing key** `pedido.criado`.
- **`RabbibMQ.Consumer`**: consome da fila `pedido.criados` (ACK manual e simulação de processamento).
- **`RabbitMQ.Model`**: modelos (`Pedido`, `Item`) serializados em **JSON**.

## Arquitetura (resumo)

- **Exchange**: `pedido.exchange` (direct, durable)
- **Queue**: `pedido.criados` (durable)
- **Binding**: routing key `pedido.criado`

## Pré-requisitos

- **Docker Desktop** (com Docker Compose)
- (Opcional) **.NET SDK** para rodar localmente

## Como rodar com Docker

Na raiz do projeto:

```bash
docker compose up --build
```

Isso vai subir:

- **RabbitMQ** (AMQP em `localhost:5672`)
- **Management UI** em `localhost:15672` (usuário `guest`, senha `guest`)
- **Consumer** (fica rodando e consumindo mensagens)
- **Producer** (envia alguns pedidos automaticamente)

### Acessar o RabbitMQ Management

- URL: `http://localhost:15672`
- Usuário: `guest`
- Senha: `guest`

### Alterar quantidade de mensagens do Producer (Docker)

Edite o `docker-compose.yml` ou rode com override de variáveis:

```bash
QUANTIDADE_PEDIDOS=10 docker compose up --build
```

No PowerShell:

```powershell
$env:QUANTIDADE_PEDIDOS = "10"
docker compose up --build
```

## Rodar apps localmente usando RabbitMQ no Docker

1) Suba apenas o RabbitMQ:

```bash
docker compose up -d rabbitmq
```

2) Rode o Consumer:

```bash
dotnet run --project src/RabbibMQ.Consumer
```

3) Rode o Producer:

```bash
dotnet run --project src/RabbitMQ.Producer
```

## Configuração por variáveis de ambiente

Tanto Producer quanto Consumer aceitam:

- `RABBITMQ_HOST` (padrão: `localhost`)
- `RABBITMQ_PORT` (padrão: `5672`)
- `RABBITMQ_USERNAME` (padrão: `guest`)
- `RABBITMQ_PASSWORD` (padrão: `guest`)
- `RABBITMQ_VHOST` (padrão: `/`)

Extras do Producer:

- `QUANTIDADE_PEDIDOS` (se definido, evita prompt e usa esse valor)
- `AUTO_SEND` (`true/false`) para enviar sem esperar ENTER entre mensagens

