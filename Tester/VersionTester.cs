using System;
using Xunit;
using p = Universe.BuildVersion.Program;
using Universe.BuildVersion;
using System.IO;

namespace Tester
{
    public class VersionTester
    {
        [Theory]
        [InlineData("test.cs")]
        [InlineData(@"test\test.cs")]
        [InlineData(@"c:\test\test.cs")]
        [InlineData(@"test\")]
        [InlineData("test")]
        public void test(string arg)
        {
            var result = p.parseArgs(new[] { arg });
        }

        [Fact]
        public void test2()
        {
            var path = @"E:\Dev\@Common\Version\App\Version\UpdateVersionFx\Properties\AssemblyInfo.cs";
            var result = p.parseArgs(new[] { "A", path });

            p.calcCurrentVersion();
            p.calcNewVersion();
            p.writeNewVersion();
        }


    }
}
