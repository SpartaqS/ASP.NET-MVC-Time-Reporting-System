using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TRS.Models
{
    /// <summary> a model for a monthly report for a user's activity </summary>
    public class MonthlyReport
    {
        public MonthlyReport(DateTime month, List<Project> projects)
        {
            Month = month;
            Projects = projects;
        }
        
        [DataType(DataType.Date)]
        public DateTime Month { get; set; }
        public List<Project> Projects { get; set; }
    }
}
