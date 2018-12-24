// Copyright 2018 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.IO;

namespace Nuke.Common
{
    public class Constants
    {
        public const string ConfigurationFileName = ".nuke";
        
        public const string TargetsSeparator = "+";
        public const string InvokedTargetsParameterName = "Target";
        public const string SkippedTargetsParameterName = "Skip";
        
        public const string CompletionParameterName = "shell-completion";

        [CanBeNull]
        public static PathConstruction.AbsolutePath TryGetRootDirectoryFrom(string startDirectory)
        {
            return (PathConstruction.AbsolutePath) FileSystemTasks.FindParentDirectory(
                startDirectory,
                predicate: x => x.GetFiles(ConfigurationFileName).Any());
        }

        public static PathConstruction.AbsolutePath GetTemporaryDirectory(PathConstruction.AbsolutePath rootDirectory)
        {
            return rootDirectory / ".tmp";
        }

        public static PathConstruction.AbsolutePath GetCompletionFile(PathConstruction.AbsolutePath rootDirectory)
        {
            var completionFileName = CompletionParameterName + ".yml";
            return File.Exists(rootDirectory / completionFileName)
                ? rootDirectory / completionFileName
                : GetTemporaryDirectory(rootDirectory) / completionFileName;
        }
        
        public static PathConstruction.AbsolutePath GetBuildAttemptFile(PathConstruction.AbsolutePath rootDirectory)
        {
            return GetTemporaryDirectory(rootDirectory) / "build-attempt.log";
        }
    }
}