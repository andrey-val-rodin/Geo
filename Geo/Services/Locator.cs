namespace Geo.Services
{
    public static class Locator
    {
        public static async Task<Location> GetCurrentLocationAsync(CancellationToken token)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                Location location = await Geolocation.Default.GetLocationAsync(request, token);

                if (location != null)
                {
                    // TODO
                    double lat = location.Latitude;
                    double lng = location.Longitude;
                    double? altitude = location.Altitude; // может быть null
                                                          // используйте координаты
                    return location;
                }
            }
            catch (FeatureNotSupportedException ex)
            {
                // Геолокация не поддерживается на устройстве или
                // службы геолокации (GPS) выключены в настройках устройства.
            }
            catch (PermissionException ex)
            {
                // Пользователь не выдал разрешение
            }
            catch (Exception ex)
            {
                // Прочие ошибки (например, выключен GPS)
            }

            return null;
        }
    }
}
