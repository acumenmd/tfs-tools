using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using TFSBuildDefinitionHelper.ViewModel;
using TFSHelperLibary;
using TFSBuildDefinitionHelper.Views;

namespace TFSBuildDefinitionHelper
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		// instance of the settings window to make sure it doesn't continually pop up.
		private MetroWindow _settingsWindow;

		protected override void OnStartup(StartupEventArgs e)
		{
			// Run startup code first
			base.OnStartup(e);

			// Get our window messenger handlers
			
			Messenger.Default.Register<NotificationMessageAction<bool>>(this, MessengerTokens.OpenSettingsToken, OpenSettings);
			this.DispatcherUnhandledException += App_DispatcherUnhandledException;
		}

		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			Messenger.Default.Send(new NotificationMessage(sender, e.Exception.Message), MessengerTokens.UnhandledError);
		}

		private void OpenSettings(NotificationMessageAction<bool> message)
		{
			if (_settingsWindow == null)
			{
				var view = new SettingsView();
				_settingsWindow = new MetroWindow()
				{
					Content = view,
					Title = "TFS Connection Settings",
					SizeToContent = SizeToContent.WidthAndHeight,
					ResizeMode = ResizeMode.NoResize,
					ShowIconOnTitleBar = false,
					ShowMinButton = false,
					ShowCloseButton = false,
					ShowMaxRestoreButton = false,
					IsWindowDraggable = false,
				};

				// Determine settings window positioning
				if (MainWindow != null)
				{
					_settingsWindow.Owner = MainWindow;
					_settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				}
				else
				{
					_settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				}

				// Attach the handler for when we are finished with the settings.
				var vm = (SettingsViewModel)view.DataContext;
				vm.FinishedUpdatingEventHandler = (sender, settingsSaved) =>
				{
					message.Execute(settingsSaved);
					// dispose of the settings window instance
					if (_settingsWindow != null)
					{
						_settingsWindow.Close();
						_settingsWindow = null;
					}
				};
			}

			_settingsWindow.ShowDialog();
		}

		private void App_OnStartup(object sender, StartupEventArgs e)
		{

			var mainWindow = new MainWindow();

			//Re-enable normal shutdown mode.
			Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
			Current.MainWindow = mainWindow;
			mainWindow.Show();

		}
	}
}
