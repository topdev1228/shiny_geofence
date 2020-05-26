using System;
using System.Windows.Input;
using System.Reactive.Linq;
using Shiny.Beacons;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;


namespace Samples.Beacons
{
    public class CreateViewModel : ViewModel
    {
        public CreateViewModel(INavigationService navigationService)
        {
            this.Title = "Create Region";

            this.EstimoteDefaults = ReactiveCommand.Create(() =>
            {
                this.Identifier = "Estimote";
                this.Uuid = "B9407F30-F5F8-466E-AFF9-25556B57FE6D";
            });

            this.WhenAnyValue(x => x.Major)
                .Select(x => !x.IsEmpty() && UInt16.TryParse(x, out _))
                .ToPropertyEx(this, x => x.IsMajorSet);

            this.Create = ReactiveCommand.CreateFromTask(
                () => navigationService.GoBack(false, (
                    nameof(BeaconRegion),
                    new BeaconRegion(
                        this.Identifier,
                        Guid.Parse(this.Uuid),
                        GetNumberAddress(this.Major),
                        GetNumberAddress(this.Minor)
                    )
                    {
                        NotifyOnEntry = this.NotifyOnEntry,
                        NotifyOnExit = this.NotifyOnExit
                    }
                )),
                this.WhenAny(
                    x => x.Identifier,
                    x => x.Uuid,
                    x => x.Major,
                    x => x.Minor,
                    x => x.NotifyOnEntry,
                    x => x.NotifyOnExit,
                    (idValue, uuidValue, majorValue, minorValue, entry, exit) =>
                    {
                        if (this.ForMonitoring)
                        {
                            var atLeast1Notification = entry.GetValue() || exit.GetValue();
                            if (!atLeast1Notification)
                                return false;
                        }
                        if (String.IsNullOrWhiteSpace(idValue.GetValue()))
                            return false;

                        var uuid = uuidValue.GetValue();
                        if (!uuid.IsEmpty() && !Guid.TryParse(uuid, out _))
                            return false;

                        var result = ValidateNumberAddress(this.Major);
                        if (!result)
                            return false;

                        result = ValidateNumberAddress(this.Minor);
                        if (!result)
                            return false;

                        return true;
                    }
                )
            );
        }


        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            this.ForMonitoring = parameters.GetValue<bool>("Monitoring");
            this.Title = this.ForMonitoring
                ? "Create Monitoring Region"
                : "Create Ranging Region";

            var current = parameters.GetValue<BeaconRegion>(nameof(BeaconRegion));
            if (current != null)
            {
                this.Identifier = current.Identifier;
                this.Uuid = current.Uuid.ToString();
                this.Major = current.Major?.ToString() ?? String.Empty;
                this.Minor = current.Minor?.ToString() ?? String.Empty;
            }
        }


        public bool IsMajorSet { [ObservableAsProperty] get; }

        public ICommand Create { get; }
        public ICommand EstimoteDefaults { get; }
        [Reactive] public string Identifier { get; set; }
        [Reactive] public string Uuid { get; set; }
        [Reactive] public string Major { get; set; }
        [Reactive] public string Minor { get; set; }
        [Reactive] public bool ForMonitoring { get; private set; }
        [Reactive] public bool NotifyOnEntry { get; set; } = true;
        [Reactive] public bool NotifyOnExit { get; set; } = true;


        static bool ValidateNumberAddress(string value)
        {
            if (value.IsEmpty())
                return true;

            return UInt16.TryParse(value, out _);
        }


        ushort? GetNumberAddress(string value)
        {
            if (value.IsEmpty())
                return null;

            return UInt16.Parse(value);
        }
    }
}
