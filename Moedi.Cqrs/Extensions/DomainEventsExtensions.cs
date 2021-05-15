using Moedi.Cqrs.Messages;
using System;
using System.Linq;

namespace Moedi.Cqrs.Extensions
{
    public static class DomainEventsExtensions
    {
        public static TReturn MapSingleEvent<TEvent, TReturn>(this DomainEvent[] src, Func<TEvent, TReturn> mapper)
            where TEvent : DomainEvent
        {
            return mapper(src.OfType<TEvent>().Single());
        }

        public static TReturn MapOptionalSingleEvent<TEvent, TReturn>(this DomainEvent[] src, Func<TEvent, TReturn> mapper)
            where TEvent : DomainEvent
        {
            var e = src.OfType<TEvent>().SingleOrDefault();

            if (e == null)
                return default;

            return mapper(e);
        }
    }
}
