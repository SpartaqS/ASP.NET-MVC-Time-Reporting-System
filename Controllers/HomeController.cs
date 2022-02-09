using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TRS.Models;
using TRS.Data;
using TRS.Filters;

namespace TRS.Controllers
{
    [LoginStateFilter]
    public class HomeController : Controller
    {
        private string _user;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Logout()
        {
            return RedirectToAction("Logout","Login"); // log out the user   
        }

        public IActionResult Index() // default view
        {
            _user = Request.Cookies["User"];
            return DailyView(DateTime.Now);
        }
        public IActionResult DailyView(DateTime date) // use to see different days
        {
            JsonIO.GetFullEntriesForDay(out var loadedEntries, date, GetUsename()); // view can handle no entries at all
            DailyScreen dailyScreen = new DailyScreen(date, loadedEntries);
            return View("Index", dailyScreen);
        }

        public IActionResult MonthlyView(DateTime month) // any day of the requested month
        {
            string username = GetUsename();
            JsonIO.GetEntriesForMonth(out var monthlyEntries, month, username); // even getting no entries is ok : we will simpy display nothing
            
            Dictionary<string, Project> monthlyProjects =  new Dictionary<string, Project>(); // will utilize the Project model to collect total time spent on project

            foreach(var entry in monthlyEntries)
            {
                if(!monthlyProjects.ContainsKey(entry.ProjectCode))
                {
                    if(!JsonIO.GetProject(out var project, entry.ProjectCode))
                    {
                        project = new Project{
                            ProjectCode = entry.ProjectCode,
                            ProjectName = "Undefined project" + entry.ProjectCode,
                            Budget = 0,
                            Active = false,
                        };
                    }
                    project.Budget = 0;
                    monthlyProjects.Add(entry.ProjectCode, project);
                }
                monthlyProjects[entry.ProjectCode].Budget += entry.Duration; // add entry time to total time spent
            }

            List<Project> monthlyProjectsTimeSpent = monthlyProjects.Values.ToList();
            var monthlyScreen = new MonthlyReport(month, monthlyProjectsTimeSpent);
            return View("MonthlyView", monthlyScreen);
        }

        public IActionResult DetailView(DateTime date, int index)
        {
            JsonIO.GetFullEntryByIndex(out Entry entryToDetail, date, index, GetUsename());
            return View(entryToDetail);
        }

        public IActionResult EntryAdd(DateTime date)
        {
            var projects = JsonIO.GetProjects();
            Entry newEntry = new Entry();
            newEntry.Date = date;
            newEntry.Index = -1; // to mark that this is an addition, not edit
            EntryEdit addScreen = new EntryEdit(projects, newEntry);
            return View("EntryEdit", addScreen);
        }
        public IActionResult EntryEdit(DateTime date, int index)
        {
            var projects = JsonIO.GetProjects();
            Entry editedEntry;
            if (!JsonIO.GetFullEntryByIndex(out editedEntry, date, index, GetUsename()))
            {// for some reason entry was not reached
                // Show error that entry no longer exists
                //return EntryAdd(date);
                return NotFound();
            }
            // entry to edit was found in the database
            EntryEdit editScreen = new EntryEdit(projects, editedEntry);
            return View("EntryEdit", editScreen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EntryEdit(Entry entry)
        {
            string username = GetUsename();
            if (entry.Index < 0)
            {// creating a new entry
                JsonIO.GetEntriesForMonth(out var monthlyEntries, entry.Date, username);
                // even if no entries were found, that's ok because we will add the new entry to the empty list
                monthlyEntries.Add(entry); // add the entry
                JsonIO.UpdateProjectBudget(entry.ProjectCode, -entry.Duration); // update project budget
                JsonIO.SaveEntries(entry.Date, ref monthlyEntries, username); // sort entries and save
                entry.Index = monthlyEntries.IndexOf(entry); // obtain the index of the new entry
            }
            else
            {// modifying an existing entry
                if (!JsonIO.GetEntriesForMonth(out var monthlyEntries, entry.Date, username))
                {
                    return NotFound();
                }
                else
                {
                    if(monthlyEntries.Count < entry.Index)
                        return NotFound();
                    
                    Entry oldEntry = monthlyEntries[entry.Index];
                    if(entry.ProjectCode != oldEntry.ProjectCode)
                    { // project has changed

                        JsonIO.UpdateProjectBudget(oldEntry.ProjectCode, oldEntry.Duration);
                        JsonIO.UpdateProjectBudget(entry.ProjectCode, -entry.Duration);
                    }
                    else // no project change - just update the budget
                    {
                        int budgetChange = oldEntry.Duration - entry.Duration;
                        if(budgetChange != 0) // no need to update the budget if there was no change in duration
                            JsonIO.UpdateProjectBudget(entry.ProjectCode, budgetChange);
                    }
                    monthlyEntries[entry.Index] = entry; // replace edited entry with its new version
                    monthlyEntries.Sort(new EntryComparer());
                    entry.Index = monthlyEntries.IndexOf(entry); // update the index of the entry (so we can display its details correctly)
                    JsonIO.SaveEntries(entry.Date, ref monthlyEntries, username); // sort entries and save
                }
            }
            // try to show the detailed view of the created/edited entry
            if (!JsonIO.GetFullEntryByIndex(out entry, entry.Date, entry.Index, username))
            {
                return NotFound();
            }
            return View("DetailView", entry);
        }

        public IActionResult EntryDelete(DateTime date, int index)
        {
            var projects = JsonIO.GetProjects();
            Entry deletedEntry;
            if (!JsonIO.GetFullEntryByIndex(out deletedEntry, date, index, GetUsename()))
            {// for some reason entry was not found
                // Show error that entry no longer exists
                //return EntryAdd(date);
                return NotFound();
            }
            // entry to edit was found in the database
            return View("EntryDelete", deletedEntry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EntryDeleteConfirm(DateTime date, int index)
        {
            string username = GetUsename();
            if (!JsonIO.GetEntriesForMonth(out var monthlyEntries, date, username))
            {
                return NotFound();
            }
            else
            {
                if(monthlyEntries.Count < index)
                    return NotFound();
                
                Entry entryToDelete = monthlyEntries[index]; // update the budget (deleted entry no longer taxes the budget)
                JsonIO.UpdateProjectBudget(entryToDelete.ProjectCode, entryToDelete.Duration);

                monthlyEntries.RemoveAt(index);// remove the entry from the monthly activity list
                JsonIO.SaveEntries(date, ref monthlyEntries, username); // sort entries and save
            }

            return DailyView(date);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string GetUsename()
        {
            return Request.Cookies[TRS.Constants.USER_COOKIE];// safe to use because the interceptor makes sure that the cookie exists
        }
    }
}
