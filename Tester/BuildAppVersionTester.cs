namespace Tester;

public class BuildAppVersionTester
{
    public BuildAppVersionTester()
    {
        var now = DateTime.UtcNow;

        NodaTime.IClock clock = new NodaTime.Testing.FakeClock(NodaTime.Instant.FromDateTimeUtc(now));
        
        instance = new Universe.Version.BuildAppVersion();

#if DEBUG
        instance.Clock = clock;
#endif

        instance.BaseVersionPrefix = "1.2";
        instance.VersionSuffix = "beta3";
        instance.SourceRevisionId = "aabbcc";
        //instance.IsTimeFormat = true;
    }
    readonly Universe.Version.BuildAppVersion instance;


    [Fact]
    void majorMinorFormat()
    {
        instance.FormatId = 0;
        instance.Execute();

        var exp = "1.2";
        Assert.StartsWith(exp, instance.VersionPrefix);
        Assert.Contains($"-beta3", instance.Version);
        Assert.Contains($"-beta3", instance.PackageVersion);
    }


    [Fact]
    void timeFormat()
    {
        instance.FormatId = 1;
        instance.Execute();

        var exp = DateTime.Now.ToString("yyyy.MM.dd.HHmm");
        Assert.Equal(exp, instance.VersionPrefix);
        Assert.Equal($"{exp}-beta3", instance.Version);
        Assert.Equal($"{exp}-beta3", instance.PackageVersion);
    }

    [Fact]
    void hybridFormat()
    {
        instance.FormatId = 2;
        instance.Execute();
        var now = DateTime.Now.ToString("yyyy.MM.dd.HHmmss");

        var exp = "1.2";
        Assert.Equal(exp, instance.VersionPrefix);
        Assert.Equal($"{exp}-beta3-{now}", instance.Version);
        Assert.Equal($"{exp}-beta3-{now}", instance.PackageVersion);
    }

    [Fact]
    void nugetFormat()
    {
        instance.FormatId = 3;
        instance.Execute();
        var now = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");

        var exp = "1.2";
        Assert.Equal(exp, instance.VersionPrefix);
        Assert.Equal($"{exp}-beta3-{now}", instance.Version);
        Assert.Equal($"1.2.0-beta3-{now}", instance.PackageVersion);
    }
}

