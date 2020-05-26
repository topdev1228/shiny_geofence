using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;
using Shiny.Locations;



namespace Samples.Gps
{
    //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/map
    public class MapViewModel : ViewModel
    {
        readonly IGpsManager gpsManager;


        public MapViewModel(IGpsManager gpsManager)
        {
            this.gpsManager = gpsManager;
            this.gpsManager
                .WhenReading()
                .SubOnMainThread(x => this.Position = x.Position)
                .DisposeWith(this.DestroyWith);

            this.Toggle = ReactiveCommand.CreateFromTask(async () =>
            {
                if (this.gpsManager.IsListening)
                {
                    await this.gpsManager.StopListener();
                }
                else
                {
                    var access = await this.gpsManager.RequestAccess(new GpsRequest());
                    if (access == AccessState.Available)
                        await this.gpsManager.StartListener(null);
                }
                this.SetText();
            });
        }


        public ICommand Toggle { get; }
        [Reactive] public string ListenerText { get; private set; } = "Start GPS";
        [Reactive] public Position Position { get; private set; }


        public override void OnAppearing()
        {
            base.OnAppearing();
            this.SetText();
        }


        void SetText() =>
            this.ListenerText = this.gpsManager.IsListening
                ? "Stop GPS"
                : "Start GPS";
    }
}
