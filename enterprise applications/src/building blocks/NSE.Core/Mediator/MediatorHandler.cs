﻿using FluentValidation.Results;
using NSE.Core.Messages;
using System.Threading.Tasks;
using MediatR;

namespace NSE.Core.Mediator
{
    public class MediatorHandler : IMediatorHandler
    {
        private readonly IMediator _mediator; //mediator do pacote

        public MediatorHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ValidationResult> EnviarComando<T>(T comando) where T : Command
        {
            return await _mediator.Send(comando); //send pode retornar
        }

        public async Task PublicarEvento<T>(T evento) where T : Event
        {
            await _mediator.Publish(evento); //publish não retorna nada
        }
    }
}