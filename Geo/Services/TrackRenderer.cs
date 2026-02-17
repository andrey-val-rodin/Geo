using Geo.Model;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.UI.Maui;

namespace Geo.Services
{
    public partial class TrackRenderer : IDisposable
    {
        private const string CPL_NAME = "CurrentPointLayer";
        private const string PPL_NAME = "PreviousPointsLayer";

        private readonly MemoryLayer _currentPointLayer;
        private readonly MemoryLayer _previousPointsLayer;
        private bool _disposed;

        public TrackRenderer(MapControl control)
        {
            ArgumentNullException.ThrowIfNull(control, nameof(control));

            MapControl = control;

            // Get or create layers
            _previousPointsLayer = MapControl.Map?.Layers.FirstOrDefault(l => l.Name == PPL_NAME) as MemoryLayer;
            if (_previousPointsLayer == null)
            {
                _previousPointsLayer = new MemoryLayer { Name = PPL_NAME, Style = null };
                MapControl.Map?.Layers.Add(_previousPointsLayer);
            }

            _currentPointLayer = MapControl.Map?.Layers.FirstOrDefault(l => l.Name == CPL_NAME) as MemoryLayer;
            if ( _currentPointLayer == null)
            {
                _currentPointLayer = new MemoryLayer { Name = CPL_NAME };
                MapControl.Map?.Layers.Add(_currentPointLayer);
            }
        }

        public MapControl MapControl { get; private set; }

        public void Render(LocationTracker tracker)
        {
            if (tracker == null || tracker.IsEmpty)
                return;

            var currentLocation = tracker?.CurrentLocation;
            var previousLocations = tracker?.PreviousLocations;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AddPreviousPoints(previousLocations);
                AddCurrentPoint(currentLocation);

                _previousPointsLayer.DataHasChanged();
                _currentPointLayer.DataHasChanged();
                MapControl.Map?.Navigator?.CenterOnAndZoomTo(currentLocation.ToSphericalMercator(), 4, 1000);
            });
        }

        private void AddPreviousPoints(IEnumerable<GeoPosition> locations)
        {
            var features = new List<GeometryFeature>();
            foreach (var location in locations)
            {
                var point = ToGeometriesPoint(location);
                var feature = new GeometryFeature { Geometry = point };
                feature.Styles.Add(new SymbolStyle
                {
                    SymbolType = SymbolType.Ellipse,
                    Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromString("#0000FF")),
                    SymbolScale = 0.2,
                    Opacity = 1.0f,
                    Outline = null
                });
                features.Add(feature);
            }
            _previousPointsLayer.Features = features;
        }

        private void AddCurrentPoint(GeoPosition location)
        {
            var point = ToGeometriesPoint(location);
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

            _currentPointLayer.Features = [feature];
        }

        private static NetTopologySuite.Geometries.Point ToGeometriesPoint(GeoPosition location)
        {
            var point = location.ToSphericalMercator();
            return new NetTopologySuite.Geometries.Point(point.X, point.Y);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _currentPointLayer.Dispose();
                    _previousPointsLayer.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
