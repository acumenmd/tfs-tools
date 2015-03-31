using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Microsoft.TeamFoundation.VersionControl.Client;
using TFSHelperLibary;

namespace TFSBuildDefinitionHelper.ViewModel
{
	public class ProjectMappingViewModel : ViewModelBase
	{

		private bool _includeInUpdate;
		public bool IncludeInUpdate
		{
			get { return this._includeInUpdate; }
			set { this.Set(ref this._includeInUpdate, value, "IncludeInUpdate"); }
		}

		private string _originalPath;
		public string OriginalPath
		{
			get { return _originalPath; }
			set { Set(ref _originalPath, value, "OriginalPath"); }
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
					NewPath = OriginalPath.Replace(CurrentBranch, value.BranchPath());
					//NewPath =
					//	IncludeInUpdate
					//	? OriginalPath.Replace(CurrentBranch, value.BranchPath())
					//	: NewPath;
				}
			}
		}

		private string _newPath;
		public string NewPath
		{
			get { return _newPath; }
			set { Set(ref _newPath, value, "NewPath"); }
		}


	}
}
