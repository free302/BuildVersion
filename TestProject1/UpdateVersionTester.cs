namespace Tester
{
    public class UpdateVersionTester
    {
        public UpdateVersionTester()
        {
            var now = DateTime.UtcNow;
            NodaTime.IClock clock = new NodaTime.Testing.FakeClock(NodaTime.Instant.FromDateTimeUtc(now));
            
            instance = new Jdt.Version.BuildAppVersion();
            instance.Clock = clock;
            instance.BaseVersionPrefix = "1.2.3";
            instance.VersionSuffix = "beta3";
            instance.SourceRevisionId = "aabbcc";
            //instance.IsTimeFormat = true;
        }
        readonly Jdt.Version.BuildAppVersion instance;

        [Fact]
        void timeFormat()
        {
            instance.IsTimeFormat = true;
            instance.Execute();

            var exp = DateTime.UtcNow.ToString("yy.MM.dd.HHmm");
            Assert.Equal(exp, instance.VersionPrefix);
            Assert.Equal($"{exp}-beta3", instance.PackageVersion);
            Assert.Equal($"{exp}-beta3+aabbcc", instance.Version);
        }


        [Fact]
        void majorMinorFormat()
        {
            instance.IsTimeFormat = false;
            instance.Execute();

            var exp = "1.2";
            Assert.StartsWith(exp, instance.VersionPrefix);
            Assert.Contains("-beta3", instance.PackageVersion);
            Assert.Contains("-beta3+aabbcc", instance.Version);
        }
    }

    
}