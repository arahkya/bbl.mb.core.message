using bbl.mb.core.message.api.configures;
using bbl.mb.core.message.api.consumer;
using bbl.mb.core.message.api.producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace bbl.mb.core.message.api.extensions
{
    public static class BblMessageServiceExtender
    {
        public static ServiceCollection AddBBLMessageService(this ServiceCollection services, IConfigurationRoot config)
        {
            services.Configure<MessageConfigure>((msgConfig) =>
            {
                msgConfig.ServerAddress = config.GetValue<string>("kafka:bootstrap:servers");
                msgConfig.Timeout = TimeSpan.FromSeconds(config.GetValue<int>("kafka:bootstrap:timeout"));
                msgConfig.CAPath = new FileInfo(config.GetValue<string>("kafka:bootstrap:security:keystore:location"));
                msgConfig.ClientCertificatePath = new FileInfo(config.GetValue<string>("kafka:bootstrap:security:client-certificate:location"));
                msgConfig.KeyPath = new FileInfo(config.GetValue<string>("kafka:bootstrap:security:client-key:location"));
            });
            services.AddSingleton<IMessageProducer, MessageProducer>();
            services.AddSingleton<IMessageConsumer, MessageConsumer>();

            return services;
        }
    }
}