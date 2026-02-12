using Mapsui.Tiling;
using Mapsui.UI.Maui;

namespace Geo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var mapControl = new MapControl();
            mapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
            Content = mapControl;
        }
    }
}
