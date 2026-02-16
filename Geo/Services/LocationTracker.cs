using Geo.Model;

namespace Geo.Services
{
    public partial class LocationTracker(Action addLocationAction = null) : IDisposable
    {
        public static event EventHandler<GeolocationListeningFailedEventArgs> ListeningFailed
        {
            add => Geolocation.ListeningFailed += value;
            remove => Geolocation.ListeningFailed -= value;
        }

        protected readonly List<GeoPosition> _previousLocations = [];
        private bool disposedValue;

        public IReadOnlyList<GeoPosition> PreviousLocations => _previousLocations as IReadOnlyList<GeoPosition>;
        public GeoPosition CurrentLocation { get; private set; }
        public bool IsEmpty => CurrentLocation == null;
        public Action AddLocationAction { get; set; } = addLocationAction;

        public async Task<bool> InitializeAsync(CancellationToken token)
        {
            try
            {
                var currentLocation = await GetCurrentLocationAsync(token);
                AddLocation(currentLocation);

                return await StartListeningAsync();
            }
            catch (TaskCanceledException) when (token.IsCancellationRequested)
            {
                return false;
            }
        }

        private static async Task<GeoPosition> GetCurrentLocationAsync(CancellationToken token)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request, token);
                return location == null ? null : new GeoPosition(location);
            }
            catch (FeatureNotSupportedException) { }
            catch (FeatureNotEnabledException) { }
            catch (PermissionException) { }

            return null;
        }

        protected async Task<bool> StartListeningAsync()
        {
            var request = new GeolocationListeningRequest(
                GeolocationAccuracy.Best,
                TimeSpan.FromSeconds(5)
            );

            Geolocation.LocationChanged += LocationChanged;
            var success = await Geolocation.StartListeningForegroundAsync(request);
            if (!success)
            {
                Geolocation.LocationChanged -= LocationChanged;
            }

            return success;
        }

        protected void StopListening()
        {
            Geolocation.LocationChanged -= LocationChanged;
            Geolocation.StopListeningForeground();
        }

        protected void LocationChanged(object sender, GeolocationLocationChangedEventArgs e)
        {
            AddLocation(new GeoPosition(e.Location));
        }

        public void AddLocation(GeoPosition location)
        {
            ArgumentNullException.ThrowIfNull(location, nameof(location));

            if (CurrentLocation != null)
            {
                _previousLocations.Add(CurrentLocation);
            }
            CurrentLocation = location;

            AddLocationAction?.Invoke();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopListening();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
