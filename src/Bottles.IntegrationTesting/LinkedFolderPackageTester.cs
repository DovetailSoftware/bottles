using System;
using FubuCore;
using NUnit.Framework;
using Shouldly;

namespace Bottles.IntegrationTesting
{
    [TestFixture]
    public class LinkedFolderPackageTester : IntegrationTestContext
    {
        [Test]
        public void read_and_write_files_from_a_linked_folder()
        {
            RunBottlesCommand("init bottles-staging BottleProject");

            AlterManifest(manifest =>
            {
                manifest.RemoveAllAssemblies();
                manifest.AddAssembly("BottleProject");
            });

            Recompile();

            RunBottlesCommand("link {0} {1}".ToFormat(AppDomain.CurrentDomain.BaseDirectory, StagingDirectory));

            _domain.Proxy.LoadViaFolder(StagingDirectory);
            _domain.Proxy.ReadWebContent("content/scripts/script1.js").Trim()
                   .ShouldBe("var x = 1;");

            _domain.Proxy.ReadData("1.txt").Trim()
                   .ShouldBe("Original Value");
        }
    }
}