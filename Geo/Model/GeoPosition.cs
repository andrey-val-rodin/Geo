using Mapsui;
using Mapsui.Projections;
using System.Globalization;

namespace Geo.Model
{
    public sealed record GeoPosition
    {
        public GeoPosition(Location location)
        {
            ArgumentNullException.ThrowIfNull(location, nameof(location));

            Location = location;
        }

        public Location Location { get; init; }

        public MPoint ToSphericalMercator()
        {
            return SphericalMercator.FromLonLat(ToLonLat());
        }

        public MPoint ToLonLat()
        {
            return new MPoint(Location.Longitude, Location.Latitude);
        }

        public override string ToString()
        {
            var latitude = Location.Latitude.ToString("0.######", CultureInfo.InvariantCulture);
            var longitude = Location.Longitude.ToString("0.######", CultureInfo.InvariantCulture);
            return $"{latitude}, {longitude}";
        }
    }
}
