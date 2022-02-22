using System;
using System.Reflection;

#if MIGRATION

namespace Microsoft.Expression.Media.Effects
{
    internal static class Global
	{
		private static string assemblyShortName;

		private static string AssemblyShortName
		{
			get
			{
				if (assemblyShortName == null)
				{
					Assembly assembly = typeof(Global).Assembly;
					assemblyShortName = assembly.ToString().Split(',')[0];
				}
				return assemblyShortName;
			}
		}

		public static Uri MakePackUri(string relativeFile)
		{
			string uriString = "/" + AssemblyShortName + ";component/" + relativeFile;
			return new Uri(uriString, UriKind.Relative);
		}
	}
}

#endif