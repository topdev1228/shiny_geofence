using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using Humanizer;
using Shiny.Locations;
using Samples.Models;
using Samples.Infrastructure;
using Prism.Navigation;
using XF.Material.Forms.UI.Dialogs;


namespace Samples.Gps
{
    public class LogsViewModel : AbstractLogViewModel<CommandItem>
    {
        readonly SampleSqliteConnection conn;
        readonly IGpsManager manager;


        public LogsViewModel(SampleSqliteConnection conn,
                             IGpsManager manager,
                             IMaterialDialog dialogs) : base(dialogs)
        {
            this.conn = conn;
            this.manager = manager;
        }



        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);
            this.manager
                .WhenReading()
                .Select(x => new CommandItem
                {
                    Text = $"{x.Timestamp}",
                    Detail = $"{x.Position.Latitude} / {x.Position.Longitude}",
                    PrimaryCommand = ReactiveCommand.CreateFromTask(async () =>
                    {
                        var msg = new StringBuilder()
                            .AppendLine("Lat: " + x.Position.Latitude)
                            .AppendLine("Lng: " + x.Position.Longitude)
                            .AppendLine("Alt: " + x.Altitude)
                            .AppendLine("Position Accuracy: " + x.PositionAccuracy)
                            .AppendLine("Heading: " + x.Heading.ToHeading())
                            .AppendLine("Heading Accuracy: " + x.HeadingAccuracy)
                            .AppendLine("Speed (m/s) " + x.Speed)
                            .ToString();

                        await this.Dialogs.AlertAsync(msg);
                    })
                })
                .SubOnMainThread(this.InsertItem)
                .DisposeWith(this.DestroyWith);
        }

        protected override Task ClearLogs() => this.conn.DeleteAllAsync<GpsEvent>();


        protected override async Task<IEnumerable<CommandItem>> LoadLogs()
        {
            var list = await this.conn
                .GpsEvents
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return list.Select(x => new CommandItem
            {
                Text = $"{x.Date.ToLocalTime()}",
                Detail = $"{x.Latitude} / {x.Longitude}",
                PrimaryCommand = ReactiveCommand.CreateFromTask(async () =>
                {
                    var msg = new StringBuilder()
                        .AppendLine("Lat: " + x.Latitude)
                        .AppendLine("Lng: " + x.Longitude)
                        .AppendLine("Alt: " + x.Altitude)
                        .AppendLine("Position Accuracy: " + x.PositionAccuracy)
                        .AppendLine("Heading: " + x.Heading.ToHeading())
                        .AppendLine("Heading Accuracy: " + x.HeadingAccuracy)
                        .AppendLine("Speed (m/s) " + x.Speed)
                        .ToString();

                    await this.Dialogs.AlertAsync(msg);
                })
            });
        }
    }
}
