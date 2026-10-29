# FlashBar

FlashBar is a lightweight desktop launcher for Windows. Press `Win + Space` to open a search bar, type the name of an application, and confirm to launch it.

## Features
- Global hotkey (`Win + Space`) to show the search bar
- Application search by name (prefix matches first, case-insensitive)
- Applications are configured in a JSON file (`apps.json`) next to the executable
- Launch the selected application with Enter or a double-click

## Configuring applications
Edit `apps.json` (copied next to `FlashBar.UI.exe` at build time):

```json
{
  "applications": [
    { "name": "Notepad", "path": "C:\\Windows\\System32\\notepad.exe", "icon": "icons\\notepad.png" },
    { "name": "Calculator", "path": "C:\\Windows\\System32\\calc.exe" }
  ]
}
```

- `name` (required): label shown in the list and used for search.
- `path` (required): absolute path to the executable or document to launch.
- `icon` (optional): image file shown next to the name. Relative paths are resolved from the folder containing `apps.json`. If omitted, the first letter of the name is shown instead.

The file is re-read on every search, so changes apply without restarting FlashBar. A missing file shows an empty list.

## Technology
- UI: [Avalonia](https://avaloniaui.net/) (desktop)
- Runtime: C# on .NET 10
- Platform: Windows
- Distribution: Native AOT (`PublishAot=true`), `win-x64`

## Architecture
FlashBar follows Clean Architecture to keep the UI layer thin:

- `FlashBar.Domain`: entities (`ApplicationEntry`) and matching rules (`ApplicationSearch`)
- `FlashBar.Repository`: repository and launcher contracts over domain types
- `FlashBar.Service`: application use cases that use repository contracts
- `FlashBar.Infrastructure`: JSON configuration repository, process launching, and platform-specific hotkey registration
- `FlashBar.UI`: Avalonia views, view models, and desktop entry point; composition root belongs here

## Status
Early development. See [.github/copilot-instructions.md](.github/copilot-instructions.md) for the project guide and open questions.
