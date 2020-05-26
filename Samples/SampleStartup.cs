//#define STARTUP_ATTRIBUTES
//#define STARTUP_AUTO

using System;
using Shiny;
using Shiny.Logging;
using Microsoft.Extensions.DependencyInjection;
using Samples;
using Samples.Settings;
using Samples.ShinyDelegates;
using Samples.ShinySetup;
using Shiny.Infrastructure;
using Shiny.Notifications;
using XF.Material.Forms.UI.Dialogs;

#if STARTUP_ATTRIBUTES
//[assembly: ShinySqliteIntegration(true, true, true, true, true)]
//[assembly: ShinyJob(typeof(SampleJob), "MyIdentifier", BatteryNotLow = true, DeviceCharging = false, RequiredInternetAccess = Shiny.Jobs.InternetAccess.Any)]
[assembly: ShinyAppCenterIntegration(Constants.AppCenterTokens, true, true)]
[assembly: ShinyService(typeof(SampleSqliteConnection))]
[assembly: ShinyService(typeof(GlobalExceptionHandler))]
[assembly: ShinyService(typeof(CoreDelegateServices))]
[assembly: ShinyService(typeof(JobLoggerTask))]
[assembly: ShinyService(typeof(IAppSettings), typeof(AppSettings))]

#if !STARTUP_AUTO
[assembly: ShinyNotifications(typeof(NotificationDelegate), true)]
[assembly: ShinyBeacons(typeof(BeaconDelegate))]
[assembly: ShinyBleCentral(typeof(BleCentralDelegate))]
[assembly: ShinyGps(typeof(LocationDelegates))]
[assembly: ShinyGeofences(typeof(LocationDelegates))]
[assembly: ShinyMotionActivity]
[assembly: ShinySensors]
[assembly: ShinyHttpTransfers(typeof(HttpTransferDelegate))]
[assembly: ShinySpeechRecognition]
//[assembly: ShinyPush(typeof(PushDelegate))]
[assembly: ShinyAzureNotificationHub(typeof(PushDelegate), Constants.AnhListenerConnectionString, Constants.AnhHubName)]
[assembly: ShinyNfc]
#endif
#endif

namespace Samples.ShinySetup
{
    public class SampleStartup : ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            Log.UseConsole();
            Log.UseDebug();
            services.UseMemoryCache();
            //services.UseAppCenterLogging(Constants.AppCenterTokens, true, false);
            services.UseSqliteLogging(true, true);
            //services.UseSqliteCache();
            //services.UseSqliteSettings();
            //services.UseSqliteStorage();
            services.AddSingleton<IMaterialDialog>(MaterialDialog.Instance);

#if STARTUP_ATTRIBUTES
            services.RegisterModule(new AssemblyServiceModule());
#if STARTUP_AUTO
            services.RegisterModule(new AutoRegisterModule());
#endif
#else
            UseAllServices(services);
#endif
        }


        static void UseAllServices(IServiceCollection services)
        {
            // your infrastructure
            services.AddSingleton<SampleSqliteConnection>();
            services.AddSingleton<CoreDelegateServices>();
            services.AddSingleton<IAppSettings, AppSettings>();

            // startup tasks
            services.AddSingleton<GlobalExceptionHandler>();
            services.AddSingleton<JobLoggerTask>();
            services.AddAppState<AppStateDelegate>();

            // register all of the shiny stuff you want to use
            services.UseJobForegroundService(TimeSpan.FromSeconds(30));
            services.UseHttpTransfers<HttpTransferDelegate>();
            services.UseBeacons<BeaconDelegate>();
            services.UseBleCentral<BleCentralDelegate>();
            services.UseBlePeripherals();
            services.UseMotionActivity();
            services.UseSpeechRecognition();
            services.UseAllSensors();
            services.UseNfc();

            services.UseGeofencing<LocationDelegates>();
            services.UseGeofencingSync<LocationSyncDelegates>();
            //services.UseGpsDirectGeofencing<LocationDelegates>();
            services.UseGps<LocationDelegates>();
            services.UseGpsSync<LocationSyncDelegates>();

            //services.UseNotifications(true);
            services.UseNotifications<NotificationDelegate>(
                true,
                new NotificationCategory(
                    "Test",
                    new NotificationAction("Reply", "Reply", NotificationActionType.TextReply),
                    new NotificationAction("Yes", "Yes", NotificationActionType.None),
                    new NotificationAction("No", "No", NotificationActionType.Destructive)
                )
            );

            //services.UsePushNotifications<PushDelegate>();
            //services.UseFirebaseMessaging<PushDelegate>();
            services.UsePushAzureNotificationHubs<PushDelegate>(
                Constants.AnhListenerConnectionString,
                Constants.AnhHubName
            );
        }
    }
}
