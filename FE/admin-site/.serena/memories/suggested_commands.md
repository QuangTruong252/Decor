# Suggested Commands

## Development Commands
```bash
# Start development server with Turbopack on port 4200
npm run dev

# Build for production
npm run build

# Start production server
npm run start

# Run ESLint
npm run lint
```

## Windows Utility Commands
```powershell
# List files/directories
dir
Get-ChildItem

# Change directory
cd <path>
Set-Location <path>

# Search files
Get-ChildItem -Recurse -Filter "*.tsx"

# Find text in files
Select-String -Path "*.tsx" -Pattern "searchtext"

# Git commands
git status
git add .
git commit -m "message"
git push
```

## Project Structure Navigation
```powershell
# Navigate to main directories
cd app        # Routes and pages
cd components # React components
cd contexts   # React contexts
cd services   # API services
cd hooks      # Custom hooks
```