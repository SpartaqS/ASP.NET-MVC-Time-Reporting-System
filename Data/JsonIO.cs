using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TRS.Models;

namespace TRS.Data
{
    public static class JsonIO
    {
        #region  Projects
        private static string _projectsFileName = "projects.json";
        /// <summary>
        ///  Returns whether a project with matching <paramref name="projectCode"/> has been found. If yes, access the project via  
        /// </summary>
        /// <param name="projectCode"> code of the desired project </param>
        /// <param name="foundProject"> found project object</param>
        /// <returns>Project with matching <paramref name="projectCode"/> or null if no matching project was found</returns>
        public static bool GetProject(out Project foundProject, string projectCode)
        {
            List<Project> projects = GetProjects();

            foundProject = null;
            foundProject = projects.Find(project => project.ProjectCode == projectCode);
            return foundProject != null;
        }

        /// <summary>
        ///  Returns all projects
        /// </summary>
        /// <returns>All projects aggregated in a list</returns>
        public static List<Project> GetProjects()
        {
            List<Project> projects = new List<Project>();

            string jsonString = File.ReadAllText(_projectsFileName);
            projects = JsonSerializer.Deserialize<List<Project>>(jsonString);

            return projects;
        }

        /// <summary>
        ///  Saves all projects from <paramref name="allProjects"/> into the database, discards any not present within <paramref name="allProjects"/>
        /// </summary>
        /// <param name="allProjects"> list of all projects (any not within this list will be lost) </param>
        private static void SaveToFile(string fileName, IEnumerable<Object> allProjects)
        {
            string jsonString = JsonSerializer.Serialize(allProjects);
            File.WriteAllText(fileName, jsonString);
        }

        /// <summary>
        ///  Updates the <paramref name="project"/> in the database
        /// </summary>
        /// <param name="project"> project to update </param>
        public static void UpdateProject(Project projectToUpdate)
        {
            List<Project> projects = GetProjects();

            foreach (Project project in projects)
            {
                if (project.ProjectCode == projectToUpdate.ProjectCode)
                {
                    project.Overwrite(projectToUpdate);
                    break; //project codes are unique
                }
            }
            SaveToFile(_projectsFileName, projects);
        }


        /// <summary>
        ///  Call only in startup, sets up the projects (if they do not exist, creates them, otherwise does nothing)
        /// </summary>
        public static void SetupHardcodedProjects()
        {
            if (File.Exists(_projectsFileName))
                return; // projects already exist, we do not want to overwrite the data (mainly: reset the budget)

            List<Project> projects = new List<Project>();
            var defaultProject = new Project
            {
                ProjectName = "The First Project",
                ProjectCode = "Project1",
                Budget = 12345,
                Active = true,
            };

            projects.Add(defaultProject);
            defaultProject = new Project
            {
                ProjectName = "Colorful Tango",
                ProjectCode = "ProjectTango",
                Budget = 4545,
                Active = true,
            };
            projects.Add(defaultProject);

            defaultProject = new Project
            {
                ProjectName = "Simple App",
                ProjectCode = "app0",
                Budget = 222,
                Active = true,
            };
            projects.Add(defaultProject);

            SaveToFile(_projectsFileName, projects);
        }
        #endregion

        #region  Entries
        private static string _entriesFileType = ".json";
        /// <summary>
        ///  Obtain all entries within given <paramref name="month"/>.
        /// </summary>
        /// <remarks> Entries within <paramref name="loadedEntries"/> do not have their "Project" field initialized
        /// Use ObtainProjectCopies() to initialize them  </remarks>
        /// <param name="month"> project to update </param>
        /// <param name="loadedEntries"> list of entries loaded from file </param>
        /// <returns>Whether a valid entry list has been loaded or not</returns>
        public static bool GetEntriesForMonth(out List<Entry> loadedEntries, DateTime month, string username)
        {
            loadedEntries = new List<Entry>();
            string fileName = ConstructEntryFileName(month, username);
            string jsonString = string.Empty;
            try
            {
                if(!File.Exists(fileName))
                {// file does not exist: quit
                    return false;
                }
                jsonString = File.ReadAllText(fileName);
            }
            catch // any exception means that we cannot read the jsonString as a list of entries
            {
                return false;
            }
            loadedEntries = JsonSerializer.Deserialize<List<Entry>>(jsonString);
            foreach(var entry in loadedEntries)
            {// read indices from the database
                entry.Index = loadedEntries.IndexOf(entry);
            }
            return true;
        }
        /// <summary>
        ///  Obtain all entries within given <paramref name="day"/>.
        /// </summary>
        /// <remarks> Entries within <paramref name="loadedEntries"/> have their "Project" field initialized </remarks>
        /// <param name="day"> specific day of a year</param>
        /// <param name="loadedEntries"> list of entries loaded from file </param>
        /// <returns>Whether a valid entry list has been loaded or not</returns>
        public static bool GetFullEntriesForDay(out List<Entry> loadedEntries, DateTime day, string username)
        {
            day = new DateTime(day.Year, day.Month, day.Day); // consider only the year, month and day of the provided date (hours/minutes do not matter)
            loadedEntries = new List<Entry>();
            if (GetEntriesForMonth(out loadedEntries, day, username))
            {
                loadedEntries.RemoveAll(entry => entry.Date != day);
                ObtainProjectCopies(ref loadedEntries);
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Obtain entry of index <paramref name="index"/> within given <paramref name="month"/>.
        /// </summary>
        /// <remarks> Entries within <paramref name="loadedEntry"/> have their "Project" field initialized </remarks>
        /// <param name="index"> index of entry within given month (within monthly user file)</param>
        /// <param name="month"> specific month of a year</param>
        /// <param name="loadedEntry"> list of entries loaded from file </param>
        /// <returns>Whether a valid entry list has been loaded or not</returns>
        public static bool GetFullEntryByIndex(out Entry loadedEntry, DateTime month, int index, string username)
        {
            var tempEntries = new List<Entry>();
            var loadedEntries = new List<Entry>();
            loadedEntry = null;
            if (GetEntriesForMonth(out tempEntries, month, username))
            {

                loadedEntries.Add(tempEntries.Find(x => x.Index == index));
                ObtainProjectCopies(ref loadedEntries);
                loadedEntry = loadedEntries[0];
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Overwrite entry file for month <paramref name="month"/> wil entries 
        /// </summary>
        /// <remarks> Sorts <paramref name="entries"/> by Index before saving to file </remarks>
        /// <param name="month"> project to update </param>
        /// <param name="entries"> list of all Entries within <paramref name="month"/> </param>
        public static void SaveEntries(DateTime month, ref List<Entry> entries, string username)
        {
            var comparer = new EntryComparer();
            entries.Sort(comparer);
            string fileName = ConstructEntryFileName(month, username);
            SaveToFile(fileName, entries);
        }

        /// <summary>
        ///  Fill in Project fields of all entries
        /// </summary>
        private static void ObtainProjectCopies(ref List<Entry> entries)
        {
            List<Project> projects = GetProjects();
            // obtain details about project
            foreach (var entry in entries)
            {
                Project referredProject = projects.Find(project => project.ProjectCode == entry.ProjectCode);
                if (referredProject != null)
                {
                    entry.Project = referredProject;
                }
            }
        }

        public static void UpdateProjectBudget(string projectCode, int budgetChangeAmount)
        {
            if(GetProject(out var projectToUpdate, projectCode))
            {
                projectToUpdate.Budget += budgetChangeAmount;
                UpdateProject(projectToUpdate);
            }
        }

        private static string ConstructEntryFileName(DateTime date, string username)
        {
            string fileName = username;
            string dateString = date.Year.ToString() + "-" + date.Month.ToString();
            fileName += "-" + dateString + _entriesFileType;
            return fileName;
        }
        #endregion
    }
}