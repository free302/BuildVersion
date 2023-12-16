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
        Assert.Contains($"-beta3", instance.PackageVersion);
        Assert.Contains($"-beta3+aabbcc", instance.Version);
    }


    [Fact]
    void timeFormat()
    {
        instance.FormatId = 1;
        instance.Execute();

        var exp = DateTime.Now.ToString("yyyy.MM.dd.HHmm");
        Assert.Equal(exp, instance.VersionPrefix);
        Assert.Equal($"{exp}-beta3", instance.PackageVersion);
        Assert.Equal($"{exp}-beta3+aabbcc", instance.Version);
    }

    [Fact]
    void hybridFormat()
    {
        instance.FormatId = 2;
        instance.Execute();
        var now = DateTime.Now.ToString("yyyy.MM.dd.HHmmss");

        var exp = "1.2";
        Assert.Equal(exp, instance.VersionPrefix);
        Assert.Equal($"{exp}-{now}", instance.PackageVersion);
        Assert.Equal($"{exp}-{now}+aabbcc", instance.Version);
    }


}

