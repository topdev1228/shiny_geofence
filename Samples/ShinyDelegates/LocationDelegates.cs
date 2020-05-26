using System;
using System.Threading.Tasks;
using Samples.Models;
using Shiny.Locations;


namespace Samples.ShinyDelegates
{
    public class LocationDelegates : IGeofenceDelegate, IGpsDelegate
    {
        readonly CoreDelegateServices services;
        public LocationDelegates(CoreDelegateServices services) => this.services = services;


        public Task OnReading(IGpsReading reading)
            => this.services.Connection.InsertAsync(new GpsEvent
            {
                Latitude = reading.Position.Latitude,
                Longitude = reading.Position.Longitude,
                Altitude = reading.Altitude,
                PositionAccuracy = reading.PositionAccuracy,
                Heading = reading.Heading,
                HeadingAccuracy = reading.HeadingAccuracy,
                Speed = reading.Speed,
                Date = reading.Timestamp.ToLocalTime()
            });


        public async Task OnStatusChanged(GeofenceState newStatus, GeofenceRegion region)
        {
            await this.services.Connection.InsertAsync(new GeofenceEvent
            {
                Identifier = region.Identifier,
                Entered = newStatus == GeofenceState.Entered,
                Date = DateTime.Now
            });
            var notify = newStatus == GeofenceState.Entered
                ? this.services.AppSettings.UseNotificationsGeofenceEntry
                : this.services.AppSettings.UseNotificationsGeofenceExit;

            await this.services.SendNotification(
                "Geofence Event",
                $"{region.Identifier} was {newStatus}",
                x => x.UseNotificationsBle
            );
        }
    }
}
