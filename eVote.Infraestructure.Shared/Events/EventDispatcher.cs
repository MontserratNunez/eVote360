using Microsoft.Extensions.DependencyInjection;
using eVote360.Core.Domain.Interfaces;
using eVote360.Core.Application.Interfaces;

namespace eVote360.Infraestructure.Shared.Events
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _provider;

        public EventDispatcher(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task DispatchAsync<TEvent>(TEvent domainEvent)
        {
            var handlers = _provider.GetServices<IEventHandler<TEvent>>();

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(domainEvent);
            }
        }
    }
}
