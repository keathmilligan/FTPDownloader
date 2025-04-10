﻿using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FTPDownloader
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        // create the root frame
        private Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            return rootFrame;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = CreateRootFrame();
            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        // handle URI activation
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                var uri = eventArgs.Uri.AbsoluteUri;
                bool secure = eventArgs.Uri.Scheme == "ftps";
                var port = eventArgs.Uri.Port;
                if (port == -1)
                {
                    port = 21;
                }
                var host = eventArgs.Uri.Host;
                var path = eventArgs.Uri.AbsolutePath;
                var userInfo = eventArgs.Uri.UserInfo.Split(':');
                var username = "anonymous";
                string password = null;
                if (userInfo.Length > 0 && userInfo[0].Length > 0)
                {
                    username = userInfo[0];
                    if (userInfo.Length > 1 && userInfo[1].Length > 0)
                    {
                        password = userInfo[1];
                    }
                }
                var appDir = ApplicationData.Current.LocalFolder;
                var appName = Package.Current.DisplayName;
                var filename = eventArgs.Uri.Segments.Last();
                var tempPath = appDir.Path + "\\" + filename;
                var targetFile = await DownloadsFolder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
                Frame rootFrame = CreateRootFrame();
                rootFrame.Navigate(typeof(MainPage), args);
                Window.Current.Activate();

                var downloadsPath = UserDataPaths.GetDefault().Downloads + "\\" + appName + "\\" + filename;
                MainPage.Instance.SetDownloadInfo(uri, downloadsPath);
                MainPage.Instance.SetDownloadStatus("Downloading...");
                try
                {
                    FtpClient client = new FtpClient(host, port, username, password);
                    client.Connect();
                    Progress<FtpProgress> progress = new Progress<FtpProgress>(p => {
                        MainPage.Instance.UpdateProgress(p.Progress);
                    });
                    var cancellationToken = new CancellationToken();
                    FtpStatus result = await client.DownloadFileAsync(tempPath, path, FtpLocalExists.Overwrite, FtpVerify.None, progress, cancellationToken);
                    if (result == FtpStatus.Success)
                    {
                        MainPage.Instance.SetDownloadStatus("Complete");
                        var file = await appDir.GetFileAsync(filename);
                        await file.MoveAndReplaceAsync(targetFile);
                    }
                    else if (result == FtpStatus.Skipped)
                    {
                        MainPage.Instance.SetDownloadStatus("Skipped");
                    }
                    else
                    {
                        MainPage.Instance.SetDownloadStatus("Error");
                    }
                }
                catch (FtpAuthenticationException e)
                {
                    MainPage.Instance.SetDownloadStatus("Authentication error");
                }
                //CoreApplication.Exit();
            }
        }
    }
}
