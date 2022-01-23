//using System;
//using System.Threading.Tasks;
//using NSE.Core.Messages.Integration;
//using EasyNetQ;
//using Polly;
//using RabbitMQ.Client.Exceptions;

//namespace NSE.MessageBus
//{
//    //dá um tratamento melhor para a easynetq
//    public class MessageBusNovaVersao : IMessageBus
//    {
//        private IBus _bus;
//        private IAdvancedBus _advancedBus; //mais recursos que o bus

//        private readonly string _connectionString;

//        public MessageBus(string connectionString)
//        {
//            _connectionString = connectionString;
//            TryConnect();
//        }

//        public bool IsConnected => _bus?.Advanced.IsConnected ?? false;
//        public IAdvancedBus AdvancedBus => _bus?.Advanced; //se o bus não existir, o advanced não existe

//        public void Publish<T>(T message) where T : IntegrationEvent
//        {
//            TryConnect();
//            _bus.PubSub.Publish(message);
//        }

//        public async Task PublishAsync<T>(T message) where T : IntegrationEvent
//        {
//            TryConnect();
//            await _bus.PubSub.PublishAsync(message);
//        }

//        public void Subscribe<T>(string subscriptionId, Action<T> onMessage) where T : class
//        {
//            TryConnect();
//            _bus.PubSub.Subscribe(subscriptionId, onMessage);
//        }

//        public void SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage) where T : class
//        {
//            TryConnect();
//            _bus.PubSub.SubscribeAsync(subscriptionId, onMessage);
//        }

//        public TResponse Request<TRequest, TResponse>(TRequest request) where TRequest : IntegrationEvent
//            where TResponse : ResponseMessage
//        {
//            TryConnect();
//            return _bus.Rpc.Request<TRequest, TResponse>(request);
//        }

//        public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
//            where TRequest : IntegrationEvent where TResponse : ResponseMessage
//        {
//            TryConnect();
//            return await _bus.Rpc.RequestAsync<TRequest, TResponse>(request);
//        }

//        public IDisposable Respond<TRequest, TResponse>(Func<TRequest, TResponse> responder)
//            where TRequest : IntegrationEvent where TResponse : ResponseMessage
//        {
//            TryConnect();
//            return _bus.Rpc.Respond(responder);
//        }

//        public Task<IDisposable> RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
//            where TRequest : IntegrationEvent where TResponse : ResponseMessage
//        {
//            TryConnect();
//            return _bus.Rpc.RespondAsync(responder);
//        }

//        //conextão com o rabbitmq
//        private void TryConnect()
//        {
//            if (IsConnected) return;

//            //polly retry pattern
//            var policy = Policy.Handle<EasyNetQException>() //exceção do easynetq
//                .Or<BrokerUnreachableException>() //exceção de conexão com o rabbitmq
//                .WaitAndRetry(3, retryAttempt =>
//                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

//            policy.Execute(() =>
//            {
//                _bus = RabbitHutch.CreateBus(_connectionString); //cria o bus
//                _advancedBus = _bus.Advanced;
//                _advancedBus.Disconnected += OnDisconnect; //usado para sempre que desconectar, lançar o evento ondisconnect
//            });
//        }

//        //evento: quando desconectar, tenta imediatamente conectar
//        private void OnDisconnect(object s, EventArgs e)
//        {
//            var policy = Policy.Handle<EasyNetQException>()
//                .Or<BrokerUnreachableException>()
//                .RetryForever();

//            policy.Execute(TryConnect);
//        }

//        public void Dispose()
//        {
//            _bus.Dispose();
//        }


//    }
//}
