# BBL Message

## Kafka
### Setup
Run command `docker compose up -d` at root of solution to start kafka container.

### Create Topic
Run command 
`docker compose exec broker \
  kafka-topics --create \
    --topic purchases \
    --bootstrap-server localhost:9092 \
    --replication-factor 1 \
    --partitions 1`
at root of solution to create topic with name "purchases"

### Consoles
#### Producer
At path "./consoles/bbl.mb.core.message.consumer"
`dotnet run --project bbl.mb.core.message.producer.csproj $(pwd)/properties/bbl.mb.core.message.producer.properties`

#### Consumer
At path "./consoles/bbl.mb.core.message.product"
`dotnet run --project bbl.mb.core.message.consumer.csproj $(pwd)/properties/bbl.mb.core.message.consumer.properties`

## Reference
1. https://developer.confluent.io/quickstart/kafka-local/