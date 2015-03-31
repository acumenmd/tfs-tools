using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSBuildDefinitionHelper.ViewModel
{
	public static class MessengerTokens
	{
		public static readonly string UnhandledError = "UnhandledError";
		public static readonly string OpenSettingsToken = "OpenSettingsToken";
		public static readonly string StartingBuildDefinitionUpdate = "StartingBuildDefinitionUpdate";
		public static readonly string FinishedBuildDefinitionUpdate = "FinishedBuildDefinitionUpdate";
	}
}
