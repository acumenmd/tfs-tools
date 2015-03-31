using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using TFSHelperLibary;

namespace TFSBuildDefinitionHelper.ViewModel
{
	public class FolderMappingViewModel : ViewModelBase
	{
		private bool _includeInUpdate;
		public bool IncludeInUpdate
		{
			get { return this._includeInUpdate; }
			set { this.Set(ref this._includeInUpdate, value, "IncludeInUpdate"); }
		}

		private IWorkspaceMapping _workspaceMapping;
		public IWorkspaceMapping WorkspaceMapping
		{
			get { return _workspaceMapping; }
			set { Set(ref _workspaceMapping, value, "WorkspaceMapping"); }
		}

		private string _newServerItem;
		public string NewServerItem
		{
			get { return _newServerItem; }
			set { Set(ref _newServerItem, value, "NewServerItem"); }
		}

		private string _currentBranch;
		public string CurrentBranch
		{
			get { return this._currentBranch; }
			set { this.Set(ref this._currentBranch, value, "CurrentBranch"); }
		}

		private BranchObject _newBranch;
		public BranchObject NewBranch
		{
			get { return this._newBranch; }
			set
			{
				if (this.Set(ref this._newBranch, value, "NewBranch") && !String.IsNullOrEmpty(CurrentBranch))
				{
					NewServerItem = WorkspaceMapping.ServerItem.Replace(CurrentBranch, value.BranchPath());
				}
			}
		}

	}
}
