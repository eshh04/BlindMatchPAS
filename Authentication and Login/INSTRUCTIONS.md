# M1 — Push 3: Authentication & Access Control

**You push THIRD** (after M7 and M3).

## Commit Message
```
feat: implement Identity authentication, RBAC policies, and role-access middleware
```

## Steps

1. **Pull the latest code:**
   ```
   cd BlindMatchPAS
   git pull origin main
   ```

2. **Extract this folder's contents into the repo root.**
   This will:
   - OVERWRITE `BlindMatchPAS/Program.cs` (adds Identity, cookie auth, authorization policies, role seeding)
   - OVERWRITE `BlindMatchPAS/Views/Shared/_Layout.cshtml` (adds auth-conditional navbar with role-specific links)
   - ADD `BlindMatchPAS/Controllers/AccountController.cs`
   - ADD `BlindMatchPAS/Middleware/RoleAccessMiddleware.cs`
   - ADD `BlindMatchPAS/ViewModels/LoginViewModel.cs`
   - ADD `BlindMatchPAS/ViewModels/RegisterViewModel.cs`
   - ADD `BlindMatchPAS/Views/Account/` (3 views)

3. **Verify it builds:**
   ```
   cd BlindMatchPAS
   dotnet build
   ```
   Expected: Build succeeded with 0 errors.

4. **Commit and push:**
   ```
   cd ..
   git add .
   git commit -m "feat: implement Identity authentication, RBAC policies, and role-access middleware"
   git push origin main
   ```

## What You're Adding (9 files)

### Modified Files
- `BlindMatchPAS/Program.cs` — Adds ASP.NET Core Identity, cookie authentication, RBAC authorization policies, role-access middleware, and admin account seeding (admin@blindmatch.ac.uk / Admin@2026!)
- `BlindMatchPAS/Views/Shared/_Layout.cshtml` — Full auth-conditional navbar with role-specific navigation links and user dropdown

### New Files
- `BlindMatchPAS/Controllers/AccountController.cs` — Login, Register, Logout, AccessDenied actions
- `BlindMatchPAS/Middleware/RoleAccessMiddleware.cs` — Restricts controller access based on user roles
- `BlindMatchPAS/ViewModels/LoginViewModel.cs` — Login form validation model
- `BlindMatchPAS/ViewModels/RegisterViewModel.cs` — Registration form validation model
- `BlindMatchPAS/Views/Account/Login.cshtml` — Login page (standalone layout with dark theme)
- `BlindMatchPAS/Views/Account/Register.cshtml` — Registration page (standalone layout with dark theme)
- `BlindMatchPAS/Views/Account/AccessDenied.cshtml` — Access denied page
