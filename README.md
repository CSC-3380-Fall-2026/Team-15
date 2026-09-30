# Beyond The Last Goodbye : Team 15
# Members
Project Manager: Justin Mexil (@justin2flyy)\
Communications Lead: Antonio Johnson (@johnsonantonio2004-creator)\
Git Master: Javin Vezinat (@JavinVez)\
Design Lead: Isaac Moreno (@IsaacMoreno7)\
Quality Assurance Tester: Jacob Wear ([GitHub Name])

# About Our Software

Beyond the Last Goodbye is a single player, 2D dungeon crawler, about loss & hope, built in Godot 4 with C#.


## Platforms Tested on
- Windows


# Important Links
Kanban Board: (https://trello.com/b/pkMnrOM0/beyond-the-last-goodbye)
\
Designs: [link]\
Styles Guide(s): [link]

## How to Run Dev and Test Environment

### Dependencies
- Godot Engine 4.7.2 .NET (the ".NET" build with C# support, not the standard build)
- .NET SDK 8.0 or newer (tested with 10.0.401)
- Git (tested with 2.54.0)
- Optional IDE: Visual Studio Code (free) with the **C# Dev Kit** and **godot-tools** extensions

### Downloading Dependencies
1. **Godot 4.7.2 .NET:** https://godotengine.org/download → download the **Windows – .NET – x86_64** build. Unzip it to a permanent folder (e.g. `C:\Godot\`). Keep the `GodotSharp` folder next to the `.exe`.
2. **.NET SDK:** https://dotnet.microsoft.com/download → install the latest **SDK** (not Runtime) for **Windows x64**. Or in PowerShell:
```
   winget install Microsoft.DotNet.SDK.10
```
3. **Git:** https://git-scm.com/downloads
4. Verify installs in a new PowerShell window:
```
   git --version
   dotnet --version
```

> If Windows blocks Godot from opening, run this in PowerShell to unblock the downloaded files:
> ```
> Get-ChildItem -Path "C:\Godot" -Recurse | Unblock-File
> ```

### Commands
Clone the repo and switch to the dev branch:
```
git clone https://github.com/CSC-3380-Fall-2026/Team-15.git
cd Team-15
git checkout dev
```

**Run from the Godot editor:**
1. Open Godot 4.7.2 .NET → **Import** → select `project.godot` in the Team-15 folder
2. Click the **hammer icon** (Build) in the top right
3. Press **F5** to run the game

**Run from the command line** (from inside the Team-15 folder):
```
dotnet build
& "C:\Godot\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe" --path .
```
Change the Godot path if you unzipped it somewhere else. The Output should show `Beyond The Last Goodbye loaded`.
