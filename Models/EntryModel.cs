using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Collections.Generic;
//using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace TRS.Models
{
    public class Entry
    {
        [JsonIgnore]
        public int Index { get; set; } // the index of the Entry within the Json file

        [JsonIgnore]
        public Project Project { get; set; } = null;

        public string ProjectCode {get; set;}

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public int Duration { get; set; }

        public string Description { get; set; }
    }

    public class EntryComparer : Comparer<Entry>
    {
        public override int Compare(Entry x, Entry y)
        {
            if(x.Date.CompareTo(y.Date) != 0)
                return x.Date.CompareTo(y.Date);

            if(x.ProjectCode.CompareTo(y.ProjectCode) != 0)
                return x.ProjectCode.CompareTo(y.ProjectCode);

            if(x.Duration.CompareTo(y.Duration) != 0)
                return x.Duration.CompareTo(y.Duration);

            return x.Description.CompareTo(y.Description); // comparing indexes is useless when sorting entries
        }
    }
}
