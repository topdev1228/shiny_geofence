﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Samples.Infrastructure;
using Shiny.Logging;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.Logging
{
    public class EventsViewModel : AbstractLogViewModel<CommandItem>
    {
        public EventsViewModel(IMaterialDialog dialogs) : base(dialogs)
        {
            Log
                .WhenEventLogged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => new CommandItem
                {
                    Text = $"{x.EventName} ({DateTime.Now:hh:mm:ss tt})",
                    Detail = x.Description,
                    PrimaryCommand = ReactiveCommand.CreateFromTask(async () =>
                    {
                        var s = $"{x.EventName} ({DateTime.Now:hh:mm:ss tt}){Environment.NewLine}{x.Description}";
                        foreach (var p in x.Parameters)
                            s += $"{Environment.NewLine}{p.Key}: {p.Value}";

                        await this.Dialogs.AlertAsync(s);
                    })
                })
                .Subscribe(this.InsertItem)
                .DisposeWith(this.DestroyWith);
        }


        protected override Task ClearLogs() => Task.CompletedTask;
        protected override Task<IEnumerable<CommandItem>> LoadLogs()
            => Task.FromResult(Enumerable.Empty<CommandItem>());
    }
}
