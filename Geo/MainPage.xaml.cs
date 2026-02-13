using Geo.Services;
using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;
using System.Globalization;

namespace Geo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            MapElement.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapElement.Map?.Widgets.Clear();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            ShowProgress();

            var location = await Locator.GetCurrentLocationAsync(CancellationToken.None);
            if (location != null)
            {
                var latitude = location.Latitude.ToString(CultureInfo.InvariantCulture);
                var longitude = location.Longitude.ToString(CultureInfo.InvariantCulture);
                TitleLabel.Text = $"{latitude}, {longitude}";

                DisplayLocation(location);
            }

            HideProgress();
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

        private bool DisplayLocation(Location location)
        {
            ArgumentNullException.ThrowIfNull(location, nameof(location));

            try
            {
                // Transform coordinates
                var point = SphericalMercator.FromLonLat(new MPoint(location.Longitude, location.Latitude));

                // Move map to coordinates
                MapElement.Map?.Navigator?.CenterOnAndZoomTo(point, 4, 1000);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void OnTitleTapped(object sender, EventArgs e)
        {
            if (sender is not Label label)
                return;

            string coordinates = label.Text;
            await Clipboard.Default.SetTextAsync(coordinates);
            label.Text = "Скопировано в буфер обмена!";
            await Task.Delay(1000);
            label.Text = coordinates;
        }
    }
}
