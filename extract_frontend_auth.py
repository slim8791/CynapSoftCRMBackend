import os
import re
import glob

def extract_angular_auth():
    app_dir = os.path.join("c:\\", "Users", "benjd", "Desktop", "BackendPFE", "CynapSoftCRMBackend", "Cynapharm", "src", "app")
    
    # Files to look for routes
    route_files = glob.glob(os.path.join(app_dir, "**", "*.routes.ts"), recursive=True)
    route_files += glob.glob(os.path.join(app_dir, "**", "*-routing.module.ts"), recursive=True)
    
    md = "# Angular Frontend Authorization Rules\n\n"
    md += "This document outlines how access control is managed in the Angular frontend application.\n\n"
    
    md += "## Core Authorization Concepts\n\n"
    md += "### 1. Guards\n"
    md += "- **`authGuard`**: Protects routes by ensuring the user is authenticated. It checks `authService.isAuthenticated()`. If not authenticated, it redirects to `/login` with the `returnUrl`.\n"
    md += "- **`roleGuard`**: Protects routes based on user roles. It expects a `data: { roles: [...] }` array in the route definition. It verifies if `authService.getUserRole()` is included in the allowed roles. If not, it redirects to `/forbidden`.\n\n"
    
    md += "### 2. User Roles\n"
    md += "Defined in `UserRole` enum (`auth.service.ts`):\n"
    md += "- `ADMIN`\n- `SUPERVISEUR`\n- `DELEGUE`\n- `MEDECIN`\n- `CLIENT`\n\n"
    
    md += "### 3. UI-Level Checks\n"
    md += "UI components use `authService.getUserRole()` or signals to show/hide elements. For instance, `isAdmin` or `isSuperviseur` boolean properties are populated and then used with `@if (isAdmin)` or `*ngIf=\"isAdmin\"` directives to selectively render buttons or table actions (e.g., in Orders, Reclamations, Navigation bar).\n\n"
    
    md += "---\n\n## Route Configuration\n\n"
    md += "| Route Path | CanActivate Guards | Allowed Roles | Description |\n"
    md += "|---|---|---|---|\n"
    
    # A bit of logic to parse typical Angular routes
    for file in route_files:
        with open(file, 'r', encoding='utf-8') as f:
            content = f.read()
            
        # Very simple regex extraction for `{ path: '...', ..., canActivate: [...], data: { roles: [...] } }`
        # We will parse blocks of { ... }
        
        # Regex to find route blocks
        route_blocks = re.findall(r'\{\s*path\s*:\s*[\'"]([^\'"]+)[\'"]([^}]*)\}', content)
        
        for path, props in route_blocks:
            guards = []
            roles = []
            
            can_activate_match = re.search(r'canActivate\s*:\s*\[([^\]]+)\]', props)
            if can_activate_match:
                guards = [g.strip() for g in can_activate_match.group(1).split(',')]
                
            roles_match = re.search(r'roles\s*:\s*\[([^\]]+)\]', props)
            if roles_match:
                # clean up UserRole.ADMIN to just ADMIN
                roles_raw = [r.strip() for r in roles_match.group(1).split(',')]
                roles = [r.replace('UserRole.', '') for r in roles_raw]
                
            guards_str = ", ".join(guards) if guards else "None (Public)"
            roles_str = ", ".join(roles) if roles else ("Any Authenticated" if 'authGuard' in guards else "All")
            
            if 'redirectTo' in props:
                continue # Skip redirects
                
            file_name = os.path.basename(file)
            md += f"| `{path}` | `{guards_str}` | `{roles_str}` | Defined in `{file_name}` |\n"
            
    md += "\n## Feature Modules Detail\n\n"
    md += "In addition to main routing, certain UI components have explicit access constraints:\n"
    md += "- **Reclamations**: Only `ADMIN` or `SUPERVISEUR` can change status. Only `ADMIN` has full delete privileges.\n"
    md += "- **Orders (Commandes)**: Only `ADMIN` can delete orders or see certain modification actions.\n"
    md += "- **Navigation Sidebar (`app.html`)**: Menu items like Users, Documents, Promo-stocks are strictly restricted to `ADMIN` or `SUPERVISEUR` via `*ngIf`.\n"

    with open(os.path.join("c:\\", "Users", "benjd", "Desktop", "BackendPFE", "CynapSoftCRMBackend", "Cynapharm", "frontend_auth_analysis.md"), "w", encoding="utf-8") as f:
        f.write(md)
        
if __name__ == '__main__':
    extract_angular_auth()
