# Settings Store Explorer
A Visual Studio Extension with a tool window that allows you to view and edit the contents of Visual Studio's Settings Store.

The "Settings Store" is like the registry -- in fact, behind the scenes, it's imported and managed as a registry hive.

In earlier versions of Visual Studio, you could use Regedit to view/edit the settings store. However, that's no longer possible (since Dev14, I think) since Visual Studio supports side-by-side installation with independent settings.

## Getting Started
Download the extension from the Visual Studio Marketplace.

Once installed, you can find the "Setting Store Explorer" command on the View/Other Windows menu.

The Settings Store Explorer tool window is divided into two. The left side shows a hierarchy (tree) of collections in the settings store. The right side shows the set of properties for the collection that's selected in the hierarchy.
