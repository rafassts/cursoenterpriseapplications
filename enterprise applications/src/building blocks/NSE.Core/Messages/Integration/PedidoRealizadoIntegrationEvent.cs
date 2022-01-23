using System;

namespace NSE.Core.Messages.Integration
{
    //mensagem publicada quando o pedido é feito na api de pedidos
    //a mensagem vai ser recebida na api de carrinho para excluir o carrinho do cliente
    public class PedidoRealizadoIntegrationEvent : IntegrationEvent
    {
        public Guid ClienteId { get; private set; }

        public PedidoRealizadoIntegrationEvent(Guid clienteId)
        {
            ClienteId = clienteId;
        }
    }
}