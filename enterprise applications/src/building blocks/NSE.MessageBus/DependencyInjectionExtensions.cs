using System;
using Microsoft.Extensions.DependencyInjection;

namespace NSE.MessageBus
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMessageBus(this IServiceCollection services, string connection)
        {
            if (string.IsNullOrEmpty(connection)) throw new ArgumentNullException();

            //a instância é criada dentro da classe, então não vamos passar por d.i
            services.AddSingleton<IMessageBus>(new MessageBus(connection));

            return services;
        }
    }
}