using System.Threading.Tasks;
using FluentValidation.Results;
using NSE.Core.Messages;

namespace NSE.Core.Mediator
{
    //abstração do mediator, para separar eventos e comandos
    public interface IMediatorHandler
    {
        Task PublicarEvento<T>(T evento) where T : Event;
        Task<ValidationResult> EnviarComando<T>(T comando) where T : Command;
    }
}