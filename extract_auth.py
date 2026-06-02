import os
import re
import glob

def parse_controllers():
    controller_files = []
    for root, dirs, files in os.walk('.'):
        for file in files:
            if file.endswith('Controller.cs'):
                controller_files.append(os.path.join(root, file))
                
    markdown_content = "# Backend API Authorization Analysis\n\n"
    markdown_content += "This document lists all API endpoints across the microservices, detailing their required permissions and roles.\n\n"

    for file_path in controller_files:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()

        class_match = re.search(r'class\s+(\w+Controller)', content)
        if not class_match:
            continue
        class_name = class_match.group(1)

        # Get service name from path (e.g., CynapCRM.Services.DocAPI)
        service_name = "Unknown Service"
        path_parts = file_path.split(os.sep)
        for part in path_parts:
            if "API" in part and part.startswith("CynapCRM"):
                service_name = part
                break

        class_route_match = re.search(r'\[Route\("([^"]+)"\)\][\s\S]*?class\s+' + class_name, content)
        class_route = class_route_match.group(1) if class_route_match else ""

        class_auth_match = re.search(r'\[Authorize(.*?)\][\s\S]*?class\s+' + class_name, content)
        class_auth = "[Authorize" + class_auth_match.group(1) + "]" if class_auth_match else ""
        
        class_anon_match = re.search(r'\[AllowAnonymous\][\s\S]*?class\s+' + class_name, content)
        if class_anon_match:
            class_auth = "[AllowAnonymous]"

        markdown_content += f"## {class_name} ({service_name})\n"
        if class_route:
            markdown_content += f"**Base Route:** `{class_route}`\n\n"
        markdown_content += f"**Class Level Authorization:** `{class_auth if class_auth else 'None'}`\n\n"
        
        markdown_content += "| Method | HTTP Verb | Route | Authorization | Access Control Explanation |\n"
        markdown_content += "|---|---|---|---|---|\n"

        lines = content.split('\n')
        current_http_verb = ""
        current_route = ""
        current_auth = ""
        in_method_attrs = False
        
        for i, line in enumerate(lines):
            line = line.strip()
            if line.startswith("[Http"):
                in_method_attrs = True
                verb_match = re.search(r'\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)', line)
                if verb_match:
                    current_http_verb = verb_match.group(1).replace("Http", "").upper()
                
                route_match = re.search(r'\("([^"]+)"\)', line)
                if route_match:
                    current_route = route_match.group(1)
            elif in_method_attrs and line.startswith("[Authorize"):
                current_auth = line
            elif in_method_attrs and line.startswith("[AllowAnonymous]"):
                current_auth = "[AllowAnonymous]"
            elif line.startswith("public") and "class" not in line and "interface" not in line and "(" in line:
                if in_method_attrs:
                    name_match = re.search(r'public\s+(?:async\s+)?(?:Task(?:<.*?>)?|IActionResult|ActionResult(?:<.*?>)?|\w+)\s+(\w+)\(', line)
                    if name_match:
                        method_name = name_match.group(1)
                        
                        final_auth = current_auth if current_auth else (class_auth if class_auth else "None")
                        
                        full_route = class_route
                        if current_route:
                            if not full_route.endswith('/'):
                                full_route += '/'
                            full_route += current_route
                            
                        explanation = ""
                        if final_auth == "[AllowAnonymous]":
                            explanation = "Public access, no token required."
                        elif final_auth == "[Authorize]":
                            explanation = "Requires valid authentication token."
                        elif "Roles" in final_auth:
                            roles_match = re.search(r'Roles\s*=\s*"([^"]+)"', final_auth)
                            if roles_match:
                                explanation = f"Requires valid token AND user must have one of these roles: **{roles_match.group(1)}**."
                        elif final_auth == "None":
                            explanation = "No explicit authorization rule found (might inherit global or fail open)."
                            
                        markdown_content += f"| {method_name} | {current_http_verb} | `{full_route}` | `{final_auth}` | {explanation} |\n"
                
                # Reset for next method
                current_http_verb = ""
                current_route = ""
                current_auth = ""
                in_method_attrs = False

        markdown_content += "\n---\n\n"

    with open("backend_auth_analysis.md", "w", encoding="utf-8") as f:
        f.write(markdown_content)

if __name__ == '__main__':
    parse_controllers()
