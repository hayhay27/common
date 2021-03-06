﻿// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common.IO;
using Nuke.Common.Utilities;

namespace Nuke.Common.Git
{
    public enum GitHubItemType
    {
        Automatic,
        File,
        Directory
    }

    [PublicAPI]
    public static class GitRepositoryExtensions
    {
        public static bool IsOnMasterBranch(this GitRepository repository)
        {
            return repository.Branch?.EqualsOrdinalIgnoreCase("master") ?? false;
        }

        public static bool IsOnDevelopBranch(this GitRepository repository)
        {
            return (repository.Branch?.EqualsOrdinalIgnoreCase("dev") ?? false) ||
                   (repository.Branch?.EqualsOrdinalIgnoreCase("develop") ?? false) ||
                   (repository.Branch?.EqualsOrdinalIgnoreCase("development") ?? false);
        }

        public static bool IsOnFeatureBranch(this GitRepository repository)
        {
            return repository.Branch?.StartsWithOrdinalIgnoreCase("feature/") ?? false;
        }

        // public static bool IsOnBugfixBranch(this GitRepository repository)
        // {
        //     return repository.Branch?.StartsWithOrdinalIgnoreCase("feature/fix-") ?? false;
        // }

        public static bool IsOnReleaseBranch(this GitRepository repository)
        {
            return repository.Branch?.StartsWithOrdinalIgnoreCase("release/") ?? false;
        }

        public static bool IsOnHotfixBranch(this GitRepository repository)
        {
            return repository.Branch?.StartsWithOrdinalIgnoreCase("hotfix/") ?? false;
        }

        public static bool IsGitHubRepository(this GitRepository repository)
        {
            return repository != null && repository.Endpoint.EqualsOrdinalIgnoreCase("github.com");
        }

        public static string GetGitHubOwner(this GitRepository repository)
        {
            ControlFlow.Assert(repository.IsGitHubRepository(), "repository.IsGitHubRepository()");

            return repository.Identifier.Split('/')[0];
        }

        public static string GetGitHubName(this GitRepository repository)
        {
            ControlFlow.Assert(repository.IsGitHubRepository(), "repository.IsGitHubRepository()");

            return repository.Identifier.Split('/')[1];
        }

        /// <summary>Url in the form of <c>https://raw.githubusercontent.com/{identifier}/blob/{branch}/{file}</c>.</summary>
        public static string GetGitHubDownloadUrl(this GitRepository repository, string file, string branch = null)
        {
            ControlFlow.Assert(repository.IsGitHubRepository(), "repository.IsGitHubRepository()");

            branch = branch ?? repository.Branch.NotNull("repository.Branch != null");
            var relativePath = GetRepositoryRelativePath(file, repository);
            return $"https://raw.githubusercontent.com/{repository.Identifier}/{branch}/{relativePath}";
        }

        /// <summary>
        /// Url in the form of <c>https://github.com/{identifier}/tree/{branch}/directory</c> or
        /// <c>https://github.com/{identifier}/blob/{branch}/file</c> depending on the item type.
        /// </summary>
        public static string GetGitHubBrowseUrl(
            this GitRepository repository,
            string path = null,
            string branch = null,
            GitHubItemType itemType = GitHubItemType.Automatic)
        {
            ControlFlow.Assert(repository.IsGitHubRepository(), "repository.IsGitHubRepository()");

            branch = branch ?? repository.Branch.NotNull("repository.Branch != null");
            var relativePath = GetRepositoryRelativePath(path, repository);
            var method = GetMethod(relativePath, itemType, repository);
            ControlFlow.Assert(path == null || method != null, "Could not determine item type.");

            return $"https://github.com/{repository.Identifier}/{method}/{branch}/{relativePath}";
        }

        [CanBeNull]
        private static string GetMethod([CanBeNull] string relativePath, GitHubItemType itemType, GitRepository repository)
        {
            var absolutePath = repository.LocalDirectory != null && relativePath != null
                ? PathConstruction.NormalizePath(Path.Combine(repository.LocalDirectory, relativePath))
                : null;

            if (itemType == GitHubItemType.Directory || Directory.Exists(absolutePath) || relativePath == null)
                return "tree";

            if (itemType == GitHubItemType.File || File.Exists(absolutePath))
                return "blob";

            return null;
        }

        [ContractAnnotation("path: null => null; path: notnull => notnull")]
        private static string GetRepositoryRelativePath([CanBeNull] string path, GitRepository repository)
        {
            if (path == null)
                return null;

            if (!Path.IsPathRooted(path))
                return path;

            ControlFlow.Assert(PathConstruction.IsDescendantPath(repository.LocalDirectory.NotNull("repository.LocalDirectory != null"), path),
                $"PathConstruction.IsDescendantPath({repository.LocalDirectory}, {path})");
            return PathConstruction.GetRelativePath(repository.LocalDirectory, path).Replace(oldChar: '\\', newChar: '/');
        }
    }
}
