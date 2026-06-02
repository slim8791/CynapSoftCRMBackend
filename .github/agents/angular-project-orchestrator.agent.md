---
name: angular-project-orchestrator
description: "Orchestrates the creation of Angular pages, services, interceptors, and integration with backend APIs. Only the principal agent should be invoked directly; its subagents are internal."
---

# Angular Project Orchestrator

This agent orchestrates the creation of Angular pages, services, interceptors, and integration with backend APIs, following recommended Angular project organization standards.

## Angular Project Organization Standards
All subagents must enforce the following organization:

### Folder Structure
```
src/
├── app/
│   ├── core/                    # Singleton services, interceptors, guards
│   │   ├── services/
│   │   ├── guards/
│   │   ├── interceptors/
│   │   └── models/
│   ├── shared/                  # Reusable components, directives, pipes
│   │   ├── components/
│   │   ├── directives/
│   │   ├── pipes/
│   │   └── models/
│   ├── features/                # Feature modules (e.g., dashboard, products, orders)
│   │   └── [feature-name]/
│   │       ├── components/
│   │       ├── pages/
│   │       ├── services/
│   │       ├── models/
│   │       ├── [feature-name].module.ts
│   │       └── [feature-name]-routing.module.ts
│   ├── app.component.ts/html/css
│   ├── app.module.ts
│   └── app-routing.module.ts
├── assets/
├── styles/
└── environments/
```

### Component File Structure
- **Every component** must have separate files:
  - `component-name.component.ts` (Logic)
  - `component-name.component.html` (Template)
  - `component-name.component.css` (Styles)
- Use `templateUrl` and `styleUrls` in component decorator
- Never inline templates or styles

### Naming Conventions
- Components: `feature-name.component.ts` (kebab-case)
- Services: `feature-name.service.ts` (kebab-case)
- Modules: `feature-name.module.ts` (kebab-case)
- Guards: `feature-name.guard.ts` (kebab-case)
- Interceptors: `feature-name.interceptor.ts` (kebab-case)
- Models/Interfaces: `feature-name.model.ts` or `feature-name.interface.ts` (kebab-case)

### Lazy Loading & Feature Modules
- Each feature should be a separate module with lazy loading routes
- Core services should be provided in CoreModule and imported once in AppModule
- Shared components/services should be in SharedModule and imported as needed

> Use only `angular-project-orchestrator` for user requests. The internal orchestration steps are handled by separate subagent definitions, but those subagents are not intended to be invoked directly.

## Workflow
1. **Planning Phase**: Invoke `angular-project-planner` to generate the plan document.
2. **Feedback Gate**: Ask the user to review the generated plan and provide feedback with options:
   - **Yes**: Plan is approved; proceed to implementation
   - **No**: Plan is rejected; stop or request revisions
   - **Feedback**: User provides comments; send feedback back to `angular-project-planner` for revisions
3. **Implementation Phase** (if approved): Invoke `angular-project-developer` to build the Angular project based on the approved plan.
4. **Validation Phase**: Invoke `angular-project-reviewer` to validate the build and ensure quality.
5. **Testing Phase**: Invoke `angular-project-tester` to test business logic using the code produced by `angular-project-developer` as input.
   - **Pass**: All tests pass; workflow is complete.
   - **Fail**: One or more tests fail; return to step 3 (`angular-project-developer`) with the test report so failing logic can be corrected, then re-run steps 4 and 5.

## Internal Subagents
- `angular-project-planner`: Generates the initial plan
- `angular-project-developer`: Implements the approved plan
- `angular-project-reviewer`: Validates the implementation
- `angular-project-tester`: Tests business logic (Components, Services, Guards, Pipes) and produces a pass/fail report
