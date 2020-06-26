using System.Threading.Tasks;
using Jasper;

namespace EMBC.ESS.Domain.Common
{
    public class JasperServiceBus : IBus, IEventPublisher, ICommandSender
    {
        private readonly IMessageContext messageContext;

        public JasperServiceBus(IMessageContext messageContext)
        {
            this.messageContext = messageContext;
        }

        public async Task PublishAsync<T>(T evt) where T : Event
        {
            await messageContext.Publish(evt);
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> command)
        {
            return await messageContext.Invoke<TResponse>(command);
        }

        public async Task SendAsync<T>(T command) where T : ICommand
        {
            await messageContext.Invoke(command);
        }
    }
}
