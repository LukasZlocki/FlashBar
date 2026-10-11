# FlashBar Agent Guide

## Project Purpose
FlashBar is a desktop launcher. Pressing `Win + Space` shows a search bar. Typing an application name and confirming runs that application.

## Technology Stack
- UI: Avalonia (desktop)
- Language/runtime: C# on .NET 10
- Targets: Windows and Linux
- Distribution: Native AOT publish (`PublishAot=true`) for both platforms

## Architecture: Clean Architecture
Keep the UI layer as thin as possible. Business logic belongs in inner layers.

| Layer | Responsibility | Depends on |
|---|---|---|
| Domain | Entities, value objects, domain rules (e.g. `Application`, `SearchQuery`, matching rules) | Nothing |
| Application | Use cases and service interfaces (e.g. search apps, launch app, handle hotkey request) | Domain |
| Infrastructure | Repositories and platform adapters (app discovery, process start, global hotkey registration per OS) | Application, Domain |
| Presentation (UI) | Avalonia views, view models that only map user input to use cases and expose state | Application |

Rules:
- Views contain no business logic. Code-behind only handles UI-specific wiring.
- View models call use cases through interfaces; they do not call OS APIs directly.
- Domain and Application must not reference Avalonia or platform APIs.
- Platform-specific code (Windows hotkey, Linux hotkey, process launching) lives in Infrastructure behind interfaces defined in Application.
- Composition root (dependency injection setup) lives in the entry project only.

## Suggested Solution Layout
```
src/
  FlashBar.Domain/
  FlashBar.Application/
  FlashBar.Infrastructure/
  FlashBar.Presentation/   (Avalonia UI)
  FlashBar.Desktop/        (entry point, composition root)
  FlashBar.Tests/
```

## Native AOT Constraints
- Avoid runtime reflection, `Activator.CreateInstance` on dynamic types, and dynamic code generation.
- Prefer source generators and compiled bindings (Avalonia `x:DataType`).
- Mark JSON serialization with source-generated contexts (`JsonSerializerContext`).
- Keep trimming warnings at zero. Verify with `dotnet publish -r win-x64 -p:PublishAot=true` and `dotnet publish -r linux-x64 -p:PublishAot=true`.
- Avoid libraries that are not AOT-compatible.

## Coding Conventions
- Use nullable reference types and treat warnings as errors where practical.
- Use `async`/`await` for I/O such as app discovery and process launch.
- Keep public APIs of each layer small and interface-based.
- Write unit tests for Domain and Application logic without UI or OS dependencies.
- Do not add comments that restate code. Comment only what code cannot express.

## Working Rules for Agents
- Make incremental changes; keep each change buildable.
- Confirm the build and AOT publish still succeed after structural changes.
- Do not add dependencies to the Domain or Application layers beyond the base library.
- Do not create extra documentation files unless requested.
- Update `.github/copilot-instructions.md` when architecture decisions change.

## Open Questions
- Linux hotkey strategy (X11 vs Wayland, desktop environment specifics).
- Application discovery source per OS (Start Menu / .desktop files / PATH).
- Matching algorithm for app names (prefix, fuzzy, or both).
