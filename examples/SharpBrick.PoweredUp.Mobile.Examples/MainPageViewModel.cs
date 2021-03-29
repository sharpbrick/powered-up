using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
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
        private BaseExample _example;
        private IUserDialogs _userDialogs;

        public ICommand ConnectCommand { get; }

        public MainPageViewModel(IPermissions permissions, BaseExample example, IUserDialogs userDialogs)
        {
            _permissions = permissions;
            _example = example;
            _userDialogs = userDialogs;

            ConnectCommand = new DelegateCommand(Connect);
        }

        private async void Connect()
        {
            _userDialogs.ShowLoading("Trying to connect");

            try
            {
                if (!await HasLocationPermissionAsync()) return;

                await _example.InitHostAndDiscoverAsync(false).ConfigureAwait(true);

                if (_example.SelectedHub != null)
                {
                    _userDialogs.HideLoading();
                    _userDialogs.Toast($"Connected to {_example.SelectedHub.AdvertisingName }", TimeSpan.FromSeconds(3));
                    _userDialogs.ShowLoading($"Connected to {_example.SelectedHub.AdvertisingName } - Executing action");
                    await _example.ExecuteAsync();
                    _userDialogs.Toast($"Executed action successfully", TimeSpan.FromSeconds(3));
                }
                else
                {
                    _userDialogs.Toast($"Could not connect", TimeSpan.FromSeconds(3));
                }
            }
            catch (Exception e)
            {
                _userDialogs.Toast($"Error: {e.Message}", TimeSpan.FromSeconds(3));
            }
            finally
            {
                _userDialogs.HideLoading();
            }
        }

        private async Task<bool> HasLocationPermissionAsync()
        {
            if (global::Xamarin.Forms.Device.RuntimePlatform == global::Xamarin.Forms.Device.Android)
            {
                var status = await _permissions.CheckPermissionStatusAsync<LocationPermission>().ConfigureAwait(false);
                if (status != PermissionStatus.Granted)
                {
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
