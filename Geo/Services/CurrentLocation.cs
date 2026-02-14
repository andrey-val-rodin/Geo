using Mapsui;
using Mapsui.Projections;
using System.Globalization;

namespace Geo.Services
{
    public sealed class CurrentLocation
    {
        private Location _location;

        public bool IsInitialized => _location != null;

        public async Task<bool> InitializeAsync(CancellationToken token)
        {
            _location = await GetCurrentLocationAsync(token);
            return IsInitialized;
        }

        public MPoint ToSphericalMercator()
        {
            return _location == null
                ? null
                : SphericalMercator.FromLonLat(ToLonLat());
        }

        public MPoint ToLonLat()
        {
            return _location == null
                ? null
                : new MPoint(_location.Longitude, _location.Latitude);
        }

        public override string ToString()
        {
            var latitude = _location.Latitude.ToString("0.######", CultureInfo.InvariantCulture);
            var longitude = _location.Longitude.ToString("0.######", CultureInfo.InvariantCulture);
            return $"{latitude}, {longitude}";
        }

        private static async Task<Location> GetCurrentLocationAsync(CancellationToken token)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                return await Geolocation.Default.GetLocationAsync(request, token);
            }
            catch (FeatureNotSupportedException) { }
            catch (FeatureNotEnabledException) { }
            catch (PermissionException) { }

            return null;
        }
    }
}
