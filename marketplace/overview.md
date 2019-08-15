The Settings Store Explorer extension adds a tool window for viewing and editing the Visual Studio Settings Store. It's like the Windows Registry Editor, but for Visual Studio's internal settings.

## Getting Started
Access the Settings Store Explorer from the View menu. It's under "View > Other Windows >Settings Store Explorer":
<br/>![View Menu](images/ViewMenu.png)

Or you can find it in "Quick search":
<br/>![Quick Search](images/QuickSearch.png)

The Tool Window has two panels. The left hand panel shows two trees: Config and User. Each tree is a hierarchical collection of sub-collections and properties. The properties are shown in the right hand panel.
<br/>![Settings Store Tool Window](images/SettingsStoreToolWindow.png)

You can edit a property of the "User" tree by double-clicking it in the right-hand pane. Note that you cannot edit values or collections under the "Config" tree unless you launch Visual Studio as an Administrator.

## Support
If you find a bug in this extension or have a feature request, please visit https://github.com/pharring/SettingsStoreView to file an issue.
