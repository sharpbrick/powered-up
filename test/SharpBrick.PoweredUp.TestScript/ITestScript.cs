using System.Threading.Tasks;
using SharpBrick.PoweredUp.Deployment;

namespace SharpBrick.PoweredUp.TestScript
{
    public interface ITestScript
    {
        void DefineDeploymentModel(DeploymentModelBuilder builder);
        Task ExecuteScriptAsync(Hub hub, TestScriptExecutionContext context);
    }
}
