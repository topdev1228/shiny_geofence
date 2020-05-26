using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny.Beacons;
using Shiny.Logging;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.Beacons
{
    public class MonitoringViewModel : ViewModel
    {
        readonly IBeaconManager beaconManager;
        readonly IMaterialDialog dialogs;


        public MonitoringViewModel(INavigationService navigator,
                                   IMaterialDialog dialogs,
                                   IBeaconManager beaconManager)
        {
            this.dialogs = dialogs;
            this.beaconManager = beaconManager;

            this.Add = navigator.NavigateCommand(
                "CreateBeacon",
                p => p.Add("Monitoring", true)
            );

            this.Load = ReactiveCommand.CreateFromTask(async () =>
            {
                var regions = await this.beaconManager.GetMonitoredRegions();

                this.Regions = regions
                    .Select(x => new CommandItem
                    {
                        Text = $"{x.Identifier}",
                        Detail = $"{x.Uuid}/{x.Major ?? 0}/{x.Minor ?? 0}",
                        PrimaryCommand = ReactiveCommand.CreateFromTask(async () =>
                        {
                            await this.beaconManager.StopMonitoring(x.Identifier);
                            this.Load.Execute(null);
                        })
                    })
                    .ToList();
            });

            this.StopAllMonitoring = ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await dialogs.ConfirmAsync("Are you sure you wish to stop all monitoring") ?? false;
                if (result)
                {
                    await this.beaconManager.StopAllMonitoring();
                    this.Load.Execute(null);
                }
            });
        }


        public ICommand Load { get; }
        public ICommand Add { get; }
        public ICommand StopAllMonitoring { get; }
        [Reactive] public IList<CommandItem> Regions { get; private set; }


        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);
            this.Load.Execute(null);
        }


        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            try
            {
                var newRegion = parameters.GetValue<BeaconRegion>(nameof(BeaconRegion));
                if (newRegion != null)
                {
                    await this.beaconManager.StartMonitoring(newRegion);
                    this.Load.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                await this.dialogs.AlertAsync(ex.ToString());
            }
        }
    }
}