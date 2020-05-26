using System;
using Shiny;
using Android.App;
using Android.Runtime;
using Samples.ShinySetup;


namespace Samples.Droid
{
    [Application]
    public class MainApplication : ShinyAndroidApplication<SampleStartup>
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }
    }
}