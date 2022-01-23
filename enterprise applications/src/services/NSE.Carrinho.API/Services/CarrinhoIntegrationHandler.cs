﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSE.Carrinho.API.Data;
using NSE.Core.Messages.Integration;
using NSE.MessageBus;

namespace NSE.Carrinho.API.Services
{
    //escutando fila - injeção resolvida no messagebusconfig
    //evento disparado da api de pedido, quando finaliza o pedido
    //via mediator, o contexto de pedido publica o evento de pedido realizado e o mediator publica na fila
    public class CarrinhoIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;

        public CarrinhoIntegrationHandler(IServiceProvider serviceProvider, IMessageBus bus)
        {
            _serviceProvider = serviceProvider;
            _bus = bus;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetSubscribers();
            return Task.CompletedTask;
        }

        private void SetSubscribers()
        {
            _bus.SubscribeAsync<PedidoRealizadoIntegrationEvent>("PedidoRealizado", async request =>
                await ApagarCarrinho(request));
        }

        //arquitetura simples - não precisamos de repositório, ou serviço para o processo
        private async Task ApagarCarrinho(PedidoRealizadoIntegrationEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            //obtém a instância contexto que foi injetado
            //lembrando que precisa ser dessa forma porque estamos em um contexto singleton (background service)
            var context = scope.ServiceProvider.GetRequiredService<CarrinhoContext>(); 

            var carrinho = await context.CarrinhoCliente
                .FirstOrDefaultAsync(c => c.ClienteId == message.ClienteId);

            if (carrinho != null)
            {
                context.CarrinhoCliente.Remove(carrinho);
                await context.SaveChangesAsync();
            }
        }
    }
}