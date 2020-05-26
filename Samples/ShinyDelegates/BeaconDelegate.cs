using System;
using System.Threading.Tasks;
using Samples.Models;
using Shiny.Beacons;


namespace Samples.ShinyDelegates
{
    public class BeaconDelegate : IBeaconDelegate
    {
        readonly CoreDelegateServices services;
        public BeaconDelegate(CoreDelegateServices services) => this.services = services;


        public async Task OnStatusChanged(BeaconRegionState newStatus, BeaconRegion region)
        {
            await this.services.Connection.InsertAsync(new BeaconEvent
            {
                Identifier = region.Identifier,
                Uuid = region.Uuid.ToString(),
                Major = region.Major,
                Minor = region.Minor,
                Entered = newStatus == BeaconRegionState.Entered,
                Date = DateTime.Now
            });
            var notify = newStatus == BeaconRegionState.Entered
                ? this.services.AppSettings.UseNotificationsBeaconRegionEntry
                : this.services.AppSettings.UseNotificationsBeaconRegionExit;

            if (notify)
            {
                await this.services.SendNotification(
                    $"Beacon Region {newStatus}",
                    $"{region.Identifier} - {region.Uuid}/{region.Major}/{region.Minor}"
                );
            }
        }
    }
}
