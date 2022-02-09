using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TRS.Models
{
    public class DailyScreen
    {
        public DailyScreen(DateTime date, List<Entry> entries)
        {
            Date = date;
            Entries = entries;
        }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public List<Entry> Entries { get; set; }
    }
}
