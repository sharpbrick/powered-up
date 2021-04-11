using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Example;
using Microsoft.Extensions.Configuration;

namespace SharpBrick.PoweredUp.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // load a configuration object to be used when dynamically loading bluetooth adapters
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("poweredup.json", true)
                .AddCommandLine(args)
                .Build();

            // select the example to execute
            var exampleToExecute = configuration["Example"] ?? "Colors";

            var typeToExecute = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => x.FullName.StartsWith($"Example.Example{exampleToExecute}"));

            var example = Activator.CreateInstance(typeToExecute) as BaseExample;

            // initialize powered up and discover hub
            await example.InitHostAndDiscoverAsync(configuration);

            // execute example
            if (example.SelectedHub is not null)
            {
                await example.ExecuteAsync();
            }
        }
    }
}
