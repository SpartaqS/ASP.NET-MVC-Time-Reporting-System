# Task description
The task was to create a Time Reporting System app in ASP.NET MVC.

# How to use:
1. run the app:
    dotnet run --urls=http://localhost:5000/
2. connect to the server with a browser (currently the adress set to localhost:5000)

# App data:
Exists within the same folder as the Startup.cs file
If nonexistent, a "projects.json" file is created (defining all projects that are available for users to add entries to)

# User Interface:
Login Screen:
![Login View](https://github.com/SpartaqS/ASP.NET-MVC-Time-Reporting-System/blob/main/docs/login.png)
Proper user authentication was not required in the task, therefore a simple username input screen was implemented.
After "logging in", user's today's entries are shown:

Daily Entries Screen:
![Daily View](https://github.com/SpartaqS/ASP.NET-MVC-Time-Reporting-System/blob/main/docs/daily-view.png)
"Show Monthly Report" button allows to see a monthly summary for the selected month.
Utilizing the "Add Entry" button, user may create a new entry for the selected date.
Each entry can be seen in detail, modified and deleted.

Monthly Report Screen:
![Monthly Report](https://github.com/SpartaqS/ASP.NET-MVC-Time-Reporting-System/blob/main/docs/monthly-report.png)
Here the total time spent on each project during a month can be seen.
Utilizing the "Daily View" button, user may return to the daily entries screen.


Entry Manipulation:
![Add a new entry](https://github.com/SpartaqS/ASP.NET-MVC-Time-Reporting-System/blob/main/docs/entry-add.png)
![See entry details](https://github.com/SpartaqS/ASP.NET-MVC-Time-Reporting-System/blob/main/docs/entry-details.png)
![See project details](https://github.com/SpartaqS/ASP.NET-MVC-Time-Reporting-System/blob/main/docs/project-details.png)
![Edit entry](https://github.com/SpartaqS/ASP.NET-MVC-Time-Reporting-System/blob/main/docs/entry-edit.png)
![Delete entry](https://github.com/SpartaqS/ASP.NET-MVC-Time-Reporting-System/blob/main/docs/entry-delete.png)