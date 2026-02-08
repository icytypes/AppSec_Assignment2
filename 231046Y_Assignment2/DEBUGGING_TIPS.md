# Visual Studio Debugging Tips

## If Visual Studio Asks to "Attach to Process"

This usually happens when:
1. The application is already running
2. Visual Studio can't find the process to debug
3. There's a port conflict

## Solution 1: Run Without Debugging (Recommended for First Time)

1. In Visual Studio, go to **Debug** menu
2. Select **Start Without Debugging** (or press **Ctrl + F5**)
3. This will run the app without attaching the debugger

## Solution 2: Stop Existing Process

If the app is already running:

1. **Check Task Manager:**
   - Press `Ctrl + Shift + Esc`
   - Look for `dotnet.exe` or your app name
   - End the process if found

2. **Or use PowerShell:**
   ```powershell
   Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"} | Stop-Process -Force
   ```

3. Then try running again

## Solution 3: Use Command Line Instead

If Visual Studio keeps asking, use command line:

1. Open **PowerShell** or **Terminal**
2. Navigate to project folder:
   ```powershell
   cd "C:\Users\yiter\source\repos\231046Y_Assignment2\231046Y_Assignment2"
   ```
3. Run:
   ```powershell
   dotnet run
   ```
4. Open browser manually: `https://localhost:7130`

## Solution 4: Change Launch Settings

1. In Visual Studio, right-click on the project
2. Select **Properties**
3. Go to **Debug** → **General**
4. Uncheck **"Enable Just My Code"** if needed
5. Or change the launch profile

## Quick Fix: Just Click "No" or "Cancel"

If Visual Studio asks to attach to a process:
- Click **"No"** or **"Cancel"**
- Then press **Ctrl + F5** (Start Without Debugging)
- The app will run normally

## Recommended: Start Without Debugging

For development and testing, use:
- **Ctrl + F5** = Start Without Debugging (faster, no debugger)
- **F5** = Start With Debugging (slower, with debugger attached)

For your first run, use **Ctrl + F5** to avoid the attach dialog!
