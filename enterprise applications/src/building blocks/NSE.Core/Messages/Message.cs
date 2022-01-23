using System;

namespace NSE.Core.Messages
{
    public abstract class Message
    {
        //command, event, notification
        public string MessageType { get; protected set; }
        public Guid AggregateId { get; protected set; }

        protected Message()
        {
            //nome da classe de quem está herdando
            MessageType = GetType().Name;
        }
    }
}