# Security Configuration Examples

This is a walkthough of various Kafka security configuration scenarios with example code in C#.

**Note:** WORK IN PROGRESS! I will hopefully get a chance to add a Kerberos example soon.

For simplicity we're just going to be setting up a single broker and client, but we'll also make some notes along the way on what you'll need to do for more complex configurations. 

As you work through these instructions, you'll need to specify the names of your broker `{server_hostname}` and client `{client_hostname}` machines a number of times. Ideally, you will use the Fully Qualified Domain Name (FQDN) of the machines but if you haven't assigned hostnames to your machines, you can make do with IP addresses. If you are just experimenting and you're going to run the broker and client on the same machine, it's also ok to use `localhost` or `127.0.0.1` for both.

For further information, some good resources are:

- [https://github.com/edenhill/librdkafka/wiki/Using-SSL-with-librdkafka](https://github.com/edenhill/librdkafka/wiki/Using-SSL-with-librdkafka)
- [https://github.com/edenhill/librdkafka/wiki/Using-SASL-with-librdkafka](https://github.com/edenhill/librdkafka/wiki/Using-SASL-with-librdkafka)
- [https://kafka.apache.org/documentation/#security](https://kafka.apache.org/documentation/#security)
- [https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md](https://github.com/edenhill/librdkafka/blob/master/CONFIGURATION.md)

## Example 1 - One-Way SSL/TLS 

- Exactly the same setup as an HTTPS connection to a website.
- Client verifies the identity of the broker - i.e. the client can be sure it is communicating with the intended broker, not an impersonator. i.e. safe from man-in-the-middle attacks.
- Communication is encrypted.
- Server **does not** verify the identity of the client (does not authenticate the client) - any client can connect to the server.

**Procedure:**



1. Create a *private key* / *public key certificate* pair for the broker:

    **Note:** You will need to repeat this step for every broker in your cluster.

    **Note:** We use the `keytool` utility that comes with Java for this task because it stores the generated key / certificate pair in a JKS container, as required by the broker.

    ```
    keytool -keystore server.keystore.jks -alias {server_hostname} -validity 365 -genkey -keyalg RSA
    ```

    This will:

    1. Create a new keystore `server.keystore.jks` in the current directory.
   
    1. Prompt you for a password to protect the keystore.

    1. Generate a new public/private key pair and prompt you for additional information that is required to make a *public key certificate*. In aggregate this information is referred to as the Distinguishing Name (DN).
        - You need to set the CN (the answer to the question 'What is your first and last name?') to `{server_hostname}`. None of the other 
        information is important for our purposes.
	- You can also set the DN via the command line `-dname` parameter like so: `-dname "cn={server_hostname}"`

    1. Store the private key and self-signed public key certificate under the alias `{server_hostname}` in the keystore file.
        - you will be prompted for a password to protect this specific key / certificate pair in the keystore.
	- alternatively you can set the password using the `-storepass` and `-keypass` command line arguments.

1. Create a Certificate Authority (CA) private key / root certificate:

    **Important Note:** Don't be tempted to simplify the configuration procedure by using self-signed certificates (avoiding the use of a CA) - this is not secure (is susceptible to man-in-the-middle attacks).

    ```
    openssl req -nodes -new -x509 -keyout ca-root.key -out ca-root.crt -days 365
    ```

    This will:

    1. Prompt you for DN information to put in the certificate. None of this is required for our purposes, but 
       - You can use the -subj command line parameter to avoid the prompt. This can't be blank. e.g.: `-subj "/C=US/ST=CA/L=Palo Alto/O=Confluent/CN=Confluent"`
       
    2. Create a private key / self-signed public key cetificate pair where the private key isn't password protected. 

    3. If you would like to password protect the private key, omit the `-nodes` flag and you will be prompted for a password.


1. Sign the broker public key certificate:

    1. Generate a Certificate Signing Request (CSR) from the self-signed certificate you created in step 1 housed inside the server keystore file:

    ```
    keytool -keystore server.keystore.jks -alias {server_hostname} -certreq -file {server_hostname}_server.csr
    ```

    2. Use the CA key pair you generated in step 2 to create a CA signed certificate from the CSR you just created:

    ```
    openssl x509 -req -CA ca-root.crt -CAkey ca-root.key -in {server_hostname}_server.csr -out {server_hostname}_server.crt -days 365 -CAcreateserial 
    ```

    3. Import this signed certificate into your server keystore (over-writing the existing self-signed one). Before you can do this, you'll need to add the CA public key certificate as well:

    ```
    keytool -keystore server.keystore.jks -alias CARoot -import -noprompt -file ca-root.crt
    keytool -keystore server.keystore.jks -alias {server_hostname} -import -file {server_hostname}_server.crt
    ```

1. Configure the broker and client

    You now have everything you need to configure the broker and client.

    Broker config:

    ```
    listeners=PLAINTEXT://{server_hostname}:9092,SSL://{server_hostname}:9093
    ssl.keystore.location=/path/to/keystore/file/server.keystore.jks
    ssl.keystore.type=JKS
    ssl.keystore.password=test1234
    ssl.key.password=test1234
    ```

    Confluent.Kafka config:

    ```
    var config = new Dictionary<string, object> 
    { 
        { "bootstrap.servers", "{server_hostname}:9093" },
        { "security.protocol", "SSL" },
        { "ssl.ca.location", @"C:\path\to\caroot\file\ca-root.crt" }
    }
    ```

## Example 2 - Two-Way SSL/TLS (Example 1 + SSL Client Authentication)

- Functionality provided by Example 1, plus:

- Server verifies the identity of the client (i.e. that the public key certificate provided by the client has been signed by a CA trusted by the server).

**Procedure:**

1. Create a private key / public key certificate pair for the client: 

    The .NET client is not Java based and consequently doesn't use Java's JKS container format for storing private keys and certificates. We will use `openssl` to create the key / certificate pair for the client, not `keytool` as we did for the broker.

    The first step is to create a Certificate Signing Request (CSR). Note: there is no need to explicitly create a self signed certificate first as we did for the broker.

    ```
    openssl req -newkey rsa:2048 -nodes -keyout {client_hostname}_client.key -out {client_hostname}_client.csr
    ```

    This will prompt you for the set of standard public key certificate field values. These are not important for our purposes. It is possible to set up ACLs based on the  you wish to later set ACLs based on their values (TODO: work through setting this up).

    You will also be prompted for a password. You can enter a blank password here, or set a password.

    Now you have the CSR, you can generate a CA signed certificate as follows:

    ```
    openssl x509 -req -CA ca-root.crt -CAkey ca-root.key -in {client_hostname}_client.csr -out {client_hostname}_client.crt -days 365 -CAcreateserial 
    ```

1. Create a truststore containing the ca-root.crt:

    The broker now requires access to the CA root certificate in order to check the validity of the certificate supplied by the client. Create a truststore  (another JKS container file) that contains the CA root certificate as follows:

    ```
    keytool -keystore server.truststore.jks -alias CARoot -import -file ca-root.crt
    ```

1. Configure the broker and client.

    Broker config:

    ```
    listeners=PLAINTEXT://{server_hostname}:9092,SSL://{server_hostname}:9093
    ssl.keystore.location=/path/to/keystore/file/server.keystore.jks
    ssl.keystore.type=JKS
    ssl.keystore.password=test1234
    ssl.key.password=test1234
    ssl.truststore.location=/path/to/truststore/file/server.truststore.jks
    ssl.truststore.type=JKS
    ssl.truststore.password=test1234
    ssl.client.auth=required
    ```

    Confluent.Kafka config:

    ```
    var mutualAuthConfig = new Dictionary<string, object> 
    { 
        { "bootstrap.servers", "{server_hostname}:9093" },
        { "security.protocol", "SSL" },
        { "ssl.ca.location", "@"C:\path\to\caroot\file\ca-root.crt" },
        { "ssl.certificate.location", "@"C:\path\to\client\certificate\file\{client_hostname}_client.crt" },
        { "ssl.key.location", "@"C:\path\to\client\key\file\{client_hostname}_client.key" },
    };
    ```

## Example 3 - Example 2 + SASL Plain Authentication

**Procedure:**

TODO


## Example 3 - Example 2 + SASL Kerberos Authentication

**Procedure:**

TODO