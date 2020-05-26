using System;
using Samples.ShinySetup;
using Shiny;
using Xamarin.Forms.Platform.Tizen;


namespace Samples.Tizen
{
    class Program : FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            this.LoadApplication(new App());
        }


        static void Main(string[] args)
        {
            var app = new Program();
            TizenShinyHost.Init(new SampleStartup());
            Xamarin.Forms.Forms.Init(app);
            app.Run(args);
        }
    }
}
