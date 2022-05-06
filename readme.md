# BBL Message

## Kafka
### Setup
Run command `docker compose up -d` at root of solution to start kafka container.

### Create Topic
Run command 
`docker compose exec broker kafka-topics --create --topic purchases --bootstrap-server localhost:9092 --replication-factor 1 --partitions 1`
at root of solution to create topic with name "purchases"

### Topic List
Run command 
`docker compose exec broker kafka-topics --list --bootstrap-server localhost:9092`


### Consume Topic in docker
**command**
`docker exec -i --tty ${docker_container} kafka-console-consumer --bootstrap-server ${docker_container}:9092 --topic ${topic_name} --from-beginning`
**example**
`docker exec -i --tty broker-test kafka-console-consumer --bootstrap-server broker-test:9092 --topic testHelloWorld --from-beginning`

## SSL
**Certificate**
1. `openssl genrsa -o key.message.key 4096`
2. `openssl req -key key.message.key -out message.crt -new -days 365 -x509`

### Truststore
**command**
`keytool -keystore $(file_name).jks -alias CARoot -import -file root.crt`
**sample**
`keytool -keystore message.truststore.jks -alias CARoot -import -file root.crt`

### Keystore
**command**
`keytool -keystore $(file_name).jks -alias $(alise name) -validity $(days) -genkey -keyalg RSA`
**sample**
`keytool -keystore keystore.message.jks -alias localhost -validity 365 -genkey -keyalg RSA`

### Consoles
#### Producer
At path "./consoles/bbl.mb.core.message.consumer"
`dotnet run --project bbl.mb.core.message.producer.csproj $(pwd)/properties/bbl.mb.core.message.producer.properties`

#### Consumer
At path "./consoles/bbl.mb.core.message.product"
`dotnet run --project bbl.mb.core.message.consumer.csproj $(pwd)/properties/bbl.mb.core.message.consumer.properties`

## Reference
1. https://developer.confluent.io/quickstart/kafka-local/
2. https://docs.oracle.com/cd/E19830-01/819-4712/ablqw/index.html - jks certificate
3. https://github.com/edenhill/librdkafka/wiki/Using-SSL-with-librdkafka
4. https://github.com/mhowlett/confluent-kafka-dotnet/tree/security/examples/Security - Good one