E-Shift Application Setup Guide
This guide provides instructions to set up and run the E-Shift application on your local machine.

Prerequisites
Before you begin, ensure you have the following software installed:

Visual Studio 2022 (with the ASP.NET web development workload enabled)

.NET 8 SDK

SQL Server

Setup Steps
Follow these steps to get the E-Shift application up and running:

a) Obtain Source Code
Choose one of the following methods to get the application's source code:

Clone from GitHub:
Open your Command Prompt or Git Bash and execute the following commands:

git clone https://github.com/SimonZarni/EShift.git
cd EShift

(Optional: You can include an image here showing the git clone command in a terminal.)

Extract Compressed Project File:
Alternatively, extract the provided compressed project file (e.g., .zip, .rar) to your desired local directory.

b) Open in Visual Studio
Launch Visual Studio 2022.
Go to File > Open > Project/Solution... (or select "Open a solution or project" from the start screen).
Navigate to the directory where you obtained the source code and select the EShift.sln solution file.

c) Restore NuGet Packages
Visual Studio typically auto-restores NuGet packages when you open a solution. If, for any reason, you notice missing references or build errors related to packages, you can manually restore them:

In Solution Explorer, right-click on the solution (the top-level item).

Select "Restore NuGet Packages" from the context menu.

d) Configure Database
You need to update the database connection string to match your local SQL Server setup.

Open the appsettings.json file located in the root of the EShift project.

Locate the "DefaultConnection" string within the "ConnectionStrings" section and modify its value to point to your SQL Server instance.

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=EShiftDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}

Note: Replace YOUR_SERVER_NAME with the actual name or IP address of your SQL Server. For SQL Server Express LocalDB, it might be (localdb)\\mssqllocaldb.

e) Apply Migrations
To create the necessary database schema and seed initial data, apply the Entity Framework Core migrations:

Open the Package Manager Console in Visual Studio. You can find it by navigating to Tools > NuGet Package Manager > Package Manager Console.

In the Package Manager Console, ensure the "Default project" dropdown is set to EShift.

Run the following commands:

If this is the very first time setting up the project and no migrations exist, run:

Add-Migration InitialCreate

Then, to apply the migrations to your database (or create the database if it doesn't exist), run:

Update-Database

After running Update-Database, an admin user will be seeded into the database with the following credentials:

Email: admin@eshift.com

Password: Admin123!

f) Run Application
Once the database is configured and migrations are applied, you can run the application:

Click the green "Run" button (often labeled "IIS Express" or the project name) located in the Visual Studio toolbar.
(Optional: You can include an image here pointing to the green run button in Visual Studio.)

Visual Studio will build the application and automatically open it in your default web browser.

g) Access Application
The application will typically open in your browser at a URL similar to https://localhost:XXXX/ (where XXXX is a dynamically assigned port number).

You can then navigate to specific sections of the system using URLs like:

https://localhost:XXXX/Jobs

https://localhost:XXXX/Loads

https://localhost:XXXX/TransportUnits

You are now ready to interact with the E-Shift application!
