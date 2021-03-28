using System.Diagnostics;
using Acr.UserDialogs;
using Example;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Prism;
using Prism.Ioc;
using Prism.Unity;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Mobile.Examples.Examples;

namespace SharpBrick.PoweredUp.Mobile.Examples
{
    public partial class App : PrismApplication
    {
        public static INativeDeviceInfo NativeDeviceInfo { get; private set; }

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

            // containerRegistry.RegisterSingleton<BaseExample, ExampleBluetoothByKnownAddress>(); // doesn't work atm
            containerRegistry.RegisterSingleton<BaseExample, ExampleMoveHubColors>();
            // containerRegistry.RegisterSingleton<BaseExample, ExampleMoveHubInternalTachoMotorControl>();

            // base navigation
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
        }

        protected override void OnInitialized()
        {
            NativeDeviceInfo = Container.Resolve<INativeDeviceInfo>();
        }

        protected override void OnResume()
        {
        }
    }
}
