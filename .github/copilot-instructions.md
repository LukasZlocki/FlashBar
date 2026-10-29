# FlashBar Agent Guide

## Project Purpose
FlashBar is a desktop launcher. Pressing `Win + Space` shows a search bar. Typing an application name and confirming runs that application.

## Technology Stack
- UI: Avalonia (desktop, Win32 backend with Skia rendering)
- Language/runtime: C# on .NET 10
- Target: Windows x64
- Distribution: Native AOT publish (`PublishAot=true`)

## Architecture: Clean Architecture
Keep the UI layer as thin as possible. Business logic belongs in inner layers.

| Layer | Responsibility | Depends on |
|---|---|---|
| Domain | Entities, value objects, domain rules (e.g. `ApplicationEntry`, `ApplicationSearch` matching rules) | Nothing |
| Repository | Repository and launcher contracts over domain types (`IApplicationRepository`, `IApplicationLauncher`) | Domain |
| Service | Application use cases (e.g. search apps, launch app, handle hotkey request) | Domain, Repository |
| Infrastructure | Repository implementations and platform adapters (JSON app config, process start, global hotkey registration on Windows) | Domain, Repository |
| `FlashBar.UI` | Avalonia views, view models, entry point, and composition root | Service, Infrastructure |

Rules:
- Views contain no business logic. Code-behind only handles UI-specific wiring.
- View models call use cases through interfaces; they do not call OS APIs directly.
- Domain, Repository, and Service must not reference Avalonia or platform APIs.
- Repository contracts are defined in `FlashBar.Repository`; their implementations and platform-specific code live in Infrastructure.
- Composition root (dependency injection setup) lives in the entry project only.

## Suggested Solution Layout
```
src/
  FlashBar.Domain/
  FlashBar.Repository/
  FlashBar.Service/
  FlashBar.Infrastructure/
  FlashBar.UI/             (Avalonia UI, entry point, composition root)
  FlashBar.Tests/
```

## Native AOT Constraints
- Avoid runtime reflection, `Activator.CreateInstance` on dynamic types, and dynamic code generation.
- Prefer source generators and compiled bindings (Avalonia `x:DataType`).
- Mark JSON serialization with source-generated contexts (`JsonSerializerContext`).
- Keep trimming warnings at zero. Verify with `dotnet publish -r win-x64 -p:PublishAot=true`.
- Avoid libraries that are not AOT-compatible.

## Coding Conventions
- Use nullable reference types and treat warnings as errors where practical.
- Use `async`/`await` for I/O such as app discovery and process launch.
- Keep public APIs of each layer small and interface-based.
- Write unit tests for Domain and Service logic without UI or OS dependencies.
- Do not add comments that restate code. Comment only what code cannot express.

## Working Rules for Agents
- Make incremental changes; keep each change buildable.
- Confirm the build and AOT publish still succeed after structural changes.
- Do not add dependencies to the Domain, Repository, or Service layers beyond project references and the base library.
- Do not create extra documentation files unless requested.
- Update `.github/copilot-instructions.md` when architecture decisions change.

## Open Questions
- Global hotkey (`Win + Space`) implementation on Windows (not yet implemented).
- Automatic application discovery beyond Start Menu shortcuts. Currently a JSON config file (`apps.json`) is used; Start Menu `.lnk` search is implemented in `WindowsApplicationDiscoveryService`.
