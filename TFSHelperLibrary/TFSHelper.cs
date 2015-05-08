using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TFSHelperLibary
{
	public static class TfsHelper
	{
		private static TfsConnectionSettings _settings;
		private static TfsTeamProjectCollection _collection;

		public static void SetTfsSettings(TfsConnectionSettings settings)
		{
			_settings = settings;
			InitalizeTfsCollection();
		}

		public static bool InitalizeTfsCollection()
		{
			try
			{
				_collection = new TfsTeamProjectCollection(
					new Uri(_settings.TfsUri),
					new NetworkCredential(_settings.UserName, _settings.Password)); //, settings.Domain));

				_collection.EnsureAuthenticated();

				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Returns 
		/// </summary>
		public static bool CanConnect()
		{
			return _collection.HasAuthenticated;
		}

		public static IEnumerable<string> GetTeamProjectNames()
		{
			return
				_collection.GetService<VersionControlServer>()
					.GetAllTeamProjects(true)
					.Select(proj => proj.Name).ToList();
		}

		public static List<IBuildDefinition> GetBuildDefinitions(string projectName)
		{
			var buildServer = _collection.GetService<IBuildServer>();
			var buildDefs = buildServer.QueryBuildDefinitions(projectName)
				.OrderBy(d => d.Name);

			return buildDefs.ToList();
		}

		public static List<BranchObject> GetBranches()
		{
			// get liset of branches
			var vc = _collection.GetService<VersionControlServer>();
			var branches = vc.QueryRootBranchObjects(RecursionType.Full)
				//.Where(b => !b.Properties.RootItem.IsDeleted)
				.OrderBy(b => b.Properties.RootItem.Item);
			return branches.ToList();
		}

		//public static List<string> GetProjectsToBuildPaths(IBuildDefinition buildDefinition)
		//{
		//	// Type 1
		//	var projects = new List<string>();
			
		//	XmlDocument xmlDoc = new XmlDocument();
		//	xmlDoc.LoadXml(buildDefinition.ProcessParameters);
		//	var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
		//	namespaceManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");

		//	var projectListNode = xmlDoc.SelectSingleNode(@"//x:Array[@x:Key='ProjectsToBuild']", namespaceManager);
		//	if (projectListNode != null)
		//	{
		//		foreach (XmlNode childNode in projectListNode.ChildNodes)
		//		{
		//			projects.Add(childNode.InnerText);
		//		}
		//	}

		//	return projects;
		//}

		//public static bool UpdateProjectToBuildPath(IBuildDefinition buildDefinition, string oldPath, string newPath)
		//{
		//	//mutableProcess
		//	//WorkflowHelpers.SerializeProcessParameters(buildDefinition.)
		//	var process = WorkflowHelpers.DeserializeProcessParameters(buildDefinition.ProcessParameters);

		//	XmlDocument xmlDoc = new XmlDocument();
		//	xmlDoc.LoadXml(buildDefinition.ProcessParameters);
		//	var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
		//	namespaceManager.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");

		//	var projectListNode = xmlDoc.SelectSingleNode(@"//x:Array[@x:Key='ProjectsToBuild']", namespaceManager);
		//	if (projectListNode != null)
		//	{
		//		foreach (XmlNode childNode in projectListNode.ChildNodes)
		//		{
		//			if (childNode.InnerText == oldPath)
		//			{
		//				childNode.InnerText = newPath;

		//				return true;
		//			}
		//		}
		//	}

		//	return false;
		//}

		public static List<string> GetProjectsToBuildPaths(IBuildDefinition buildDefinition)
		{
			// Type 2
			var process = WorkflowHelpers.DeserializeProcessParameters(buildDefinition.ProcessParameters);
			object proj;
			if (process.TryGetValue("ProjectsToBuild", out proj) && proj is string[])
			{
				return ((string[])proj).ToList();
			}
			return null;
		}

		public static bool UpdateProjectToBuildPath(IBuildDefinition buildDefinition, List<string> projectsToBuild)
		{
			var process = WorkflowHelpers.DeserializeProcessParameters(buildDefinition.ProcessParameters);
			
			if (process.ContainsKey("ProjectsToBuild"))
			{
				process["ProjectsToBuild"] = projectsToBuild.ToArray();

				buildDefinition.ProcessParameters = WorkflowHelpers.SerializeProcessParameters(process);

				return true;
			}

			return false;
		}

		public static string BranchPath(this BranchObject branch)
		{
			if (branch == null)
				return string.Empty;

			return branch.Properties.RootItem.Item;
		}
		public static string BranchIdentifier(this BranchObject branch)
		{

			if (branch == null)
				return string.Empty;

			return branch.BranchPath().Split('/').Last();
		}
	}

	public class TfsConnectionSettings
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string TfsUri { get; set; }

		public string Domain { get; set; }
	}
}
