using System;
using System.Reactive.Linq;
using Shiny;
using Shiny.Net;
using Shiny.Power;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Samples
{
    public class EnvironmentViewModel : ViewModel
    {
        readonly IEnvironment environment;


        public EnvironmentViewModel(IEnvironment environment,
                                    IConnectivity connectivity,
                                    IPowerManager powerManager)
        {
            this.environment = environment;

            connectivity
                .WhenAnyValue(x => x.Reach)
                .Select(x => x.ToString())
                .ToPropertyEx(this, x => x.NetworkReach)
                .DisposedBy(this.DeactivateWith);

            connectivity
                .WhenAnyValue(x => x.Access)
                .Select(x => x.ToString())
                .ToPropertyEx(this, x => x.NetworkAccess)
                .DisposedBy(this.DeactivateWith);

            connectivity
                .WhenAnyValue(x => x.CellularCarrier)
                .ToPropertyEx(this, x => x.CellularCarrier)
                .DisposedBy(this.DeactivateWith);

            powerManager
                .WhenAnyValue(x => x.IsEnergySavingEnabled)
                .ToPropertyEx(this, x => x.IsEnergySavingEnabled)
                .DisposedBy(this.DeactivateWith);

            powerManager
                .WhenAnyValue(x => x.Status)
                .Select(x => x.ToString())
                .ToPropertyEx(this, x => x.PowerStatus)
                .DisposedBy(this.DeactivateWith);

            powerManager
                .WhenAnyValue(x => x.BatteryLevel)
                .ToPropertyEx(this, x => x.BatteryPercentage)
                .DisposedBy(this.DeactivateWith);
        }


        public string AppIdentifier => this.environment.AppIdentifier;
        public string AppVersion => this.environment.AppVersion;
        public string AppBuild => this.environment.AppBuild;
        public string Name => this.environment.MachineName;
        public string OS => this.environment.OperatingSystem;
        public string OSVersion => this.environment.OperatingSystemVersion;
        public string Device => $"{this.environment.Manufacturer} {this.environment.Model}";

        public bool IsEnergySavingEnabled { [ObservableAsProperty] get; }
        public string NetworkReach { [ObservableAsProperty] get; }
        public string NetworkAccess { [ObservableAsProperty] get; }
        public string PowerStatus { [ObservableAsProperty] get; }
        public int BatteryPercentage { [ObservableAsProperty] get; }
        public string CellularCarrier { [ObservableAsProperty] get; }
    }
}
