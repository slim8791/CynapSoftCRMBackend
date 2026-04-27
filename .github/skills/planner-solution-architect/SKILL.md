---
name: planner-solution-architect
description: "Skill for the planner subagent to create a detailed plan as a solution architect for Angular projects."
---

# Planner Solution Architect Skill

## Purpose
This skill enables the planner subagent to act as a solution architect, creating a comprehensive plan for Angular projects. The plan includes:
- Page structure and navigation flow.
- Required services and their responsibilities.
- Interceptors for API communication.
- Component file structure with separated TS, HTML, and CSS files.
- Integration points with backend APIs.

## Component File Structure Requirements
All Angular components, pages, modules, and features MUST be planned with this architecture:
```
component-name/
├── component-name.component.ts
├── component-name.component.html
└── component-name.component.css
```

**Standards:**
- Separate TypeScript logic from templates and styles
- Use `templateUrl` for external HTML references
- Use `styleUrls` for external CSS references
- Apply this consistently across the **entire application**

## Workflow
1. **Analyze Requirements**: Gather and analyze project requirements.
2. **Define Page Structure**: Outline the pages and their navigation hierarchy.
3. **Plan Component Architecture**: Define component file structure with separated TS/HTML/CSS for all pages and modules.
4. **Identify Services**: List the services needed and their responsibilities.
5. **Plan Interceptors**: Define interceptors for handling API requests and responses.
6. **Integration Points**: Map out the backend APIs to be consumed.
7. **Generate Plan Document**: Create a markdown document summarizing the plan, including component file structure details.

## Output
- A markdown document detailing the project plan with component architecture specifications.