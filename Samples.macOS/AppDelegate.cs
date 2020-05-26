using AppKit;
using Foundation;
using Shiny;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;


namespace Samples.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        readonly NSWindow window;
        public override NSWindow MainWindow => this.window;

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 200, 800, 600);
            this.window = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "Shiny"
            };
        }


        public override void DidFinishLaunching(NSNotification notification)
        {
            this.ShinyDidFinishLaunching();
            Forms.Init();
            this.LoadApplication(new App());
            base.DidFinishLaunching(notification);
        }


        public override void RegisteredForRemoteNotifications(NSApplication application, NSData deviceToken)
            => this.ShinyRegisteredForRemoteNotifications(deviceToken);

        public override void FailedToRegisterForRemoteNotifications(NSApplication application, NSError error)
            => this.ShinyFailedToRegisterForRemoteNotifications(error);
    }
}
