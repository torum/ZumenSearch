# Copilot Instructions

Strictly follow these boundaries:

## Change Approval
- Do not edit code or change any files in this repository without the user's explicit consent.
- Before making any change, present a plan describing the intended edits and wait for the user's approval.

## General Guidelines
- Follow .github/copilot-instructions.md
- Follow and preserve the existing code, code style, design patterns and conventions strictly when making changes.
- For the current database redesign, implement only the fresh database schema; do not add migration logic. The user will delete the existing database file and local cache.
- Avoid presenting code changes as unified diff/patch blocks; use clear replacement code snippets with line numbers and file names and paths or concise prose instead.
- Maintain the application as a single project for the foreseeable future, enforcing cleaner internal boundaries rather than splitting into multiple projects.

## Code Style

### XAML formatting
- Single line makes code folding easier. Keep each element's opening tag and all its attributes on a single line. Do not wrap an element's attributes across multiple lines.

### C# formatting
- Priority is for humans to understand the code and follow the logic and flow. The maintainability is important but secondary. Follow the existing style, patterns, and conventions. 
- Do not extract methods or helpers unnecessarily. It will become exponentially harder for a human to follow. Extract only if the same code is actually used in multiple places/methods/classes.
- Code folding is very important for readability. Therefore, keep method, constructor, and lambda parameter lists on one line, even when the line is long. Keep argument lists on one line. Do not wrap argument lists. Do not put parameters on separate lines.
- Avoiding hard-coded enum member-name strings in parsers; use generic enum parsing so enum renames do not require updating duplicated string constants.
- For enum choices that need human-readable labels, use a model label type containing the enum Key and localized Label, following PropertyKindTypeLabel, instead of displaying enum member names.
- Follow existing setter conventions: typed-label properties use matching `Set...TypeLabel`/`Set...Label` methods and should have `FromString` counterparts when applicable; preserve the existing parsing pattern.

## WinUI 3 & Windows App SDK Architecture Guidelines (.NET 11)

### Core Ecosystem Rules
- **Target Framework:** Only target `.NET 11.0` with the Windows-specific Target Framework Moniker (e.g., `net11.0-windows10.0.19041.0`).
- **Namespaces:** Never output `Windows.UI.Xaml` (UWP). ALWAYS output `Microsoft.UI.Xaml` (WinUI 3)`.

### Native AOT
- The app is AOT-compatible; code changes must remain compatible with trimming and Native AOT.
- Avoid reflection/dynamic patterns that cannot be statically analyzed.

### Deployment Strategy (Hybrid: Unpackaged & Packaged)
- Acknowledge the app supports unpackaged deployment. When initializing app startup logic outside of a package lifecycle, ensure the Windows App SDK Bootstrapper is explicitly managed or `<WindowsPackageType>None</WindowsPackageType>` configurations are respected.
- **Conditional Guardrails:** When writing file I/O, local settings persistence, or protocol extensions, use conditional checks (`AppInstance.IsPackaged` equivalents or matching preprocessor directives) to gracefully fall back when running unpackaged.

### MVVM Architecture (CommunityToolkit.Mvvm)
- **Source Generators:** Do not write `INotifyPropertyChanged` boilerplate.
- **Properties:** Use `[ObservableProperty]` on private fields (using `camelCase` or `_camelCase`) so the Toolkit generates public `PascalCase` properties when the generated property requires no custom getter or setter logic. For properties that require custom logic, implement them manually and call ` SetProperty(ref field, value)` in the setter instead of using [ObservableProperty].
- **Commands:** Use `[RelayCommand]` on methods instead of creating `ICommand` properties manually.
- Use `ObservableRecipient` or `Messenger` patterns for decoupled View-Model communication.

### Threading & Concurrency
- **UI Thread Access:** If updating the UI from a background thread or asynchronous task, never use `Dispatcher.RunAsync` (UWP legacy). Use `this.DispatcherQueue.TryEnqueue(() => { ... })` or equivalent custom method like IDispatcherService.TryEnqueue.
- **Asynchronous Execution:** Prefer `async` / `await` over synchronous blocks or `.Result`. Ensure async events use `await` properly inside UI lifecycle methods.

### XAML Controls & Styling
- All XAML data bindings in this codebase must use compiled `x:Bind`; do not introduce runtime `{Binding}` expressions.
- Prefer modern WinUI 3 native controls over legacy custom implementations: `InfoBar`, `NumberBox`, `TeachingTip`, `ItemContainer`, and `PipsPager`.
- Utilize **ThemeResources** (e.g., `ThemeResource SystemControlPageBackgroundChromeLowBrush`) for seamless native Dark/Light mode switching instead of hardcoded hex colors.

### Fields
- For fields, prefer typed enums defined in Models over string constants/options declared in XAML.






