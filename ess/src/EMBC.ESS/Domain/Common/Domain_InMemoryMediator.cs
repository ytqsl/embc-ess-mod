using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EMBC.ESS.Domain.Common
{
    public class InMemoryMediator : ICommandSender
    {
        private readonly Dictionary<Type, MethodInfo> commandHandlersMap;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger logger;

        public InMemoryMediator(IServiceProvider serviceProvider, Type[] handlerHostTypes, ILogger logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            commandHandlersMap = new Dictionary<Type, MethodInfo>();
            foreach (var handlerHostType in handlerHostTypes)
            {
                var handlers = GetHandlersMap(handlerHostType);
                foreach (var handler in handlers)
                {
                    if (commandHandlersMap.ContainsKey(handler.Key))
                    {
                        throw new InvalidOperationException(string.Format("Command type {0} already has a registered handler in {1}. " +
                            "Cannot add more than one command handler for each type. second host handler is {2}",
                            handler.Key.Name, commandHandlersMap[handler.Key].DeclaringType, handlerHostType.FullName));
                    }
                    commandHandlersMap.Add(handler.Key, handler.Value);
                }
            }
        }

        private IDictionary<Type, MethodInfo> GetHandlersMap(Type handlerHostType) => handlerHostType.GetMethods()
            .Where(mi => mi.Name.StartsWith("Handle") && mi.HasNumberOfParameters(1) && mi.HasParameterOfType(typeof(ICommand)))
            .GroupBy(mi => mi.GetParameters()[0].ParameterType).ToDictionary(g => g.Key, mi => mi.Single());

        public async Task SendAsync(ICommand command)
        {
            var commandType = command.GetType();
            if (!commandHandlersMap.TryGetValue(commandType, out var handler)) { return; }
            var handlerHost = serviceProvider.GetRequiredService(handler.DeclaringType);

            try
            {
                if (handler.ReturnType == typeof(Task) || handler.ReturnType == typeof(ValueTask))
                {
                    var result = (Task)handler.Invoke(handlerHost, new[] { command });
                    await result;
                }
                else
                {
                    handler.Invoke(handlerHost, new[] { command });
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception when invoking {0}.{1}", handler.DeclaringType.FullName, handler.Name);
                throw;
            }
        }

        public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command)
        {
            var commandType = command.GetType();
            if (!commandHandlersMap.TryGetValue(commandType, out var handler))
            {
                throw new InvalidOperationException($"No handler found for command '{commandType.FullName}'");
            }
            var handlerHost = serviceProvider.GetRequiredService(handler.DeclaringType);

            if (handler.ReturnType.BaseType == typeof(Task) || handler.ReturnType.BaseType == typeof(ValueTask))
            {
                var result = (Task<TResponse>)handler.Invoke(handlerHost, new[] { command });
                return await result;
            }
            else
            {
                return (TResponse)handler.Invoke(handlerHost, new[] { command });
            }
        }
    }

    public static class InMemoryMediatorConfigurationEx
    {
        public static IServiceCollection AddInMemoryMediator(this IServiceCollection services)
        {
            var handlerHostTypes = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => t.Name.EndsWith("Handler")).ToArray();
            return services.AddInMemoryMediator(handlerHostTypes);
        }

        public static IServiceCollection AddInMemoryMediator(this IServiceCollection services, Type[] commandHandlerHosts)
        {
            foreach (var handlerType in commandHandlerHosts)
            {
                services.AddTransient(handlerType);
            }
            services.AddSingleton<ICommandSender, InMemoryMediator>(sp => new InMemoryMediator(sp, commandHandlerHosts, sp.GetRequiredService<ILogger<InMemoryMediator>>()));

            return services;
        }
    }

    public static class ReflectionEx
    {
        public static bool HasParameterOfType(this MethodInfo mi, Type parameterType)
        {
            return mi.GetParameters().Any(p => parameterType.IsAssignableFrom(p.ParameterType));
        }

        public static bool HasNumberOfParameters(this MethodInfo mi, int numberOfParameters)
        {
            return mi.GetParameters().Length == numberOfParameters;
        }
    }
}
