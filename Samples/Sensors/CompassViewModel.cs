using System;
using System.Reactive.Disposables;
using ReactiveUI.Fody.Helpers;
using Shiny.Sensors;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.Sensors
{
    public class CompassViewModel : ViewModel
    {
        readonly ICompass compass;
        readonly IMaterialDialog dialogs;


        public CompassViewModel(IMaterialDialog dialogs, ICompass compass)
        {
            this.compass = compass;
            this.dialogs = dialogs;
        }


        [Reactive] public double Rotation { get; private set; }
        [Reactive] public double Heading { get; private set; }


        public override async void OnAppearing()
        {
            base.OnAppearing();
            if (!this.compass.IsAvailable)
            {
                await this.dialogs.AlertAsync("Compass is not available");
                return;
            }
            this.compass
                .WhenReadingTaken()
                .Subscribe(x =>
                {
                    this.Rotation = 360 - x.MagneticHeading;
                    this.Heading = x.MagneticHeading;
                })
                .DisposeWith(this.DeactivateWith);
        }
    }
}
