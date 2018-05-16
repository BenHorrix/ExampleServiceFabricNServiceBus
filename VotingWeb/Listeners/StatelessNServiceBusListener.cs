using System;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;
using Handlers;
using Messages;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using NServiceBus;

namespace VotingWeb.Listeners
{
    internal class StatelessNServiceBusListener : ICommunicationListener
    {
        private const string RabbitMqSettingsSectionName = "RabbitMQ";
        private const string RabbitMqConnectionStringSettingName = "RabbitMQConnectionString";
        private readonly string _rabbitConnectionString = null;
        private readonly StatelessServiceContext _serviceContext;
        private readonly VotingWeb _hostApplication;

        public StatelessNServiceBusListener(StatelessServiceContext context, VotingWeb votingWeb)
        {
            _hostApplication = votingWeb ?? throw new ArgumentNullException(nameof(votingWeb));
            _serviceContext = context ?? throw new ArgumentNullException(nameof(context));

            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            ConfigurationSection rabbitMqConfigSection = configurationPackage.Settings.Sections[RabbitMqSettingsSectionName];
            ConfigurationProperty rabbitMqConnectionString = rabbitMqConfigSection.Parameters[RabbitMqConnectionStringSettingName];

            if (rabbitMqConfigSection == null)
            {
                throw new Exception($"No service fabric configuration section called {RabbitMqSettingsSectionName}");
            }

            if (rabbitMqConnectionString == null)
            {
                throw new Exception($"No service fabric configuration section within {RabbitMqSettingsSectionName} called {RabbitMqConnectionStringSettingName} was found");
            }

            _rabbitConnectionString = rabbitMqConnectionString.Value;

            if (string.IsNullOrEmpty(_rabbitConnectionString))
            {
                throw new Exception($"The setting {RabbitMqSettingsSectionName}/{RabbitMqConnectionStringSettingName} was found but was empty. This must be a valid RabbitMQ connection string");
            }
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            string endpointName = "voting";
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.EnableInstallers();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(_rabbitConnectionString);
            transport.UseConventionalRoutingTopology();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(CastVote).Assembly, endpointName);

            VotingWeb.EndpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            ServiceEventSource.Current.ServiceMessage(_serviceContext, $"The {nameof(StatelessNServiceBusListener)} has been instantiated");

            return endpointName;
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return VotingWeb.EndpointInstance?.Stop();
        }

        public void Abort()
        {
            CloseAsync(CancellationToken.None);
        }
    }
}
