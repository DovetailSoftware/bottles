﻿using System;
using System.IO;
using Bottles.Commands;
using Bottles.Environment;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    [CommandDescription("Runs installer actions and/or environment checks for an application")]
    public class InstallCommand : FubuCommand<InstallInput>
    {
        public override bool Execute(InstallInput input)
        {
            input.AppFolder = AliasCommand.AliasFolder(input.AppFolder);
            Execute(input, new FileSystem());
            return true;
        }

        public void Execute(InstallInput input, IFileSystem fileSystem)
        {
            installManifest(input);
        }

        private void installManifest(InstallInput input)
        {
            Console.WriteLine("Executing the installers for the FubuMVC application at '{0}'", input.AppFolder);

            var run = CreateEnvironmentRun(input);

            try
            {
                run.AssertIsValid();
            }
            catch (EnvironmentRunnerException e)
            {
                Console.WriteLine("The supplied directives to the installer commands are either incomplete or invalid");
                Console.WriteLine(e.Message);

                throw;
            }

            runTheEnvironment(input, new EnvironmentGateway(run));
        }


        private void runTheEnvironment(InstallInput input, IEnvironmentGateway gateway)
        {
            var runner = new InstallationRunner(gateway, new InstallationLogger());
            runner.RunTheInstallation(input);
        }

        public virtual void WriteEnvironmentRunIsInvalid(string message)
        {
            Console.WriteLine("Application Manifest file is incomplete or invalid");
            Console.WriteLine(message);
        }

        public static EnvironmentRun CreateEnvironmentRun(InstallInput input)
        {
            var binFolder = FileSystem.Combine(input.AppFolder, "bin").ToFullPath();
            var configFile = input.ConfigFileFlag ?? "web.config";
            configFile = Path.GetFileName(configFile);
            configFile = FileSystem.Combine(input.AppFolder, configFile).ToFullPath();

            return new EnvironmentRun{
                ApplicationBase = binFolder,
                AssemblyName = input.EnvironmentAssemblyFlag,
                EnvironmentClassName = input.EnvironmentClassNameFlag,
                ConfigurationFile = configFile,
                ApplicationDirectory = input.AppFolder.ToFullPath()
            };
        }
    }
}