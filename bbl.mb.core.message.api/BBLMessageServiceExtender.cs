using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace bbl.mb.core.message.api
{
    public static class BBLMessageServiceExtender
    {
        public static ServiceCollection AddBBLMessageService(this ServiceCollection services, IConfigurationRoot config)
        {
            services.Configure<MessageConfigure>((msgConfig) => {
               msgConfig.Uri = new Uri(config.GetValue<string>("kafka:bootstrap:servers"));
               msgConfig.Timeout = TimeSpan.FromSeconds(config.GetValue<int>("kafka:bootstrap:timeout"));
               msgConfig.GroupId = config.GetValue<string>("kafka:group:id");
               msgConfig.AutoOffsetReset = config.GetValue<string>("kafka:auto:offset:reset");
            });
            services.AddSingleton<IMessageManager, MessageManager>();
            services.AddSingleton<IMessageConsumer, MessageConsumer>();

            return services;
        }
    }
}