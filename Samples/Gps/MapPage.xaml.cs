using System;
using System.Reactive.Linq;
using ReactiveUI;
using Xamarin.Forms;
using Xamarin.Forms.Maps;


namespace Samples.Gps
{
    public partial class MapPage : ContentPage
    {
        IDisposable gpsSub;


        public MapPage()
        {
            this.InitializeComponent();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            this.gpsSub = this.ViewModel
                .WhenAnyValue(x => x.Position)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    var p = new Position(x.Latitude, x.Longitude);
                    myMap.Pins.Clear();
                    myMap.Pins.Add(new Pin
                    {
                        Label = "YOU",
                        Position = p
                    });
                    var mapSpan = MapSpan.FromCenterAndRadius(p, Distance.FromKilometers(3));
                    myMap.MoveToRegion(mapSpan);
                });
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.gpsSub.Dispose();
        }

        public MapViewModel ViewModel => this.BindingContext as MapViewModel;
    }
}