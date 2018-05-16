using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Messages;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using NServiceBus;

namespace VotingData.Listeners
{
    public class StatefulNServiceBusListener : ICommunicationListener
    {
        private const string RabbitMqSettingsSectionName = "RabbitMQ";
        private const string RabbitMqConnectionStringSettingName = "RabbitMQConnectionString";
        private readonly string _rabbitConnectionString = null;
        private readonly StatefulServiceContext _serviceContext;
        private EndpointConfiguration _endpointConfiguration;

        public StatefulNServiceBusListener(StatefulServiceContext stateManager)
        {
            this._serviceContext = stateManager;

            _serviceContext = stateManager ?? throw new ArgumentNullException(nameof(stateManager));

            var configurationPackage = _serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config");

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
            _endpointConfiguration = new EndpointConfiguration(endpointName);
            _endpointConfiguration.EnableInstallers();

            var transport = _endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(_rabbitConnectionString);
            transport.UseConventionalRoutingTopology();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(CastVote).Assembly, endpointName);

            VotingData.EndpointInstance = await Endpoint.Start(_endpointConfiguration).ConfigureAwait(false);

            ServiceEventSource.Current.ServiceMessage(_serviceContext, $"The {nameof(StatefulNServiceBusListener)} has been instantiated");

            return endpointName;
        }

        public async Task RunAsync()
        {
            VotingData.EndpointInstance = await Endpoint.Start(_endpointConfiguration)
                .ConfigureAwait(false);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return VotingData.EndpointInstance?.Stop();
        }

        public void Abort()
        {
            CloseAsync(CancellationToken.None);
        }
    }
}
