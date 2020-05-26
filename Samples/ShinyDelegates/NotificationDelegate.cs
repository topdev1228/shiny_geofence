using System;
using System.Threading.Tasks;
using Samples.Models;
using Shiny;
using Shiny.Notifications;


namespace Samples.ShinyDelegates
{
    public class NotificationDelegate : INotificationDelegate
    {
        readonly SampleSqliteConnection conn;
        readonly IMessageBus messageBus;
        readonly INotificationManager notifications;


        public NotificationDelegate(SampleSqliteConnection conn, IMessageBus messageBus, INotificationManager notifications)
        {
            this.conn = conn;
            this.messageBus = messageBus;
            this.notifications = notifications;
        }


        public Task OnEntry(NotificationResponse response) => this.Store(new NotificationEvent
        {
            NotificationId = response.Notification.Id,
            NotificationTitle = response.Notification.Title ?? response.Notification.Message,
            Action = response.ActionIdentifier,
            ReplyText = response.Text,
            IsEntry = true,
            Timestamp = DateTime.Now
        });


        public Task OnReceived(Notification notification) => this.Store(new NotificationEvent
        {
            NotificationId = notification.Id,
            NotificationTitle = notification.Title ?? notification.Message,
            IsEntry = false,
            Timestamp = DateTime.Now
        });
        

        async Task Store(NotificationEvent @event)
        {
            await this.conn.InsertAsync(@event);
            this.messageBus.Publish(@event);
        }
    }
}
