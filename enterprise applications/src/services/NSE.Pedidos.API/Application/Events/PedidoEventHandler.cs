using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NSE.Core.Messages.Integration;
using NSE.MessageBus;

namespace NSE.Pedidos.API.Application.Events
{
    //captura a publicação em memória via mediator para dar o devido tratamento via bus
    //essa publicação vai ser escutada na api de carrinho, em um backgroundservice
    //este evento vai apagar o carrinho do cliente na api de carrinhos

    public class PedidoEventHandler : INotificationHandler<PedidoRealizadoEvent>
    {
        private readonly IMessageBus _bus;

        public PedidoEventHandler(IMessageBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(PedidoRealizadoEvent message, CancellationToken cancellationToken)
        {
            await _bus.PublishAsync(new PedidoRealizadoIntegrationEvent(message.ClienteId));
        }
    }
}