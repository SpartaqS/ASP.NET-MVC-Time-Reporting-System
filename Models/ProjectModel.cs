using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TRS.Models
{
    public class Project
    {
        /// <summary> replace all data in this object with data from <paramref name="newState"/> </summary>
        /// <param name="newState"> target state of the project </param>
        public void Overwrite(Project newState)
        {
            ProjectName = newState.ProjectName;
            ProjectCode = newState.ProjectCode;
            Budget = newState.Budget;
            Active = newState.Active;
        }
        public string ProjectName { get; set; }
        [Display(Name = "Code")]
        public string ProjectCode { get; set; }
        [Display(Name = "Budget")]
        public int Budget { get; set; }
        [Display(Name = "Active")]
        public bool Active { get; set; }
    }
}
