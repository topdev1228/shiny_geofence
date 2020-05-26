﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Samples.Infrastructure;
using Samples.Models;
using XF.Material.Forms.UI.Dialogs;

namespace Samples.Beacons
{
    public class LogsViewModel : AbstractLogViewModel<BeaconEvent>
    {
        readonly SampleSqliteConnection conn;


        public LogsViewModel(IMaterialDialog dialogs, SampleSqliteConnection conn) : base(dialogs)
        {
            this.conn = conn;
        }


        protected override Task ClearLogs() => this.conn.DeleteAllAsync<BeaconEvent>();
        protected override async Task<IEnumerable<BeaconEvent>> LoadLogs()
            => await this.conn
                .BeaconEvents
                .OrderByDescending(x => x.Date)
                .ToListAsync();
    }
}
