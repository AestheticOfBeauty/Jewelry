using Jewelry.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jewelry.Services
{
    public class MessageBus
    {
        private readonly Dictionary<MessageSubscriber, Action<IMessage>> _receivers;

        public MessageBus()
        {
            _receivers = new Dictionary<MessageSubscriber, Action<IMessage>>();
        }

        public void SendTo<T>(IMessage message)
        {
            var messageType = message.GetType();
            var receiverType = typeof(T);

            var tasks = _receivers
                .Where(r => r.Key.MessageType == messageType
                            && r.Key.ReceiverType == receiverType)
                .Select(r => r.Value);

            foreach (Action<IMessage> task in tasks)
            {
                task.Invoke(message);
            }
        }

        public IDisposable Receive<TMessage>(object receiver, Action<TMessage> handler)
                           where TMessage : IMessage
        {
            var subscriber = new MessageSubscriber(receiver.GetType(), typeof(TMessage), r => _receivers.Remove(r));
            _receivers.Add(subscriber, (item) => handler((TMessage)item));
            return subscriber;
        }
    }
}
