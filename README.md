# FSharp.WebUI

F# binding for [WebUI](https://github.com/webui-dev/webui) - Use any web browser as GUI.

## Installation

`dotnet add package FSharp.WebUI --version 0.0.1`

## Project Structure

```
fsharp-webui/
├── fsharp-webui.fsproj          # Library project
├── src/
│   └── WebUI.fs                 # Main library code
├── examples/
│   ├── simple-example/          # Runnable example
│   │   ├── SimpleExample.fs
│   │   └── simple-example.fsproj
│   └── prettier/                # Pretty CSS buttons example
│       ├── Prettier.fs
│       └── prettier.fsproj
├── tests/
│   └── WebUITests/              # Simple test build
│       ├── Tests.fs
│       └── WebUITests.fsproj
└── lib/
    ├── linux-x64/               # Native WebUI library (Linux)
    │   └── libwebui-2.so
    ├── macos-arm64/             # Native WebUI library (macOS)
    │   └── libwebui-2.dylib
    └── win-x64/                 # Native WebUI library (Windows)
        └── webui-2.dll
```

## Supported Platforms

- Linux x64 (libwebui-2.so)
- macOS arm64 (libwebui-2.dylib)
- Windows x64 (webui-2.dll)

## Build Commands

### Library
```bash
dotnet build fsharp-webui.fsproj
```

### Example Applications

Simple example:
```bash
dotnet run --project examples/simple-example
# or
dotnet build examples/simple-example/simple-example.fsproj
./bin/Debug/net10.0/simple-example
```

Prettier (3x3 pretty buttons) example — default browser: Chromium:
```bash
dotnet run --project examples/prettier
# or
dotnet build examples/prettier/prettier.fsproj
./bin/Debug/net10.0/prettier
```

Note: examples use WebUI.Browser.Chromium by default in their sample code; change to another browser by passing a different Browser enum to WebUI.showBrowser if desired.

### Tests
```bash
dotnet run --project tests/WebUITests
# or
dotnet build tests/WebUITests/WebUITests.fsproj
./bin/Debug/net10.0/WebUITests
```

## Usage Example

```fsharp
open WebUI

let html = """<!DOCTYPE html>
<html>
  <head>
    <script src="webui.js"></script>
  </head>
  <body>
    <button id="myBtn">Click Me</button>
  </body>
</html>"""

[<EntryPoint>]
let main argv =
    let myWindow = WebUI.newWindow()
    WebUI.bind myWindow "myBtn" (WebUIEventHandler(fun _ -> printfn "Clicked!"))
    WebUI.showBrowser myWindow html WebUI.Browser.Chromium |> ignore
    WebUI.wait()
    0
```

## Requirements

- .NET 10.0+
- A web browser (Chrome, Firefox, Edge, Safari, or Chromium)
- Native libraries are included in the NuGet package for:
  - Linux x64
  - macOS arm64
  - Windows x64