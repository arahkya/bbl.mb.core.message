using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using bbl.mb.core.message.api.configures;
using bbl.mb.core.message.api.consumer;
using bbl.mb.core.message.api.producer;
using System.Security;

namespace bbl.mb.core.message.api.extensions
{
    public static class BBLMessageServiceExtender
    {
        public static ServiceCollection AddBBLMessageService(this ServiceCollection services, IConfigurationRoot config)
        {
            services.Configure<MessageConfigure>((msgConfig) =>
            {
                var secureStringFunc = new Func<string, SecureString>(plainPassword =>
                {
                    var plainPasswordSplited = plainPassword.ToCharArray();
                    var securePassword = new SecureString();

                    for (int i = 0; i < plainPasswordSplited.Length; i++)
                    {
                        securePassword.AppendChar(plainPasswordSplited[i]);
                    }

                    return securePassword;
                });

                msgConfig.ServerAddress = config.GetValue<string>("kafka:bootstrap:servers");
                msgConfig.Timeout = TimeSpan.FromSeconds(config.GetValue<int>("kafka:bootstrap:timeout"));
                msgConfig.CAPath = new DirectoryInfo(config.GetValue<string>("kafka:bootstrap:security:keystore:location"));                
            });
            services.AddSingleton<IMessageProducer, MessageProducer>();
            services.AddSingleton<IMessageConsumer, MessageConsumer>();

            return services;
        }
    }
}