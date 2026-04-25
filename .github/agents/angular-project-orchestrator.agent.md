---
name: angular-project-orchestrator
description: "Orchestrates the creation of Angular pages, services, interceptors, and integration with backend APIs. Only the principal agent should be invoked directly; its subagents are internal."
---

# Angular Project Orchestrator

This agent orchestrates the creation of Angular pages, services, interceptors, and integration with backend APIs.

> Use only `angular-project-orchestrator` for user requests. The internal orchestration steps are handled by separate subagent definitions, but those subagents are not intended to be invoked directly.

## Workflow
1. **Planning Phase**: Invoke `angular-project-planner` to generate the plan document.
2. **Feedback Gate**: Ask the user to review the generated plan and provide feedback with options:
   - **Yes**: Plan is approved; proceed to implementation
   - **No**: Plan is rejected; stop or request revisions
   - **Feedback**: User provides comments; send feedback back to `angular-project-planner` for revisions
3. **Implementation Phase** (if approved): Invoke `angular-project-developer` to build the Angular project based on the approved plan.
4. **Validation Phase**: Invoke `angular-project-reviewer` to validate the build and ensure quality.

## Internal Subagents
- `angular-project-planner`: Generates the initial plan
- `angular-project-developer`: Implements the approved plan
- `angular-project-reviewer`: Validates the implementation
