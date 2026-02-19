using Geo.Services;
using Mapsui.Tiling;
using Mapsui.UI.Maui;

namespace Geo
{
    public partial class MainPage : ContentPage
    {
        private LocationTracker _tracker;
        private bool _isTapped;

        public MainPage()
        {
            InitializeComponent();

            // Disable automatic screen turn-off
            DeviceDisplay.Current.KeepScreenOn = true;

            var mapControl = new MapControl();
            mapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
            mapControl.Map?.Widgets.Clear();
            Content = mapControl;

            ((App)App.Current).Resumed += async (s, e) => await OnAppResumedAsync();
        }

        public MapControl MapElement => Content as MapControl;

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
            _tracker = null;
        }

        public async Task OnAppResumedAsync()
        {
            if (!IsInitialized || IsBusy)
                return;

            IsBusy = true;
            TitleLabel.Text = "Определение местоположения...";
            try
            {
                if (!await _tracker.FetchCurrentLocation(CancellationToken.None))
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
                IsBusy = false;
            }
        }

        private async Task InitializeAsync()
        {
            IsBusy = true;
            TitleLabel.Text = "Определение местоположения...";
            try
            {
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
                IsBusy = false;
            }
        }

        void AddLocationAction()
        {
            if (_tracker == null || _tracker.IsEmpty)
                return;

            using var renderer = new TrackRenderer(MapElement);
            renderer.Render(_tracker);
            TitleLabel.Text = _tracker.CurrentLocation.ToString();
        }

        private async void OnTitleTapped(object sender, EventArgs e)
        {
            if (!IsInitialized || _isTapped || sender is not Label label)
                return;

            _isTapped = true;
            string originalText = label.Text;

            try
            {
                string coordinates = _tracker.CurrentLocation.ToString();
                await Clipboard.Default.SetTextAsync(coordinates);

                label.Text = "Скопировано в буфер обмена";
                await Task.Delay(1000);
                label.Text = originalText;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Clipboard error: {ex}");
                await DisplayAlert("Ошибка", $"Ошибка копирования в буфер обмена", "OK");
                label.Text = originalText;
            }
            finally
            {
                _isTapped = false;
            }
        }
    }
}
