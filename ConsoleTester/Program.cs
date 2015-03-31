using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace ConsoleApplication3
{
    class Program
    {
        static void Main(string[] args)
        {
            var creds = new NetworkCredential()
            {
                Domain = "",
                UserName = "",
                Password = ""
            };

            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(""), creds);
            tfs.EnsureAuthenticated();

            var buildServer = tfs.GetService<IBuildServer>();
            var buildDefs = buildServer.QueryBuildDefinitions("");

            var bd = buildDefs.FirstOrDefault(b => b.Name == "update-test");
            bd.Workspace.Mappings.ForEach(m =>
            {
                m.ServerItem = "";
            });

            var processParams = WorkflowHelpers.DeserializeProcessParameters(bd.ProcessParameters);

            // get liset of branches
            var vcs = tfs.GetService<VersionControlServer>();
            var branches =
                vcs.QueryRootBranchObjects(RecursionType.Full).Where(b => !b.Properties.RootItem.IsDeleted).ToList();
            var branchRoots = branches.Select(b => b.Properties.RootItem.Item).OrderBy(b=>b).ToList();

            

            //bd.Save();


        }
    }
}
