# FlashBar

FlashBar is a lightweight desktop launcher for Windows and Linux. Press `Win + Space` to open a search bar, type the name of an application, and confirm to launch it.

## Features
- Global hotkey (`Win + Space`) to show the search bar
- Application search by name
- Launch the selected application with a single confirmation

## Technology
- UI: [Avalonia](https://avaloniaui.net/) (desktop)
- Runtime: C# on .NET 10
- Platforms: Windows and Linux
- Distribution: Native AOT (`PublishAot=true`)

## Architecture
FlashBar follows Clean Architecture to keep the UI layer thin:

- `FlashBar.Domain`: entities, value objects, and matching rules
- `FlashBar.Application`: use cases and service interfaces
- `FlashBar.Infrastructure`: application discovery, process launching, and platform-specific hotkey registration
- `FlashBar.Presentation`: Avalonia views and view models
- `FlashBar.Desktop`: entry point and composition root

## Status
Early development. See [.github/copilot-instructions.md](.github/copilot-instructions.md) for the project guide and open questions.
