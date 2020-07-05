using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Deployment;

namespace SharpBrick.PoweredUp
{
    public static class HubExtensions
    {
        public static async Task VerifyDeploymentModelAsync(this Hub self, Action<DeploymentModelBuilder> configure)
        {
            if (self is null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            var model = BuildModel(configure);

            var awaitable = self.VerifyObservable(model)
                .Do(LogErrors(self))
                .Where(x => x.Length == 0)
                .FirstAsync()
                .GetAwaiter();

            var firstErors = model.Verify(self.Protocol);

            if (firstErors.Length > 0)
            {
                LogErrors(self)(firstErors);

                await awaitable;
            }
        }

        private static Action<DeploymentModelError[]> LogErrors(Hub self)
        {
            return errors =>
            {
                var logger = self.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<Hub>();
                if (errors.Length > 0)
                {

                    foreach (var error in errors)
                    {
                        logger.LogError($"ERROR {error.ErrorCode} @ {error.HubId}-{error.PortId}: {error.Message}");
                    }
                }
                else
                {
                    logger.LogInformation("Deployment Validation Successful");
                }
            };
        }

        public static IObservable<DeploymentModelError[]> VerifyObservable(this Hub self, Action<DeploymentModelBuilder> configure)
        {
            if (self is null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            var model = BuildModel(configure);

            return self.VerifyObservable(model);
        }

        public static IObservable<DeploymentModelError[]> VerifyObservable(this Hub self, DeploymentModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return self.Protocol.UpstreamMessages.Select(msg => model.Verify(self.Protocol));
        }

        private static DeploymentModel BuildModel(Action<DeploymentModelBuilder> configure)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var deploymentModelBuilder = new DeploymentModelBuilder();

            configure(deploymentModelBuilder);

            var model = deploymentModelBuilder.Build();
            return model;
        }
    }
}