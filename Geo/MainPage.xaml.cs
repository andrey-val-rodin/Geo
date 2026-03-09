using Geo.Model;
using Geo.Services;
using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.UI.Maui;
using Mapsui.Widgets.ButtonWidgets;

namespace Geo
{
    public partial class MainPage : ContentPage
    {
        private LocationTracker _tracker;

        public MainPage()
        {
            InitializeComponent();

            // Disable automatic screen turn-off
            DeviceDisplay.Current.KeepScreenOn = true;

            var mapControl = new MapControl();
            mapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
            mapControl.Map?.Widgets.Clear();
            Content = mapControl;
#if WINDOWS
            var widget = new ZoomInOutWidget
            {
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Right,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Center
            };
            MapControl.Map.Widgets.Add(widget);
#endif

            ((App)App.Current).Resumed += async (s, e) => await OnAppResumedAsync();
        }

        public MapControl MapControl => Content as MapControl;

        private async void ListeningFailed(object sender, GeolocationListeningFailedEventArgs e)
        {
            await DisplayAlert("Неожиданная ошибка", $"Ошибка отслеживания: {e.Error}", "OK");
        }

        public bool IsInitialized => _tracker != null && !_tracker.IsEmpty;

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await StartTrackingAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopTracking();
        }

        public async Task OnAppResumedAsync()
        {
            if (!IsInitialized || IsBusy)
                return;

            await StartTrackingAsync();
        }

        private async Task StartTrackingAsync()
        {
            IsBusy = true;
            TitleLabel.Text = "Определение местоположения...";
            LocationTracker.ListeningFailed -= ListeningFailed;
            LocationTracker.ListeningFailed += ListeningFailed;
            try
            {
                if (_tracker == null)
                {
                    _tracker = new LocationTracker();
                    _tracker.LocationAdded += LocationAdded;
                    if (!await _tracker.InitializeAsync(CancellationToken.None))
                    {
                        TitleLabel.Text = "Местоположение неизвестно";
                    }
                }
                else
                {
                    if (!await _tracker.FetchCurrentLocation(CancellationToken.None))
                    {
                        TitleLabel.Text = "Местоположение неизвестно";
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Неожиданная ошибка", ex.ToString(), "OK");
                TitleLabel.Text = "Ошибка";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void StopTracking()
        {
            LocationTracker.ListeningFailed -= ListeningFailed;
            if (_tracker != null)
            {
                _tracker.LocationAdded -= LocationAdded;
                _tracker.Dispose();
                _tracker = null;
            }
        }

        private void LocationAdded(object sender, LocationAddedEventArgs e)
        {
            if (sender is not LocationTracker tracker)
                return;

            using var renderer = new TrackRenderer(MapControl);
            renderer.Render(tracker);
            MainThread.BeginInvokeOnMainThread(() => TitleLabel.Text = tracker.CurrentLocation.ToString());
        }

        private async void OnTitleTapped(object sender, EventArgs e)
        {
            if (IsBusy || sender is not Label label)
                return;

            IsBusy = true;
            string originalText = label.Text;

            try
            {
                string coordinates = _tracker.CurrentLocation.ToString();
                await Clipboard.Default.SetTextAsync(coordinates);

                label.Text = "Скопировано в буфер обмена";
                using var renderer = new TrackRenderer(MapControl);
                renderer.Render(_tracker);

                await Task.Delay(1000);
                label.Text = originalText;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Неожиданная ошибка", ex.ToString(), "OK");
                label.Text = originalText;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
