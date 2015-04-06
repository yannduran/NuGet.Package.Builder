﻿using Newtonsoft.Json;
using System.IO;

namespace NuGet.Package.Builder
{
	public class PackageOptions
	{
		public bool UseNuspecFileOnly { get; set; }
		public bool Symbols { get; set; }
		public bool IncludeReferencedProjects { get; set; }
		public bool NoDefaultExcludes { get; set; }
		public string Verbosity { get; set; }
		public string AdditionalProperties { get; set; }

		public PushPackageOptions Publish { get; set; }

		public PackageOptions()
		{
			UseNuspecFileOnly = false;
			Symbols = true;
			IncludeReferencedProjects = true;
			NoDefaultExcludes = true;
			Verbosity = "Detailed";
			Publish = new PushPackageOptions();
		}

		public static PackageOptions LoadOrDefault(ArgumentOptions arguments)
		{
			var file = Path.Combine(arguments.WorkingDirectory, "package.builder.json");
			var options = (File.Exists(file))
				? JsonConvert.DeserializeObject<PackageOptions>(File.ReadAllText(file))
				: new PackageOptions();

			if (arguments.ForcePublishing)
				options.Publish.PublishOnBuild = true;

			return options;
		}

		private string GetProjectOrNuspecFile(ArgumentOptions arguments)
		{
			return Path.Combine(arguments.WorkingDirectory, arguments.TargetName + ((UseNuspecFileOnly) ? ".nuspec" : arguments.ProjectExt));
		}

		private string GetVerbosity()
		{
			return string.IsNullOrWhiteSpace(Verbosity)
				? ""
				: string.Format("-Verbosity {0}", Verbosity);
		}

		private string GetProperties(ArgumentOptions arguments)
		{
			return string.Format("-Properties \"OutDir={0};{1}{2}\"",
				arguments.OutDir,
				arguments.Properties,
				string.IsNullOrWhiteSpace(AdditionalProperties) ? "" : ";" + AdditionalProperties
			);
		}

		private string GetOutputDirectory(ArgumentOptions arguments)
		{
			return string.Format("-OutputDirectory \"{0}\"", arguments.OutDir);
		}

		private string GetBasePath(ArgumentOptions arguments)
		{
			return string.Format("-basepath \"{0}\"", arguments.OutDir);
		}

		public string GetBuildCommandArgs(ArgumentOptions arguments)
		{
			return string.Format(@"pack ""{0}"" {1} {2} {3} {4} -NonInteractive {5} {6} {7}",
				GetProjectOrNuspecFile(arguments),
				Symbols ? "-Symbols" : "",
				NoDefaultExcludes ? "-NoDefaultExcludes" : "",
				GetVerbosity(),
				GetProperties(arguments),
				GetOutputDirectory(arguments),
				GetBasePath(arguments),
				IncludeReferencedProjects ? "-IncludeReferencedProjects" : ""
			);
		}

		private string GetPackagesToPush(ArgumentOptions arguments)
		{
			return string.Format("{0}\\{1}.*.nupkg", arguments.OutDir, arguments.TargetName);
		}

		private string GetPackageSource(ArgumentOptions arguments)
		{
			return string.Format("-s {0}", arguments.OverrideSource == null ? Publish.Source : arguments.OverrideSource);
		}

		private string GetApiKey(ArgumentOptions arguments)
		{
			return arguments.OverrideApiKey == null 
				? Publish.ApiKey 
				: arguments.OverrideApiKey;
		}

		public string GetTimeout()
		{
			return string.Format("-Timeout {0}", Publish.Timeout);
		}

		public string GetPushCommandArgs(ArgumentOptions arguments)
		{
			//push foo.nupkg 4003d786-cc37-4004-bfdf-c4f3e8ef9b3a -s http://customsource/
			return string.Format(@"push ""{0}"" {1} {2} {3} {4} -NonInteractive",
				GetPackagesToPush(arguments),
				GetApiKey(arguments),
				GetPackageSource(arguments),
				GetTimeout(),
				GetVerbosity()
			);
		}
	}

}