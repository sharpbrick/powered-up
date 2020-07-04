using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Deployment;

namespace SharpBrick.PoweredUp
{
    public static class HubExtensions
    {
        public static async Task<DeploymentModelError[]> VerifyAsync(this Hub self, Action<DeploymentModelBuilder> configure)
        {
            if (self is null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var deploymentModelBuilder = new DeploymentModelBuilder();

            configure(deploymentModelBuilder);

            var model = deploymentModelBuilder.Build();

            var errors = await model.VerifyAsync(self.Protocol);

            if (errors.Length > 0)
            {
                var logger = self.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<Hub>();

                foreach (var error in errors)
                {
                    logger.LogError($"ERROR {error.ErrorCode} @ {error.HubId}-{error.PortId}: {error.Message}");
                }
            }

            return errors;
        }
    }
}