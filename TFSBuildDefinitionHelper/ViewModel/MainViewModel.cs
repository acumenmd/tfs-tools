using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TFSHelperLibary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using TFSBuildDefinitionHelper.Properties;

namespace TFSBuildDefinitionHelper.ViewModel
{
	/// <summary>
	/// This class contains properties that the main View can data bind to.
	/// <para>
	/// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
	/// </para>
	/// <para>
	/// You can also use Blend to data bind with the tool's support.
	/// </para>
	/// <para>
	/// See http://www.galasoft.ch/mvvm
	/// </para>
	/// </summary>
	public class MainViewModel : ViewModelBase
	{

		private BranchObject _selectedBranch;
		private IBuildDefinition _selectedBuildDefinition;

		public IBuildDefinition SelectedBuildDefinition
		{
			get { return _selectedBuildDefinition; }
			set
			{
				if (Set(ref _selectedBuildDefinition, value, "SelectedBuildDefinition"))
				{
					UpdateWorkingFolders();
					UpdateBuildSolutions();
					UpdateSelectedBranch();
				}
			}
		}
		public BranchObject SelectedBranch
		{
			get { return _selectedBranch; }
			set
			{
				Set(ref _selectedBranch, value, "SelectedBranch");

				WorkingFolders.ToList().ForEach(folder => folder.NewBranch = value);
				ProjectsToBuild.ToList().ForEach(proj => proj.NewBranch = value);
			}
		}

		public ObservableCollection<IBuildDefinition> AvailibleBuildDefinitions
		{
			get;
			private set;
		}
		public ObservableCollection<BranchObject> AvailibleBranches
		{
			get;
			private set;
		}
		public ObservableCollection<FolderMappingViewModel> WorkingFolders
		{
			get;
			private set;
		}
		public ObservableCollection<ProjectMappingViewModel> ProjectsToBuild
		{
			get;
			private set;
		}

		public RelayCommand SettingsCommand
		{
			get;
			private set;
		}
		public RelayCommand UpdateMappingsCommand
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel()
		{
			AvailibleBranches = new ObservableCollection<BranchObject>();
			AvailibleBuildDefinitions = new ObservableCollection<IBuildDefinition>();
			WorkingFolders = new ObservableCollection<FolderMappingViewModel>();
			ProjectsToBuild = new ObservableCollection<ProjectMappingViewModel>();

			UpdateMappingsCommand = new RelayCommand(ApplyMappings);
			// Register the settings command with a callback for when the settings are updated
			SettingsCommand = new RelayCommand(() => Messenger.Default.Send(
					new NotificationMessageAction<bool>(MessengerTokens.OpenSettingsToken, OnSettingsUpdated),
					MessengerTokens.OpenSettingsToken));

			try
			{
				InitalizeMainViewModel();
			}
			catch (Exception ex)
			{
				// If the Initalization fails, open the settings view model.
				SettingsCommand.Execute(ex);
			}

		}

		private void InitalizeMainViewModel()
		{
			TfsHelper.SetTfsSettings(new TfsConnectionSettings()
			{
				UserName = Settings.Default.Username,
				Password = Settings.Default.Password,
				TfsUri = Settings.Default.TfsUri,
			});


			var _previousBuild = SelectedBuildDefinition;
			var _previousBranch = SelectedBranch;

			AvailibleBuildDefinitions.Clear();
			AvailibleBranches.Clear();

			// Populate the list of Branches and Build Definitions
			TfsHelper.GetBuildDefinitions(Settings.Default.ProjectName).ForEach(b => AvailibleBuildDefinitions.Add(b));
			TfsHelper.GetBranches().ForEach(b => AvailibleBranches.Add(b));

			// Set the default branch selection
			SelectedBranch = AvailibleBranches.FirstOrDefault();
			if (_previousBuild == null)
			{
				SelectedBuildDefinition = AvailibleBuildDefinitions.FirstOrDefault();
			}
			else
			{
				SelectedBuildDefinition = AvailibleBuildDefinitions.FirstOrDefault(b => b.Name == _previousBuild.Name);
			}
		}

		private void OnSettingsUpdated(bool updated)
		{
			if (!updated)
				return;

			try
			{
				InitalizeMainViewModel();
			}
			catch (Exception ex)
			{
				// ignored
			}
		}

		private void UpdateWorkingFolders()
		{
			WorkingFolders.Clear();
			if (SelectedBuildDefinition == null || SelectedBranch == null)
				return;

			// Populate the list of WorkspaceMapping ViewModels
			foreach (var workspaceMapping in SelectedBuildDefinition.Workspace.Mappings)
			{
				WorkingFolders.Add(new FolderMappingViewModel
				{
					WorkspaceMapping = workspaceMapping,
					NewServerItem = workspaceMapping.ServerItem,
					CurrentBranch = AvailibleBranches.FirstOrDefault(branch =>
						workspaceMapping.ServerItem.StartsWith(branch.BranchPath())).BranchPath(),
				});
			}

		}

		private void UpdateBuildSolutions()
		{
			ProjectsToBuild.Clear();
			if (SelectedBuildDefinition == null || SelectedBranch == null)
				return;

			// Populate the list of ProjectMappingViewModels
			foreach (var projPath in TfsHelper.GetProjectsToBuildPaths(SelectedBuildDefinition))
			{
				ProjectsToBuild.Add(new ProjectMappingViewModel()
				{
					IncludeInUpdate = true,
					OriginalPath = projPath,
					CurrentBranch = AvailibleBranches.FirstOrDefault(branch =>
						projPath.StartsWith(branch.BranchPath())).BranchPath(),

				});
			}


		}

		private void UpdateSelectedBranch()
		{
			// use the first working folder since that should be the primary mapping
			var defaultBranch = WorkingFolders.FirstOrDefault();

			if (defaultBranch != null)
			{
				var matchingBranch = AvailibleBranches.FirstOrDefault(b =>
					b.Properties.RootItem.Item == defaultBranch.WorkspaceMapping.ServerItem);

				if (matchingBranch != null)
				{
					SelectedBranch = matchingBranch;
					// Only include it if it matches our current branch selection
					WorkingFolders.ToList().ForEach(fm =>
						fm.IncludeInUpdate = fm.CurrentBranch == SelectedBranch.BranchPath());
				}
			}
		}

		private void ApplyMappings()
		{
			var completeMessage = String.Format(
				"Build Definition {0} updated to use the branch {1}",
				SelectedBuildDefinition.Name,
				SelectedBranch.BranchIdentifier()
				);

			Messenger.Default.Send(new NotificationMessage("Starting Build Definition Update"), MessengerTokens.StartingBuildDefinitionUpdate);

			try
			{
				foreach (var fm in WorkingFolders)
				{
					if (fm.IncludeInUpdate)
						fm.WorkspaceMapping.ServerItem = fm.NewServerItem;
				}

				TfsHelper.UpdateProjectToBuildPath(
					SelectedBuildDefinition,
					ProjectsToBuild.Select(p => p.IncludeInUpdate ? p.NewPath : p.OriginalPath).ToList());

				SelectedBuildDefinition.Save();

				InitalizeMainViewModel();

				Messenger.Default.Send(new NotificationMessage<bool>(true, completeMessage), MessengerTokens.FinishedBuildDefinitionUpdate);
			}
			catch (Exception ex)
			{
				Messenger.Default.Send(new NotificationMessage<bool>(false, ex.Message), MessengerTokens.FinishedBuildDefinitionUpdate);
			}
		}
	}
}