﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;

namespace Bottles.IntegrationTesting
{
    /*
       alias -> Manage folder aliases
  assemblies -> Adds assemblies to a given manifest
assembly-pak -> Bundle up the content and data files for a self contained assembly package
      create -> Create a bottle zip from a package directory
  create-pak -> Create a package file from a package directory
        help -> list all the available commands
        init -> Initialize a bottle manifest
        link -> Links a package folder to an application folder in development mode
     */



    public class IntegrationTestContext
    {

        protected BottleLoadingDomain _domain;

        [SetUp]
        public void SetUp()
        {
            _domain = new BottleLoadingDomain();

            new FileSystem().CleanDirectory("content");

            ResetBottleProjectCode();
        }

        [TearDown]
        public void TearDown()
        {
            _domain.Dispose();
        }

        public static string SolutionDirectory;
        public static string StagingDirectory;
        public static string SourceDirectory;
        public static string ZipsDirectory;
        private static readonly string BottleRunnerFile;

        static IntegrationTestContext()
        {
            var dir = Path.GetDirectoryName(typeof(IntegrationTestContext).Assembly.CodeBase);
            dir = new System.Uri(dir).LocalPath;
            Directory.SetCurrentDirectory(dir);

            SolutionDirectory = ".".ToFullPath().ParentDirectory().ParentDirectory().ParentDirectory().ParentDirectory();
            StagingDirectory = SolutionDirectory.AppendPath("bottles-staging");
            SourceDirectory = SolutionDirectory.AppendPath("src").AppendPath("BottleProject");
            ZipsDirectory = SolutionDirectory.AppendPath("zips");
            BottleRunnerFile = SolutionDirectory.AppendPath("src").AppendPath("Bottles.Console").AppendPath("bin").AppendPath("debug").AppendPath("BottleRunner.exe");

            if (!File.Exists(BottleRunnerFile))
            {
                BottleRunnerFile = SolutionDirectory.AppendPath("src").AppendPath("Bottles.Console").AppendPath("bin").AppendPath("release").AppendPath("BottleRunner.exe");
            }
        }

        public static void CleanStagingDirectory()
        {
            var system = new FileSystem();
            system.CleanDirectory(StagingDirectory);
            Thread.Sleep(100);
        }

        public static void ResetBottleProjectCode()
        {
            var system = new FileSystem();
            CleanStagingDirectory();

            system.Copy(SourceDirectory, StagingDirectory);

            system.CreateDirectory(ZipsDirectory);
            system.CleanDirectory(ZipsDirectory);
        }

        public static void Recompile()
        {
            var info = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {SolutionDirectory.AppendPath("compile-bottle-staging.cmd")}",
                WorkingDirectory = SolutionDirectory

            };

            var returnCode = new ProcessRunner().Run(info, text => Debug.WriteLine(text));
            returnCode.ExitCode.ShouldEqual(0);
        }

        public static void SetAssemblyVersion(string version)
        {
            var file = StagingDirectory.AppendPath("version.cs");
            new FileSystem().AlterFlatFile(file, list => {
                list.Clear();

                list.Add("using System.Reflection;");
                list.Add(" ");
                list.Add("[assembly: AssemblyVersion(\"{0}\")]".ToFormat(version));
                list.Add("[assembly: AssemblyFileVersion(\"{0}\")]".ToFormat(version));
            });
        }

        public static void WriteZipVersion(string version)
        {
            new FileSystem().WriteStringToFile(StagingDirectory.AppendPath(BottleFiles.VersionFile), version);
        }

        public static void SetData(string value)
        {
            var file = StagingDirectory.AppendPath("data").AppendPath("1.txt");
            new FileSystem().WriteStringToFile(file, value);
        }

        public static void SetConfig(string value)
        {
            var file = StagingDirectory.AppendPath("config").AppendPath("1.txt");
            new FileSystem().WriteStringToFile(file, value);
        }

        public static void SetContent(string value)
        {
            var file = StagingDirectory.AppendPath("content").
                AppendPath("scripts").AppendPath("script1.js");
            new FileSystem().WriteStringToFile(file, value);
        }

        public static void RunBottlesCommand(string arguments)
        {
            var processInfo = new ProcessStartInfo(BottleRunnerFile)
            {
                Arguments = arguments,
                WorkingDirectory = SolutionDirectory
            };

            var processReturn = new ProcessRunner().Run(processInfo, text => Debug.WriteLine(text));
            processReturn.ExitCode.ShouldEqual(0);
        }

        public static void AlterManifest(Action<PackageManifest> alteration)
        {
            var fileSystem = new FileSystem();
            var manifest = fileSystem.LoadPackageManifestFrom(StagingDirectory);

            alteration(manifest);

            fileSystem.WriteObjectToFile(StagingDirectory.AppendPath(PackageManifest.FILE), manifest);
        }
    }
}