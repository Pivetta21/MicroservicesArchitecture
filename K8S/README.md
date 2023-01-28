# Kubernetes

Arquivo destinado à instruções para implantar os serviços da solução em ambiente de
desenvolvimento, isto é, utilizando os arquivos `.yaml` presentes neste diretório.

--------------------------------------------------------------------

## RabbitMQ

``Deploy + Services``

```bash
kubectl apply -f rabbitmq.yaml
```

## ELK Stack

``Deploy + Services``

```bash
kubectl apply -f elasticsearch.yaml
```

```bash
kubectl apply -f kibana.yaml
```

## Jaeger

``Deploy + Services``

```bash
kubectl apply -f jaeger-tracing.yaml
```

## Armory Service

### Database Deployment

``Secrets``

```bash
kubectl create secret generic armory-pgsql \
--from-literal=POSTGRES_USER="lucas" \
--from-literal=POSTGRES_PASSWORD="21022000"
```

``Deploy + Services + PVC``

```bash
kubectl apply -f armory-postgres.yaml
```

### API Deployment

``Images``

```bash
docker build -f ../Armory/Dockerfile -t localhost:5000/armory-service:latest ../
```

```bash
docker push localhost:5000/armory-service:latest
```

``Deploy + Service``

```bash
kubectl apply -f armory-service.yaml
```

## Game Service

### Database Deployment

``Secrets``

```bash
kubectl create secret generic game-pgsql \
--from-literal=POSTGRES_USER="pivetta" \
--from-literal=POSTGRES_PASSWORD="20002102"
```

``Deploy + Services + PVC``

```bash
kubectl apply -f game-postgres.yaml
```

### API Deployment

``Images``

```bash
docker build -f ../Game/Dockerfile -t localhost:5000/game-service:latest ../
```

```bash
docker push localhost:5000/game-service:latest
```

``Deploy + Service``

```bash
kubectl apply -f game-service.yaml
```

## Ingress NGINX

```bash
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.5.1/deploy/static/provider/cloud/deploy.yaml
```

```bash
kubectl apply -f nginx-ingress.yaml
```

--------------------------------------------------------------------

**Comandos Úteis** - kubectl

``Lista todos os serviços K8S em todos namespaces``

```bash
kubectl get services -A
```

``Remove todos os recursos referentes ao namespace ingress-nginx``

```bash
kubectl delete all --all -n ingress-nginx
```

``Reinicia e atualiza um deployment, substitui aos poucos os pods antigos pelos novos``

```bash
kubectl rollout restart deployment game-deploy
```

``Escala o número de replicas de um deploy para um número 'x' de pods``

```bash
kubectl scale --replicas=3 deployment/game-deploy
```

``Mostra detalhes de um recurso ou grupo de recursos (no caso descreve um segredo especifico)``

````bash
kubectl describe secret armory-pgsql
````

--------------------------------------------------------------------

> ### Local Docker Registry
> É onde as imagens docker dever ser publicadas/buscadas. É uma solução local alternativa ao DockerHub,
> as imagens persistidas para o **registry** serão utilizadas pelos **pods** do K8S.
>
> **Documentação:** [Deploy a registry server - localhost:5000](https://docs.docker.com/registry/deploying/)

> ### Nginx Ingress Controller
> É necessário adicionar um host para o endereço loopback.
>
> **Exemplo**: 127.0.0.1 microservices.kubernetes.internal