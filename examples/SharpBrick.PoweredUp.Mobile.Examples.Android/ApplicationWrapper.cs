namespace SharpBrick.PoweredUp.Mobile.Examples.Droid
{
    public static class ApplicationWrapper
    {
        private static App instance;

        public static App Application
        {
            get
            {
                return instance ??= new App(new AndroidInitializer());
            }
        }
    }
}