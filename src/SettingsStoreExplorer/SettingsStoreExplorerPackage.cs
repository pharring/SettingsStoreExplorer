// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace SettingsStoreExplorer
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(c_packageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SettingsStoreExplorerToolWindow), Style = VsDockStyle.Tabbed, DockedWidth = 600, Window = "DocumentWell", Orientation = ToolWindowOrientation.Left)]
    [ProvideKeyBindingTable(SettingsStoreExplorerToolWindow.c_toolWindowGuidString, 113)]
    public sealed class SettingsStoreExplorerPackage : AsyncPackage
    {
        /// <summary>
        /// SettingsStoreExplorerPackage GUID string.
        /// </summary>
        private const string c_packageGuidString = "e8762000-5824-4411-bc19-417b39b309f5";

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsStoreExplorerPackage"/> class.
        /// </summary>
        public SettingsStoreExplorerPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region AsyncPackage Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var shellVersion = await GetShellVersionAsync(cancellationToken);
            var initializeTelemetry = Telemetry.CreateInitializeTelemetryItem(nameof(SettingsStoreExplorerPackage) + "." + nameof(InitializeAsync));
            initializeTelemetry.Properties.Add("VSVersion", shellVersion);
            Telemetry.Client.TrackEvent(initializeTelemetry);

            await SettingsStoreExplorerToolWindowCommand.InitializeAsync(this);
        }

        private async Task<string> GetShellVersionAsync(CancellationToken cancellationToken)
        {
            if (await GetServiceAsync(typeof(SVsShell)) is IVsShell shell)
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                if (ErrorHandler.Succeeded(shell.GetProperty((int)__VSSPROPID5.VSSPROPID_ReleaseVersion, out var obj)) && obj != null)
                {
                    return obj.ToString();
                }
            }

            return "Unknown";
        }

        #endregion
    }
}
