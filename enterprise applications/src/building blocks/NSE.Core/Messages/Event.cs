using System;
using MediatR;

namespace NSE.Core.Messages
{
    //INotification - notificação do mediator (mediator.Publish)
    public class Event : Message, INotification
    {
        public DateTime Timestamp { get; private set; }

        protected Event()
        {
            Timestamp = DateTime.Now;
        }
    }
}