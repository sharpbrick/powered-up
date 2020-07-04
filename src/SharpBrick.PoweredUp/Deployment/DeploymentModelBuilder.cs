using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Hubs;

namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentModelBuilder
    {
        private List<DeploymentHubModel> _hubs = new List<DeploymentHubModel>();

        public DeploymentModelBuilder AddHub<THub>(Action<DeploymentModelHubBuilder<THub>> configure) where THub : Hub
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var hubBuilder = new DeploymentModelHubBuilder<THub>();

            configure(hubBuilder);

            var hubType = HubFactory.GetSystemTypeFromType(typeof(THub));

            _hubs.Add(new DeploymentHubModel(hubType, hubBuilder.Devices.ToArray()));

            return this;
        }

        public DeploymentModel Build()
        {
            return new DeploymentModel(_hubs.ToArray());
        }
    }
}