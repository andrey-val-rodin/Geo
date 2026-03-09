namespace Geo.Model
{
    public class LocationAddedEventArgs(GeoPosition newLocation)
    {
        public GeoPosition Location { get; private set; } = newLocation;
    }
}
