using Geo.Services;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Objects;

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
                    AddUserLocationPoint();
                }
                else
                    TitleLabel.Text = "Местоположение неизвестно";
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

        private void AddUserLocationPoint()
        {
            var coo = _currentLocation.ToSphericalMercator();
            var point = new NetTopologySuite.Geometries.Point(coo.X, coo.Y);
            var feature = new GeometryFeature { Geometry = point };

            feature.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromString("#00FF00")),
                SymbolScale = 2.0,
                Opacity = 0.1f,
                Outline = null
            });
            feature.Styles.Add(new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromString("#FF5E5E")),
                SymbolScale = 0.6,
                Opacity = 1.0f,
                Outline = null
            });

            var userLocationLayer = new MemoryLayer
            {
                Name = "CurrentLocationLayer",
                Features = [feature]
            };
            MapElement.Map?.Layers.Add(userLocationLayer);
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
            label.Text = "Скопировано в буфер обмена";
            await Task.Delay(1000);
            label.Text = coordinates;
            _isTapped = false;
        }
    }
}
