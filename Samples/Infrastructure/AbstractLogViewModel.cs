using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Shiny;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.Infrastructure
{
    public abstract class AbstractLogViewModel<TItem> : ViewModel
    {
        readonly object syncLock = new object();


        protected AbstractLogViewModel(IMaterialDialog dialogs)
        {
            this.Dialogs = dialogs;

            this.Logs = new ObservableList<TItem>();
            this.Logs
                .WhenCollectionChanged()
                .Synchronize(this.syncLock)
                .Select(_ => this.Logs.Any())
                .ToPropertyEx(this, x => x.HasLogs);

            this.Load = ReactiveCommand.CreateFromTask(async () =>
            {
                var logs = await this.LoadLogs();
                this.Logs.ReplaceAll(logs);
            });
            this.Clear = ReactiveCommand.CreateFromTask(this.DoClear);
            this.BindBusyCommand(this.Load);
        }


        protected IMaterialDialog Dialogs { get; }
        public ObservableList<TItem> Logs { get; }
        public ICommand Load { get; }
        public ICommand Clear { get; }
        public bool HasLogs { [ObservableAsProperty] get; }


        public override void OnAppearing()
        {
            base.OnAppearing();
            this.Load.Execute(null);
        }


        protected abstract Task<IEnumerable<TItem>> LoadLogs();
        protected abstract Task ClearLogs();
        protected virtual void InsertItem(TItem item)
        {
            lock (this.syncLock)
                this.Logs.Insert(0, item);
        }


        protected virtual async Task DoClear()
        {
            var confirm = await this.Dialogs.ConfirmAsync("Clear Logs?") ?? false;
            if (confirm)
            {
                await this.ClearLogs();
                this.Load.Execute(null);
            }
        }
    }
}
