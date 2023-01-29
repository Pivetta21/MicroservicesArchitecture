# Microservices Architecture

Repositório destinado a implementação da arquitetura de
microsserviços e seus principais componentes.

`` C# | .NET | PostgreSQL | RabbitMQ | ELK Stack | OpenTelemetry + Jaeger | K8S ``

--------------------------------------------------------------------

## Armory Service

É o microsserviço responsável por manter os dados relacionados ao personagem: informações gerais, level, ouro,
equipamentos, inventário e entradas para masmorras.

- Url base ambiente local: http://localhost:5274
- Url base ambiente K8S: http://microservices.kubernetes.internal:80/armory

``Docker``

```bash
docker build -f Armory/Dockerfile -t localhost:5000/armory-service:latest .
```

```bash
docker run --name armory-service -p 8080:80 localhost:5000/armory-service:latest -d
```

``Dev Secrets``

```bash
cd ./Armory && \
dotnet user-secrets init && \
dotnet user-secrets set "POSTGRES_USER" "postgres" && \
dotnet user-secrets set "POSTGRES_PASSWORD" "1234" 
```

## Game Service

É o microsserviço responsável por manter os dados relacionados a masmorras: informações gerais,
recompensas, histórico e informações dos itens (preço, qualidade, atributos, e etc).

- Url base ambiente local: http://localhost:5058
- Url base ambiente K8S: http://microservices.kubernetes.internal:80/game

``Docker``

```bash
docker build -f Game/Dockerfile -t localhost:5000/game-service:latest .
```

```bash
docker run --name game-service -p 8080:80 localhost:5000/game-service:latest -d
```

``Dev Secrets``

```bash
cd ./Game && \
dotnet user-secrets init && \
dotnet user-secrets set "POSTGRES_USER" "postgres" && \
dotnet user-secrets set "POSTGRES_PASSWORD" "1234"
```

--------------------------------------------------------------------

> O arquivo **README.md** presente na pasta **K8S** contém as informações necessárias para inicializar os microsserviços no Kubernetes.
