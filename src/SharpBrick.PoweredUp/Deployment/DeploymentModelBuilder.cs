using System;
using System.Collections.Generic;
using SharpBrick.PoweredUp.Hubs;

namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentModelBuilder
    {
        private List<DeploymentHubModel> _hubs = new List<DeploymentHubModel>();

        /// <summary>
        /// Add a hub with the given hub type to the model
        /// </summary>
        /// <param name="configure">Configuration fallback for the hub model.</param>
        /// <typeparam name="THub"></typeparam>
        /// <returns></returns>
        public DeploymentModelBuilder AddHub<THub>(Action<DeploymentModelHubBuilder> configure)
        {
            var hubType = HubFactory.GetSystemTypeFromType(typeof(THub));

            return AddHub(hubType, configure);
        }

        /// <summary>
        /// Add an unspecific hub to the model
        /// </summary>
        /// <param name="configure">Configuration fallback for the hub model.</param>
        /// <returns></returns>
        public DeploymentModelBuilder AddAnyHub(Action<DeploymentModelHubBuilder> configure)
            => AddHub(null, configure);

        /// <summary>
        /// Adds a hub with a given hub Type to the model.
        /// </summary>
        /// <param name="hubType"></param>
        /// <param name="configure">Configuration fallback for the hub model.</param>
        /// <returns></returns>
        public DeploymentModelBuilder AddHub(SystemType? hubType, Action<DeploymentModelHubBuilder> configure)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var hubBuilder = new DeploymentModelHubBuilder();
            hubBuilder.AddHubType(hubType);

            configure(hubBuilder);

            return AddHub(hubBuilder.Build());
        }

        /// <summary>
        /// Adds a hub to the model
        /// </summary>
        /// <param name="hubType"></param>
        /// <param name="devices"></param>
        /// <returns></returns>
        private DeploymentModelBuilder AddHub(DeploymentHubModel hubModel)
        {
            _hubs.Add(hubModel);

            return this;
        }

        /// <summary>
        /// Build the deployment model for further use.
        /// </summary>
        /// <returns></returns>
        public DeploymentModel Build()
        {
            return new DeploymentModel(_hubs.ToArray());
        }
    }
}