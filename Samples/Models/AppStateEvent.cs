﻿using System;
using SQLite;

namespace Samples.Models
{
    public class AppStateEvent
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public string Event { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

