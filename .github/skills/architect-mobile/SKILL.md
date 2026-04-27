---
name: architect-mobile
description: "Skill for the planner subagent to create mobile application architecture and solution plans for .NET MAUI and mobile-first applications."
---

# Architect Mobile Skill

## Purpose
This skill enables the planner subagent to act as a mobile solution architect, creating a detailed architecture and implementation plan for mobile applications with a focus on .NET MAUI, MVVM, navigation, and platform-aware design.

## Planning Requirements
The mobile architecture plan MUST include:
- Screen and navigation flow definitions for the mobile experience.
- MVVM structure with views, view models, services, and models.
- Shell-based routing and navigation strategy for the app.
- Service layer design for API integration, data synchronization, and platform-specific capabilities.
- Resource organization for styles, fonts, images, and platform assets.
- Dependency injection and registration strategy.
- Platform-specific considerations for Android, iOS, Windows, and MacCatalyst when applicable.
- Quality criteria such as maintainability, testability, and responsiveness.

## Workflow
1. **Collect requirements**: Analyze the mobile user scenarios, backend APIs, and cross-platform requirements.
2. **Define screens**: List the pages, modal dialogs, and screen flows needed for the application.
3. **Plan navigation**: Specify `AppShell` routes, flyout or tab navigation, and transition behavior.
4. **Design MVVM architecture**: Assign views to view models and define service responsibilities.
5. **Map services and models**: Define API clients, data services, sync flows, and domain models.
6. **Include platform concerns**: Note any platform-specific behavior, permissions, or device-specific features.
7. **Create the plan document**: Produce a markdown plan summarizing the architecture, folder layout, and implementation checklist.

## Output
- A mobile architecture plan with screen flow diagrams, MVVM folder structure, service and API mapping, and platform considerations.
- A clear implementation checklist for the `.NET MAUI` developer subagent.
