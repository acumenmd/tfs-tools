using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using TFSHelperLibary;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using TFSBuildDefinitionHelper.ViewModel;
using TFSBuildDefinitionHelper.Views;

namespace TFSBuildDefinitionHelper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			Messenger.Default.Register<NotificationMessage>(this, MessengerTokens.StartingBuildDefinitionUpdate, StartingUpdate);
			Messenger.Default.Register<NotificationMessage>(this, MessengerTokens.UnhandledError, OnUnhandledError);
			Messenger.Default.Register<NotificationMessage<bool>>(this, MessengerTokens.FinishedBuildDefinitionUpdate, UpdateFinished);


			//Messenger.Default.Register<NotificationMessageAction<bool>>(this, MessengerTokens.OpenSettingsToken, OpenSettings);
		}

		private void StartingUpdate(NotificationMessage message)
		{
		}

		private async void UpdateFinished(NotificationMessage<bool> message)
		{
			await this.ShowMessageAsync(
				message.Content
					? "Update Complete"
					: "Update Failed",
				message.Notification);
		}
		private async void OnUnhandledError(NotificationMessage message)
		{
			await this.ShowMessageAsync(
				"An Error Occured",
				message.Notification);
		}

		private void OpenSettings(NotificationMessageAction<bool> message)
		{
			var view = new SettingsView();

			// Attach the handler for when we are finished with the settings.
			var vm = (SettingsViewModel)view.DataContext;

			var dialog = new CustomDialog()
			{
				Content = view,
			};

			vm.FinishedUpdatingEventHandler = (sender, settingsSaved) =>
			{
				message.Execute(settingsSaved);
				// dispose of the settings window instance
				this.HideMetroDialogAsync(dialog);
			};

			this.ShowMetroDialogAsync(dialog);

		}

	}
}
