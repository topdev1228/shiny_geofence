using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny.Notifications;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.Notifications
{
    public class PendingViewModel : ViewModel
    {
        public PendingViewModel(INotificationManager notifications,
                                IMaterialDialog dialogs)
        {
            this.Load = ReactiveCommand.CreateFromTask(async () =>
            {
                var pending = await notifications.GetPending();
                this.PendingList = pending
                    .Select(x => new CommandItem
                    {
                        Text = $"[{x.Id}] {x.Title}",
                        Detail = $"[{x.ScheduleDate.Value}] {x.Message}",
                        PrimaryCommand = ReactiveCommand.CreateFromTask(async () =>
                        {
                            await notifications.Cancel(x.Id);
                            ((ICommand)this.Load).Execute(null);
                        })
                    })
                    .ToList();
            });
            this.BindBusyCommand(this.Load);

            this.Clear = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    var confirm = await dialogs.ConfirmAsync("Clear All Pending Notifications?", "Confirm", "Yes", "No") ?? false;
                    if (confirm)
                    {
                        await notifications.Clear();
                        ((ICommand)this.Load).Execute(null);
                    }
                }
                //this.WhenAny(
                //    x => x.PendingList,
                //    x => x.GetValue()?.Any() ?? false
                //)
            );
        }


        public IReactiveCommand Load { get; }
        public IReactiveCommand Clear { get; }
        [Reactive] public IList<CommandItem> PendingList { get; private set; }


        public override void OnAppearing()
        {
            base.OnAppearing();
            ((ICommand)this.Load).Execute(null);
        }
    }
}
