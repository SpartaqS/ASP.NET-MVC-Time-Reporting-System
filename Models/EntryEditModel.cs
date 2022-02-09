using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TRS.Models
{
    public class EntryEdit
    {
        public EntryEdit(List<Project> availableProjects, Entry entry)
        {
            Projects = availableProjects;
            Entry = entry;
        }
        public List<Project> Projects { get; set; }
        public Entry Entry { get; set;}
    }
}
