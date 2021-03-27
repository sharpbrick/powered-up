using System.Diagnostics;
using Acr.UserDialogs;
using Example;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Prism;
using Prism.Ioc;
using Prism.Unity;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Mobile.Examples
{
    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer platformInitializer) : base(platformInitializer)
        {
            InitializeComponent();
        }

        protected override async void OnStart()
        {
            var result = await NavigationService.NavigateAsync(nameof(MainPage)).ConfigureAwait(false);
#if DEBUG
            if (result.Exception != null)
            {
                Debugger.Break();
            }
#endif
        }

        protected override void OnSleep()
        {
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IBluetoothLE>(CrossBluetoothLE.Current);
            containerRegistry.RegisterInstance<IUserDialogs>(UserDialogs.Instance);

            containerRegistry.RegisterSingleton<IPoweredUpBluetoothAdapter, XamarinPoweredUpBluetoothAdapter>();

            containerRegistry.RegisterSingleton<ExampleMoveHubColors>();

            // base navigation
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
        }

        protected override void OnInitialized()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
