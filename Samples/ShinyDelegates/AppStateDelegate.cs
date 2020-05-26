using System;
using Samples.Models;
using Shiny;
using Shiny.Notifications;

namespace Samples.ShinyDelegates
{
    public class AppStateDelegate : IAppStateDelegate
    {
        readonly SampleSqliteConnection conn;
        readonly INotificationManager notificationManager;


        public AppStateDelegate(SampleSqliteConnection conn, INotificationManager notificationManager)
        {
            this.conn = conn;
            this.notificationManager = notificationManager;
        }


        public void OnStart() => this.Store("Start");        
        public void OnForeground()
        {
            this.notificationManager.Badge = 0;
            this.Store("Foreground");
        }
        public void OnBackground() => this.Store("Background");



        void Store(string eventName) => this.conn.GetConnection().Insert(new AppStateEvent
        {
            Event = eventName,
            Timestamp = DateTime.UtcNow
        });
    }
}

