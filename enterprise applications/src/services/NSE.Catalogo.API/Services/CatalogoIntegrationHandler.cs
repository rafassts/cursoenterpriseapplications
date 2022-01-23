using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSE.Catalogo.API.Models;
using NSE.Core.DomainObjects;
using NSE.Core.Messages.Integration;
using NSE.MessageBus;

namespace NSE.Catalogo.API.Services
{
    //fila subscribe para pegar pedidos autorizados para baixar do estoque
    public class CatalogoIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceProvider _serviceProvider;

        public CatalogoIntegrationHandler(IServiceProvider serviceProvider, IMessageBus bus)
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
            _bus.SubscribeAsync<PedidoAutorizadoIntegrationEvent>("PedidoAutorizado", async request =>
                await BaixarEstoque(request));
        }

        private async Task BaixarEstoque(PedidoAutorizadoIntegrationEvent message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var produtosComEstoque = new List<Produto>(); //lista de controle para validações
                var produtoRepository = scope.ServiceProvider.GetRequiredService<IProdutoRepository>();

                var idsProdutos = string.Join(",", message.Itens.Select(c => c.Key)); 
                var produtos = await produtoRepository.ObterProdutosPorId(idsProdutos);

                if (produtos.Count != message.Itens.Count) //inconsistência na quantidade de itens no pedido
                {
                    CancelarPedidoSemEstoque(message);
                    return;
                }

                foreach (var produto in produtos)
                {
                    var quantidadeProduto = message.Itens.FirstOrDefault(p => p.Key == produto.Id).Value;
                    
                    if (produto.EstaDisponivel(quantidadeProduto))
                    {
                        produto.RetirarEstoque(quantidadeProduto);
                        produtosComEstoque.Add(produto);
                    }
                }

                if (produtosComEstoque.Count != message.Itens.Count) //se não bater os itens com estoque
                {
                    CancelarPedidoSemEstoque(message);
                    return;
                }

                foreach (var produto in produtosComEstoque)
                {
                    produtoRepository.Atualizar(produto);
                }

                if (!await produtoRepository.UnitOfWork.Commit())
                {
                    //mensagem volta para a fila para ser processada novamente (não é marcada com completada)
                    //pode ser um problema pelo acúmulo de mensagens não processadas
                    throw new DomainException($"Problemas ao atualizar estoque do pedido {message.PedidoId}");
                }

                //api de pagamento vai pegar essa mensagem
                var pedidoBaixado = new PedidoBaixadoEstoqueIntegrationEvent(message.ClienteId, message.PedidoId);
                await _bus.PublishAsync(pedidoBaixado);
            }
        }

        //api pedido e api de pagamento vão pegar essa mensagem
        public async void CancelarPedidoSemEstoque(PedidoAutorizadoIntegrationEvent message)
        {
            var pedidoCancelado = new PedidoCanceladoIntegrationEvent(message.ClienteId, message.PedidoId);
            await _bus.PublishAsync(pedidoCancelado);
        }
    }
}