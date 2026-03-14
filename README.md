# JobService.API

Serviço de processamento de tarefas em background construído com ASP.NET Core 9, MongoDB e RabbitMQ.

## Funcionalidades

- Criação de tarefas via API REST
- Processamento assíncrono por workers em background
- Sistema de retry com limite configurável de tentativas
- Controle de concorrência com lock otimista no MongoDB
- Roteamento de tarefas por tipo via padrão de handlers

## Pré-requisitos

Para rodar com Docker:

- [Docker](https://www.docker.com/) e Docker Compose

Para rodar localmente:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- MongoDB rodando na porta `27017`
- RabbitMQ rodando na porta `5672`

## Executando com Docker

Suba todos os serviços com um único comando a partir da pasta `JobService.API/`:

```bash
docker-compose up --build
```

Isso irá inicializar três containers:

| Serviço   | Porta         | Descrição                        |
|-----------|---------------|----------------------------------|
| api       | 8080          | API REST                         |
| mongodb   | 27017         | Banco de dados                   |
| rabbitmq  | 5672 / 15672  | Fila / Interface de gerenciamento|

A interface de gerenciamento do RabbitMQ estará disponível em `http://localhost:15672` com as credenciais `guest / guest`.

## Executando localmente

Configure o arquivo `appsettings.Development.json` com as credenciais dos serviços locais:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "jobservice",
    "CollectionName": "jobs"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "QueueName": "jobs"
  }
}
```

Execute a aplicação:

```bash
dotnet run
```

## Endpoints

### Criar tarefa

```
POST /jobs
```

Corpo da requisição:

```json
{
  "taskType": "EnviarEmail",
  "payload": "{\"to\": \"usuario@email.com\"}"
}
```

Tipos de tarefa suportados: `EnviarEmail`, `GerarRelatorio`.

Resposta de sucesso: `201 Created` com o objeto da tarefa criada.

### Consultar status

```
GET /jobs/{id}
```

Resposta de sucesso: `200 OK` com o objeto da tarefa.

Resposta quando não encontrado: `404 Not Found`.

## Variáveis de ambiente

As configurações podem ser sobrescritas via variáveis de ambiente usando `__` como separador de chave aninhada:

```
MongoDB__ConnectionString
MongoDB__DatabaseName
MongoDB__CollectionName
RabbitMQ__Host
RabbitMQ__Port
RabbitMQ__Username
RabbitMQ__Password
RabbitMQ__QueueName
```