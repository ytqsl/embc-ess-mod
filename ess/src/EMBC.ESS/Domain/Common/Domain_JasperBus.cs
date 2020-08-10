using System.Threading.Tasks;
using Jasper;
using Microsoft.Extensions.DependencyInjection;

namespace EMBC.ESS.Domain.Common.Jasper
{
    public class JasperServiceBus : IBus
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

        public async Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> command)
        {
            return await messageContext.Invoke<TResponse>(command);
        }

        public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command)
        {
            return await messageContext.Invoke<TResponse>(command);
        }

        public async Task SendAsync(ICommand command)
        {
            await messageContext.Invoke(command);
        }
    }

    public static class JasperServiceBusConfigEx
    {
        public static IServiceCollection AddJasperMessageBus(this IServiceCollection services)
        {
            services.AddTransient<IEventPublisher, JasperServiceBus>();
            services.AddTransient<ICommandSender, JasperServiceBus>();
            services.AddTransient<IQuerySender, JasperServiceBus>();

            return services;
        }
    }
}
