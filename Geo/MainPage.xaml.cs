using Geo.Services;
using Mapsui.Tiling;

namespace Geo
{
    public partial class MainPage : ContentPage
    {
        private readonly CurrentLocation _currentLocation = new();
        private bool _isTapped;

        public MainPage()
        {
            InitializeComponent();

            MapElement.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapElement.Map?.Widgets.Clear();
        }

        public bool IsInitialized => _currentLocation.IsInitialized;

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            ShowProgress();
            try
            {
                await _currentLocation.InitializeAsync(CancellationToken.None);
                if (_currentLocation.IsInitialized)
                {
                    MapElement.Map?.Navigator?.CenterOnAndZoomTo(_currentLocation.ToSphericalMercator(), 4, 1000);
                    TitleLabel.Text = _currentLocation.ToString();
                }
                else
                    TitleLabel.Text = "Местоположение неизвестно";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Неожиданная ошибка", "OK", ex.ToString());
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
            string coordinates = _currentLocation.ToString();
            await Clipboard.Default.SetTextAsync(coordinates);
            label.Text = "Скопировано в буфер обмена!";
            await Task.Delay(1000);
            label.Text = coordinates;
            _isTapped = false;
        }
    }
}
