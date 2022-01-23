using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSE.Clientes.API.Application.Commands;
using NSE.Core.Mediator;
using NSE.Core.Messages.Integration;
using NSE.MessageBus;

namespace NSE.Clientes.API.Services
{
    //backgroundservice: singleton - não podemos injetar o mediator, que é scoped, em um singletom
    //solução: usar o IServiceProvider
    public class RegistroClienteIntegrationHandler : BackgroundService
    {
        //abstração do easynetq desenvolvida pelo dudu (tipo o mediatr) 
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;

        public RegistroClienteIntegrationHandler(
                            IServiceProvider serviceProvider, 
                            IMessageBus bus)
        {
            _serviceProvider = serviceProvider;
            _bus = bus;
        }

        private void SetResponder()
        {
            _bus.RespondAsync<UsuarioRegistradoIntegrationEvent, ResponseMessage>(async request =>
                await RegistrarCliente(request));  //retorna o responsemessage

            _bus.AdvancedBus.Connected += OnConnect;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //problema: quando o rabbitmq cai, a fila de request-response não volta, então precisa sempre conectar de novo
            SetResponder();
            return Task.CompletedTask;
        }

        private void OnConnect(object s, EventArgs e)
        {
            SetResponder();
        }

        //chama o comando para registrar o cliente via mediatr
        private async Task<ResponseMessage> RegistrarCliente(UsuarioRegistradoIntegrationEvent message)
        {
            var clienteCommand = new RegistrarClienteCommand(message.Id, message.Nome, message.Email, message.Cpf);
            ValidationResult sucesso;

            //usar um serviço scoped dentro no singleton (diferente de injeção de dependência... não é indicado para todos os cenários)
            using (var scope = _serviceProvider.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediatorHandler>();
                sucesso = await mediator.EnviarComando(clienteCommand);
            }

            return new ResponseMessage(sucesso);
        }
    }
}