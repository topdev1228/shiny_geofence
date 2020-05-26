using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;
using Shiny.Locations;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.Gps
{
    public class GpsViewModel : ViewModel
    {
        readonly IGpsManager manager;
        IDisposable gpsListener;


        public GpsViewModel(IGpsManager manager, IMaterialDialog dialogs)
        {
            this.manager = manager;
            this.IsUpdating = this.manager.IsListening;

            this.WhenAnyValue(x => x.UseBackground)
                .Subscribe(x => this.Access = this.manager.GetCurrentStatus(
                    new GpsRequest { UseBackground = this.UseBackground }).ToString()
                );

            this.WhenAnyValue(x => x.IsUpdating)
                .Select(x => x ? "Stop Listening" : "Start Updating")
                .ToPropertyEx(this, x => x.ListenerText);

            this.GetCurrentPosition = ReactiveCommand.CreateFromTask(async _ =>
            {
                var result = await dialogs.RequestAccess(() => this.manager.RequestAccess(new GpsRequest()));
                if (!result)
                    return;

                var reading = await this.manager.GetLastReading();
                if (reading == null)
                    await dialogs.AlertAsync("Could not getting GPS coordinates");
                else
                    this.SetValues(reading);
            });
            this.BindBusyCommand(this.GetCurrentPosition);

            ReactiveCommand.Create(() => dialogs.ActionSheet(
                false,
                ("Highest", () => this.Priority = GpsPriority.Highest),
                ("Normal", () => this.Priority = GpsPriority.Normal),
                ("Low", () => this.Priority = GpsPriority.Low)
            ));

            this.ToggleUpdates = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    if (this.manager.IsListening)
                    {
                        await this.manager.StopListener();
                        this.gpsListener?.Dispose();
                    }
                    else
                    {
                        var result = await dialogs.RequestAccess(() => this.manager.RequestAccess(new GpsRequest { UseBackground = this.UseBackground }));
                        if (!result)
                        {
                            await dialogs.AlertAsync("Insufficient permissions");
                            return;
                        }

                        var request = new GpsRequest
                        {
                            UseBackground = this.UseBackground,
                            Priority = this.Priority,
                        };
                        if (IsInterval(this.DesiredInterval))
                            request.Interval = ToInterval(this.DesiredInterval);

                        if (IsInterval(this.ThrottledInterval))
                            request.ThrottledInterval = ToInterval(this.ThrottledInterval);

                        await this.manager.StartListener(request);
                    }
                    this.IsUpdating = this.manager.IsListening;
                },
                this.WhenAny(
                    x => x.IsUpdating,
                    x => x.DesiredInterval,
                    x => x.ThrottledInterval,
                    (u, i, t) =>
                    {
                        if (u.GetValue())
                            return true;

                        var isdesired = IsInterval(i.GetValue());
                        var isthrottled = IsInterval(t.GetValue());

                        if (isdesired && isthrottled)
                        {
                            var desired = ToInterval(i.GetValue());
                            var throttle = ToInterval(t.GetValue());
                            if (throttle.TotalSeconds >= desired.TotalSeconds)
                                return false;
                        }
                        return true;
                    }
                )
            );

            this.UseRealtime = ReactiveCommand.Create(() =>
            {
                var rt = GpsRequest.Realtime(false);
                this.ThrottledInterval = String.Empty;
                this.DesiredInterval = rt.Interval.TotalSeconds.ToString();
                this.Priority = rt.Priority;
            });

            this.RequestAccess = ReactiveCommand.CreateFromTask(async () =>
            {
                var access = await this.manager.RequestAccess(new GpsRequest { UseBackground = this.UseBackground });
                this.Access = access.ToString();
            });
            this.BindBusyCommand(this.RequestAccess);
        }


        public override void OnAppearing()
        {
            base.OnAppearing();

            this.gpsListener = this.manager
                .WhenReading()
                .SubOnMainThread(this.SetValues)
                .DisposeWith(this.DeactivateWith);
        }


        void SetValues(IGpsReading reading)
        {
            using (this.DelayChangeNotifications())
            {
                this.Latitude = reading.Position.Latitude;
                this.Longitude = reading.Position.Longitude;
                this.Altitude = reading.Altitude;
                this.PositionAccuracy = reading.PositionAccuracy;

                this.Heading = reading.Heading;
                this.HeadingAccuracy = reading.HeadingAccuracy;
                this.Speed = reading.Speed;
            }
        }


        public IReactiveCommand UseRealtime { get; }
        public IReactiveCommand SelectPriority { get; }
        public IReactiveCommand GetCurrentPosition { get; }
        public IReactiveCommand ToggleUpdates { get; }
        public IReactiveCommand RequestAccess { get; }


        public string ListenerText { [ObservableAsProperty] get; }

        [Reactive] public bool UseBackground { get; set; } = true;
        [Reactive] public GpsPriority Priority { get; set; } = GpsPriority.Normal;
        [Reactive] public string DesiredInterval { get; set; }
        [Reactive] public string ThrottledInterval { get; set; }
        [Reactive] public string Access { get; private set; }
        [Reactive] public bool IsUpdating { get; private set; }
        [Reactive] public double Latitude { get; private set; }
        [Reactive] public double Longitude { get; private set; }
        [Reactive] public double Altitude { get; private set; }
        [Reactive] public double PositionAccuracy { get; private set; }
        [Reactive] public double Heading { get; private set; }
        [Reactive] public double HeadingAccuracy { get; private set; }
        [Reactive] public double Speed { get; private set; }


        static bool IsInterval(string value)
        {
            if (value.IsEmpty())
                return false;

            if (Int32.TryParse(value, out var r))
                return r > 0;

            return false;
        }


        static TimeSpan ToInterval(string value)
        {
            var i = Int32.Parse(value);
            var ts = TimeSpan.FromSeconds(i);
            return ts;
        }
    }
}
