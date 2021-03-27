using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Prism;
using Prism.Ioc;

namespace SharpBrick.PoweredUp.Mobile.Examples.Droid
{
    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IPermissions>(CrossPermissions.Current);
        }
    }
}