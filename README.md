[English](README.md) | [日本語](README.ja.md)

# WPF Impact Checklist for Migrating from .NET 6 to .NET 8

> Scope:
> - WPF / Desktop apps
> - .NET 6 → .NET 8 (i.e., you’re taking both .NET 7 and .NET 8 changes at once)
>
> Priority legend:
> - **A** = Likely to cause real issues; must verify
> - **B** = Conditional impact; verify if applicable
> - **C** = Usually safe; quick sanity check is enough

---

## Impact Areas (in Priority Order)

| Priority | Area | What can happen? (Typical symptoms) | When it’s likely to apply | Fastest way to verify | Mitigation / Direction |
|---|---|---|---|---|---|
| **A** | **UI Automation / UI testing / Accessibility** | UIA tree changes; elements can’t be found; automated UI tests fail | Using FlaUI / WinAppDriver / Appium / Accessibility Insights | Compare the same screen in Accessibility Insights between .NET 6 and .NET 8 builds | Explicitly set `AutomationProperties.*`; adjust AutomationPeers if needed |
| **A** | **WindowsFormsHost + Fluent/Theme** | WindowsFormsHost turns black, rendering glitches, incorrect colors | Embedding WinForms controls inside WPF | Visual check on target screens with Fluent theme ON/OFF | Work around theme issues, isolate host, update components |
| **A** | **Build environment (VS / MSBuild / SDK)** | Local builds succeed but CI fails; builds fail on some machines | CI or some PCs use older Build Tools | Check `dotnet --info` and Build Tools versions on CI | Update Build Tools, pin SDK, update CI images |
| **B** | **TextBox / RichTextBox Drag & Drop (from .NET 7)** | Custom DnD code breaks due to DataObject format assumptions | You hook TextBox/RichTextBox DnD and parse the DataObject | Test DnD between text boxes on screens with custom handlers | Stop relying on a single format; accept multiple formats |
| **B** | **Third-party WPF UI libraries** | Theme/input/automation/virtualization details change | Using WPF-UI / MahApps / Telerik, etc. | Regression test key screens | Update libraries, apply known workarounds, adjust styles |
| **B** | **MSIX / Store distribution pipeline** | Issues in publish/signing/artifacts | You build MSIX and ship via Microsoft Store | Run `dotnet publish` + MSIX generation both locally and on CI | Update packaging project/tools; standardize the pipeline |
| **B** | **Single-file / Trim (only if used)** | App fails to start; XAML load failures | You enable single-file or trimming | Build with the target publish settings and run | Trimming is generally not recommended for WPF; use exceptions if unavoidable |
| **C** | **General WPF runtime behavior** | Feels slightly smoother; mostly minor differences | Typical WPF apps | Quick regression: scroll/selection/editing | Fix only where issues appear |
| **C** | **Performance / GC / JIT improvements** | Fewer stalls/hitches under load | Large datasets / virtualization scenarios | Same workload: quick “feel” test + basic timing | Don’t assume improvements everywhere; side effects are rare |

---

## Top 3 Checks for FastExplorer / Similar Apps

1. **Do you use UI Automation (UI tests / accessibility tooling)?**
2. **Do you use WindowsFormsHost?**
3. **Do you have CI or multiple build machines (Build Tools version drift)?**

---

## Conclusion

- **Officially, .NET 8 lists 0 WPF-specific breaking changes**
- In practice, the main risk areas are:  
  **UIA / WindowsFormsHost / build environment**
- WPF itself is generally **very stable from .NET 6 → .NET 8**

👉 For new development and core libraries, **.NET 8 LTS is recommended**
