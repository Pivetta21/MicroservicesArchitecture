# Microservices Architecture

Repositório destinado à implementação da arquitetura de microsserviços e seus principais componentes.
_________________________________________________________

## Armory Service

Armazena informações dos personagens de um usuário.

### Database - PostgreSQL

> Dev Secrets

```
cd Armory/ 

dotnet user-secrets init 

dotnet user-secrets set "POSTGRES_USER" "postgres" 

dotnet user-secrets set "POSTGRES_PASSWORD" "1234" 
```

> K8S Secrets

```
kubectl create secret generic armory-pgsql \
--from-literal=POSTGRES_USER="lucas" \
--from-literal=POSTGRES_PASSWORD="21022000"
```

## Game Service

Possui endpoint para simular partidas e, consequentemente, gerar pontuações/itens.

### Database - PostgreSQL

> Dev Secrets

```
cd Game/ 

dotnet user-secrets init 

dotnet user-secrets set "POSTGRES_USER" "postgres" 

dotnet user-secrets set "POSTGRES_PASSWORD" "1234" 
```

> K8S Secrets

```
kubectl create secret generic game-pgsql \
--from-literal=POSTGRES_USER="pivetta" \
--from-literal=POSTGRES_PASSWORD="20002102"
```

## Leaderboard Service

Consume eventos de conclusão de partidas e mantém um ranking que pode ser consultado.

## Auction Service

Permite jogadores vender itens que podem estar em três estados: na fila, listado, em disputa, cancelado, vendido e
arrematado.

## Identity Service

Possui informações de cadastro de usuário, permissões, e etc.
