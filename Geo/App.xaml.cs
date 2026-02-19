namespace Geo
{
    public partial class App : Application
    {
        private readonly WeakEventManager _eventManager = new();

        public event EventHandler Resumed
        {
            add => _eventManager.AddEventHandler(value);
            remove => _eventManager.RemoveEventHandler(value);
        }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new AppShell());
        }

        protected override void OnResume()
        {
            base.OnResume();
            _eventManager.HandleEvent(this, EventArgs.Empty, nameof(Resumed));
        }
    }
}