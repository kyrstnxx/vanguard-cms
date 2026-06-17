# 🟩 Vanguard CMS

Welcome to Vanguard CMS. Developed as a final project for the **Applications Development and Emerging Technologies (ADET)** course, Vanguard is a comprehensive complaint management system designed to empower residents to report issues, and equip administrators with the tools they need to track, manage, and resolve them with full accountability. Built with a sleek, responsive UI and a highly secure ASP.NET Core backend, Vanguard ensures every voice is heard and every action is tracked from the first report to the final resolution.

## Tech Stack

* **Framework:** ASP.NET Core MVC (.NET)
* **Database:** SQL Server & Entity Framework Core (EF Core)
* **Authentication:** ASP.NET Core Identity (Role-Based Access Control)
* **Frontend:** HTML5, CSS3, Bootstrap 5.3
* **Icons:** Tabler Icons
* **Architecture:** Model-View-Controller (MVC)

## Key Features

* **Role-Based Security:** Distinct dashboards and permissions for **Admin** staff and standard **Residents**.
* **Complaint Lifecycle Management:** Residents can seamlessly submit and track their own reports, while Admins possess full oversight to review and manage all system data.
* **Automated Audit Trails:** Every status update and resolution is logged for complete transparency.

## Getting Started (Local Development)

Follow these steps to get Vanguard CMS running on your local machine.

### Prerequisites
* SQL Server
* Visual Studio 2022 / 2026
* .NET 8 SDK (or compatible version)

### Installation & Setup

**1. Clone the repository:**
Open your terminal and pull down the project files:
```bash
git clone [https://github.com/kyrstnxx/vanguard-cms.git](https://github.com/kyrstnxx/vanguard-cms.git)
cd vanguard-cms/complaint_MS
```


**2. Configure Local Settings:**
For security purposes, default passwords and sensitive connection strings are not hardcoded into the repository. You must configure your local environment manually before running the app.

* Step A: Open the project in your code editor and locate the appsettings.json file in the root of the complaint_MS folder.

* Step B: Inside the JSON file, locate the "ConnectionStrings" block and set your "DefaultConnection" to point to your local SQL Server instance. It should look like this:
```bash
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=complaint_MS_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
```
* Step C: Directly below the ConnectionStrings block, create a new section named "SeedCredentials". This will define the initial passwords for your test Admin and Resident accounts when the database generates.
```bash
"SeedCredentials": {
    "AdminPassword": "YourSecureAdminPassword123!",
    "ResidentPassword": "YourSecureResidentPassword123!"
  }
```
* Step D: Save the appsettings.json file.


**3. Build the Database:**
Apply the Entity Framework migrations to build your local SQL database.
```bash
Update-Database
```

### Git Workflow & Collaboration
When contributing to Vanguard CMS, please follow this standard Git workflow to keep the main branch stable and avoid merge conflicts.

- Get the Latest Changes

Always pull the latest code from the repository before starting new work:
```Bash
git pull origin main
```

- Create a New Branch 
Never work directly on the main branch. Create a new branch for your specific feature or bug fix:

```Bash
# Replace 'feature-name' with a descriptive name (e.g., add-login-UI)
git checkout -b feature-name
```

- Save and Commit Your Work 
Once you have made your changes, stage and commit them with a clear, descriptive message:
```Bash
git add .
git commit -m "Added the resident dashboard interface"
```

- Push Your Branch
Push your new branch up to GitHub:
```Bash
git push -u origin feature-name
```
