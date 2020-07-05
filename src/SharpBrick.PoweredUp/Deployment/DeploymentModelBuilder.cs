using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Hubs;

namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentModelBuilder
    {
        private List<DeploymentHubModel> _hubs = new List<DeploymentHubModel>();

        public DeploymentModelBuilder AddHub<THub>(Action<DeploymentModelHubBuilder> configure)
        {
            var hubType = HubFactory.GetSystemTypeFromType(typeof(THub));

            return AddHub(hubType, configure);
        }

        public DeploymentModelBuilder AddAnyHub(Action<DeploymentModelHubBuilder> configure)
            => AddHub(null, configure);

        public DeploymentModelBuilder AddHub(SystemType? hubType, Action<DeploymentModelHubBuilder> configure)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var hubBuilder = new DeploymentModelHubBuilder();

            configure(hubBuilder);

            return AddHub(hubType, hubBuilder.Devices.ToArray());
        }

        private DeploymentModelBuilder AddHub(SystemType? hubType, DeploymentDeviceModel[] devices)
        {
            _hubs.Add(new DeploymentHubModel(hubType, devices));

            return this;
        }

        public DeploymentModel Build()
        {
            return new DeploymentModel(_hubs.ToArray());
        }
    }
}