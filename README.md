# Draft Dominator Sleeper Draft Sync
Sync live Sleeper drafts with Draft Dominator

## How To Use

**WARNING:** this is more of an experiment and not thoroughly tested. If you use this, be prepared to take over manually entering picks in the event the playwright script fails.

* Ensure you have dotnet sdk and powershell core installed. This project targets dotnet 6.0
* Run `dotnet restore`
* Run `dotnet build`
* Run `pwsh bin/Debug/net6.0/playwright.ps1 install` to install chromium
* Run `dotnet run` in the project directory
* Enter the requested information
* You should see a chromium browser launch and create a new live draft
* One the draft on sleeper starts, the chromium window should start automatically entering the picks.
* Be sure to enter your picks on sleeper, NOT in Draft Dominator
