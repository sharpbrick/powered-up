using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleMarioPants : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var mario = Host.FindByType<MarioHub>())
            {
                await mario.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                    .AddHub<MarioHub>(hubBuilder => { })
                );

                var pants = mario.Pants;

                var d1 = pants.PantsObservable.Subscribe(x => Log.LogWarning($"Pants: {x}"));

                await Task.Delay(20_000);

                d1.Dispose();

                await mario.SwitchOffAsync();
            }
        }
    }
}