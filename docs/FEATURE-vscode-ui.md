````markdown name=docs/FEATURE-vscode-ui.md
```markdown
# Feature: VS Code style UI and Process Editor

This branch adds a Process Editor and integrates Module Provider data into a ModuleCatalog.

How to build:
1. git fetch && git checkout feature/vscode-ui
2. dotnet restore && dotnet build

How to run:
1. Open solution in Visual Studio and run TangerineAutomationSystem project.
2. Open the Process Editor via the ActivityBar (Flow) or by opening Views/ProcessEditor in MainWindow.

Notes on gRPC:
- PlatformCallGrpcClient is a stub. Replace with real generated gRPC client calls and configure endpoint.

``` 
````