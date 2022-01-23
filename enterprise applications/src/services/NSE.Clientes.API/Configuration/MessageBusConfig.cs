using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSE.Clientes.API.Services;
using NSE.Core.Utils;
using NSE.MessageBus;

namespace NSE.Clientes.API.Configuration
{
    public static class MessageBusConfig
    {
        public static void AddMessageBusConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            //GetMessageQueueConnection: extensão da configuration para pagar outra sessão do appsettings (meio frescura)

            //messagebus: está abstraído em um projeto no buildingblocks
            services.AddMessageBus(configuration.GetMessageQueueConnection("MessageBus"))
                .AddHostedService<RegistroClienteIntegrationHandler>();
        }
    }
}