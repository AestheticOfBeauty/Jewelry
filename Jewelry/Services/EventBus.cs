using Jewelry.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jewelry.Services
{
    public class EventBus
    {
        private Dictionary<EventSubscriber, Action<IEvent>> _subscribers;

        public EventBus()
        {
            _subscribers = new Dictionary<EventSubscriber, Action<IEvent>>();
        }

        public void Publish<T>(T message) where T : IEvent
        {
            var messageType = typeof(T);

            var handlers = _subscribers
                .Where(s => s.Key.MessageType == messageType)
                .Select(s => s.Value);

            foreach (Action<IEvent> handler in handlers)
            {
                handler?.Invoke(message);
            }

        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var disposer = new EventSubscriber(typeof(T), s => _subscribers.Remove(s));
            _subscribers.Add(disposer, (item) => handler((T)item));
            return disposer;
        }
    }
}
