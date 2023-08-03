namespace Robocraft2BlockPenSimApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            window.Title = "Robocraft 2 Block Penetration Simulator";
            return window;
        }
    }
}