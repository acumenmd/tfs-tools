using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using TFSBuildDefinitionHelper.Properties;
using TFSHelperLibary;

namespace TFSBuildDefinitionHelper.ViewModel
{
	public class SettingsViewModel : ViewModelBase
	{
		private string _userName;
		public string UserName
		{
			get { return _userName; }
			set
			{
				if (Set(ref _userName, value))
				{
					Settings.Default.Username = UserName;
				}
			}
		}

		private string _password;
		public string Password
		{
			get { return _password; }
			set
			{
				if (Set(ref _password, value))
				{
					Settings.Default.Password = Password;
				}
			}
		}

		private string _tfsUri;
		public string TfsUri
		{
			get { return _tfsUri; }
			set
			{
				if (Set(ref _tfsUri, value))
				{
					Settings.Default.TfsUri = TfsUri;
					RaisePropertyChanged();
				}
			}
		}

		private string _selectedProject;
		public string SelectedProject
		{
			get { return _selectedProject; }
			set
			{
				if (Set(ref _selectedProject, value))
				{
					Settings.Default.ProjectName = SelectedProject;
				}
			}
		}

		public ObservableCollection<string> ProjectList
		{
			get;
			private set;
		}

		public SettingsViewModel()
		{
			// Property Initialization
			LoadSettingsValues();
			ProjectList = new ObservableCollection<string>();

			// Command Initialization
			SaveSettingsCommand = new RelayCommand(
				() =>
				{
					Settings.Default.Save();
					OnFinishedUpdatingRequest(true);
				},
				() => !String.IsNullOrWhiteSpace(SelectedProject));
			CancelChangesCommand = new RelayCommand(
				() =>
				{
					Settings.Default.Reload();
					LoadSettingsValues();
					OnFinishedUpdatingRequest(false);
				});
			TestConnectionCommand = new RelayCommand(
				TestConnectionCommandExecute,
				TestConnectionCommandCanExecute);

			// Data Initialization
			if (TestConnectionCommand.CanExecute(null))
			{
				TestConnectionCommand.Execute(null);
				SelectedProject = ProjectList.FirstOrDefault(p => p == Settings.Default.ProjectName);
			}
		}

		private void LoadSettingsValues()
		{
			UserName = Settings.Default.Username;
			Password = Settings.Default.Password;
			TfsUri = Settings.Default.TfsUri;
		}

		public RelayCommand TestConnectionCommand
		{
			get;
			private set;
		}
		public RelayCommand SaveSettingsCommand
		{
			get;
			private set;
		}
		public RelayCommand CancelChangesCommand
		{
			get;
			private set;
		}

		public EventHandler<bool> FinishedUpdatingEventHandler;
		protected void OnFinishedUpdatingRequest(bool saveChanges)
		{
			if (this.FinishedUpdatingEventHandler != null)
			{
				this.FinishedUpdatingEventHandler(this, saveChanges);
			}
		}

		private bool TestConnectionCommandCanExecute()
		{
			Debug.WriteLine("TestConnectionCommandCanExecute");
			return
				!String.IsNullOrWhiteSpace(TfsUri) &&
				!String.IsNullOrWhiteSpace(UserName) &&
				!String.IsNullOrWhiteSpace(Password);
		}
		private void TestConnectionCommandExecute()
		{
			ProjectList.Clear();

			try
			{
				TfsHelper.SetTfsSettings(new TfsConnectionSettings()
				{
					UserName = UserName,
					Password = Password,
					TfsUri = TfsUri
				});

				if (TfsHelper.CanConnect())
				{
					TfsHelper.GetTeamProjectNames().ToList().ForEach(p => ProjectList.Add(p));
					SelectedProject = ProjectList.FirstOrDefault();
				}
			}
			catch
			{
				// ignored
			}
		}
	}
}
