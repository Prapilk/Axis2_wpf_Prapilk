using Axis2.WPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
    

namespace Axis2.WPF.Mvvm
{
    public class EventAggregator
    {
        private readonly Dictionary<Type, List<WeakReference>> _subscribers = new Dictionary<Type, List<WeakReference>>();

        // Méthode Subscribe pour les IHandler<TMessage>
        public void Subscribe(object subscriber)
        {
            var subscriberTypes = subscriber.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<>));
            foreach (var subscriberType in subscriberTypes)
            {
                var messageType = subscriberType.GetGenericArguments()[0];
                if (!_subscribers.ContainsKey(messageType))
                {
                    _subscribers[messageType] = new List<WeakReference>();
                }
                _subscribers[messageType].Add(new WeakReference(subscriber));
            }
        }

        // Méthode Subscribe pour les Action<TMessage>
        public void Subscribe<TMessage>(Action<TMessage> action)
        {
            var messageType = typeof(TMessage);
            if (!_subscribers.ContainsKey(messageType))
            {
                _subscribers[messageType] = new List<WeakReference>();
            }
            _subscribers[messageType].Add(new WeakReference(action));
        }

        public void Publish<TMessage>(TMessage message)
        {
            var messageType = typeof(TMessage);
            if (_subscribers.ContainsKey(messageType))
            {
                var deadReferences = new List<WeakReference>();
                foreach (var reference in _subscribers[messageType])
                {
                    if (reference.IsAlive)
                    {
                        // Tente de caster vers IHandler<TMessage>
                        var handler = reference.Target as IHandler<TMessage>;
                        if (handler != null)
                        {
                            Logger.Log($"DEBUG: EventAggregator - Invoking handler for {messageType.Name}");
                            handler.Handle(message);
                        }
                        else
                        {
                            // Tente de caster vers Action<TMessage>
                            var action = reference.Target as Action<TMessage>;
                            action?.Invoke(message);
                        }
                    }
                    else
                    {
                        deadReferences.Add(reference);
                    }
                }

                foreach (var deadReference in deadReferences)
                {
                    _subscribers[messageType].Remove(deadReference);
                }
            }
        }
    }

    public interface IHandler<TMessage>
    {
        void Handle(TMessage message);
    }
}