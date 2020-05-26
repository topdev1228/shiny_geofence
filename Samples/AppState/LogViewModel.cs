using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Samples.Infrastructure;
using Samples.Models;
using XF.Material.Forms.UI.Dialogs;

namespace Samples.AppState
{
    public class LogViewModel : AbstractLogViewModel<CommandItem>
    {
        readonly SampleSqliteConnection conn;


        public LogViewModel(SampleSqliteConnection conn, IMaterialDialog dialogs) : base(dialogs)
        {
            this.conn = conn;
        }


        protected override Task ClearLogs() => this.conn.DeleteAllAsync<AppStateEvent>();
        protected override async Task<IEnumerable<CommandItem>> LoadLogs()
        {
            var events = await this.conn
                .AppStateEvents
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();

            return events.Select(x => new CommandItem
            {
                Text = x.Event,
                Detail = x.Timestamp.ToLocalTime().ToString()
            });
        }
    }
}

