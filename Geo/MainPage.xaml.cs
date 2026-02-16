using Geo.Services;
using Mapsui.Tiling;

namespace Geo
{
    public partial class MainPage : ContentPage
    {
        private LocationTracker _tracker;
        private readonly TrackRenderer _renderer;
        private bool _isTapped;

        public MainPage()
        {
            InitializeComponent();

            MapElement.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapElement.Map?.Widgets.Clear();
            _renderer = new TrackRenderer(MapElement);
        }

        private async void ListeningFailed(object sender, GeolocationListeningFailedEventArgs e)
        {
            await DisplayAlert("Неожиданная ошибка", $"Ошибка отслеживания: {e.Error}", "OK");
        }

        public bool IsInitialized => _tracker != null && !_tracker.IsEmpty;

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            LocationTracker.ListeningFailed += ListeningFailed;
            await InitializeAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            LocationTracker.ListeningFailed -= ListeningFailed;
            _tracker.Dispose();
        }

        private async Task InitializeAsync()
        {
            ShowProgress();
            try
            {
                void AddLocationAction()
                {
                    if (_tracker == null || _tracker.IsEmpty)
                        return;

                    _renderer.Render(_tracker);
                    TitleLabel.Text = _tracker.CurrentLocation.ToString();
                }
                _tracker = new LocationTracker(AddLocationAction);
                if (!await _tracker.InitializeAsync(CancellationToken.None))
                {
                    TitleLabel.Text = "Местоположение неизвестно";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Неожиданная ошибка", ex.ToString(), "OK");
            }
            finally
            {
                HideProgress();
            }
        }

        private void ShowProgress()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
        }

        private void HideProgress()
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private async void OnTitleTapped(object sender, EventArgs e)
        {
            if (!IsInitialized || _isTapped || sender is not Label label)
                return;

            _isTapped = true;
            string coordinates = _tracker.CurrentLocation.ToString();
            await Clipboard.Default.SetTextAsync(coordinates);
            label.Text = "Скопировано в буфер обмена";
            await Task.Delay(1000);
            label.Text = coordinates;
            _isTapped = false;
        }
    }
}
