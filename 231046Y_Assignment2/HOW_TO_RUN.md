# How to Run the Fresh Farm Market Application

## Prerequisites
- ✅ .NET 8.0 SDK (already installed)
- ✅ SQLite (no installation needed - included with .NET)

## Method 1: Using Command Line (Recommended)

### Step 1: Open Terminal/PowerShell
- Press `Win + X` and select "Terminal" or "PowerShell"
- Or open Command Prompt

### Step 2: Navigate to Project Folder
```powershell
cd "C:\Users\yiter\source\repos\231046Y_Assignment2\231046Y_Assignment2"
```

### Step 3: Run the Application
```powershell
dotnet run
```

### Step 4: Open in Browser
After running, you'll see output like:
```
Now listening on: http://localhost:5233
Now listening on: https://localhost:7130
```

- Open your browser
- Go to: **https://localhost:7130** (HTTPS) or **http://localhost:5233** (HTTP)
- If you see a security warning, click "Advanced" → "Proceed to localhost"

### Step 5: Stop the Application
- Press `Ctrl + C` in the terminal to stop

---

## Method 2: Using Visual Studio

### Step 1: Open Project
1. Open **Visual Studio**
2. Click **File** → **Open** → **Project/Solution**
3. Navigate to: `C:\Users\yiter\source\repos\231046Y_Assignment2\`
4. Select `231046Y_Assignment2.sln` or the `.csproj` file
5. Click **Open**

### Step 2: Run
1. Press **F5** (or click the green "Play" button)
2. Visual Studio will:
   - Build the project
   - Start the web server
   - Open your browser automatically

### Step 3: Stop
- Click the **Stop** button in Visual Studio
- Or press **Shift + F5**

---

## Method 3: Using Visual Studio Code

### Step 1: Open Project
1. Open **Visual Studio Code**
2. Click **File** → **Open Folder**
3. Navigate to: `C:\Users\yiter\source\repos\231046Y_Assignment2\231046Y_Assignment2`
4. Click **Select Folder**

### Step 2: Open Terminal
- Press `` Ctrl + ` `` (backtick) to open terminal
- Or go to **Terminal** → **New Terminal**

### Step 3: Run
```powershell
dotnet run
```

### Step 4: Open Browser
- The terminal will show the URL
- Press `Ctrl + Click` on the URL to open in browser
- Or manually go to: `https://localhost:7130`

---

## First Time Setup

When you run the application for the first time:

1. **Database will be created automatically** - `FreshFarmMarketDb.db` file will appear in your project folder
2. **All tables will be set up** - Members, AuditLogs, PasswordHistories, PasswordResetTokens
3. **You can start using the application immediately**

---

## Testing the Application

### 1. Register a New Account
- Go to: `https://localhost:7130/Register`
- Fill in all the form fields:
  - Full Name
  - Credit Card Number
  - Gender
  - Mobile Number
  - Delivery Address
  - Email (must be unique)
  - Password (min 12 chars, with uppercase, lowercase, numbers, special chars)
  - Confirm Password
  - Photo (.JPG only)
  - About Me
- Complete the reCAPTCHA
- Click **Register**

### 2. Login
- Go to: `https://localhost:7130/Login`
- Enter your email and password
- Complete the reCAPTCHA
- Click **Login**

### 3. View Homepage
- After login, you'll see your member information
- Credit card number will be displayed (decrypted)
- You can change password, enable 2FA, or logout

---

## Troubleshooting

### Port Already in Use
If you see "Port 5233 is already in use":
```powershell
# Find and kill the process
netstat -ano | findstr :5233
taskkill /PID <PID_NUMBER> /F
```

### Database Locked Error
- Close the application
- Delete `FreshFarmMarketDb.db` file
- Run again (database will be recreated)

### Build Errors
```powershell
# Clean and rebuild
dotnet clean
dotnet build
dotnet run
```

---

## Application URLs

- **Homepage:** `https://localhost:7130/` or `http://localhost:5233/`
- **Register:** `https://localhost:7130/Register`
- **Login:** `https://localhost:7130/Login`
- **Change Password:** `https://localhost:7130/ChangePassword`
- **Forgot Password:** `https://localhost:7130/ForgotPassword`
- **Enable 2FA:** `https://localhost:7130/Enable2FA`

---

## Quick Start Command

```powershell
cd "C:\Users\yiter\source\repos\231046Y_Assignment2\231046Y_Assignment2"
dotnet run
```

Then open: **https://localhost:7130**
