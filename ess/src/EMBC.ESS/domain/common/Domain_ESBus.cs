using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace EMBC.ESS.Domain.Common
{
    public class Domain_ESBus : IBus
    {
        private readonly IEventStoreConnection conn;

        public Domain_ESBus(IEventStoreConnection conn)
        {
            this.conn = conn;
            var handlers = Assembly.GetExecutingAssembly().ExportedTypes.Where(t =>
            t.GetMethods().Any(m => m.IsPublic && m.Name.StartsWith("Handle") && m.GetParameters().Length == 1 && typeof(Event).IsAssignableFrom(m.GetParameters()[0].ParameterType)));
        }

        public async Task PublishAsync<T>(T evt) where T : Event
        {
            await Task.CompletedTask;
        }

        public async Task SendAsync(ICommand command)
        {
            await Task.CompletedTask;
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> command)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
