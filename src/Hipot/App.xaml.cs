namespace Hipot
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                Console.WriteLine($"[Unhandled] {ex}");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Console.WriteLine($"[Unobserved Task] {e.Exception}");
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "Hipot" };
        }
    }
}
