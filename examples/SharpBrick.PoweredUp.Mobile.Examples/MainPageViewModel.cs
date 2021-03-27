using System.Threading.Tasks;
using System.Windows.Input;
using Example;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Prism.Commands;
using SharpBrick.PoweredUp.Mobile.Examples.Examples;

namespace SharpBrick.PoweredUp.Mobile.Examples
{
    public class MainPageViewModel
    {
        private IPermissions _permissions;
        private ExampleMoveHubColors _example;
        public ICommand ConnectCommand { get; }

        public MainPageViewModel(IPermissions permissions, ExampleMoveHubColors example)
        {
            _permissions = permissions;
            _example = example;

            ConnectCommand = new DelegateCommand(Connect);
        }

        private async void Connect()
        {
            if (! await HasLocationPermissionAsync()) return;

            await _example.InitHostAndDiscoverAsync(false);
        }

        private async Task<bool> HasLocationPermissionAsync()
        {
            if (global::Xamarin.Forms.Device.RuntimePlatform == global::Xamarin.Forms.Device.Android)
            {
                var status = await _permissions.CheckPermissionStatusAsync<LocationPermission>().ConfigureAwait(false);
                if (status != PermissionStatus.Granted)
                {
                    bool shouldShow = await _permissions.ShouldShowRequestPermissionRationaleAsync(Permission.Location).ConfigureAwait(false);
                    if (!shouldShow)
                    {
                        _permissions.OpenAppSettings();
                        return false;
                    }

                    var permissionResult = await _permissions.RequestPermissionAsync<LocationPermission>().ConfigureAwait(false);

                    if (permissionResult != PermissionStatus.Granted)
                    {
                        _permissions.OpenAppSettings();
                        return false;
                    }
                }
            }

            return true;
        }

    }
}
