using Microsoft.Build.Framework;
using System;
using System.Diagnostics;

namespace Universe.Version;

using V = System.Version;

public class BuildAppVersion : Microsoft.Build.Utilities.Task
{
    public BuildAppVersion() { }


    #region ---- OUTPUT : ----

    /// <summary>
    /// MSBuild $(VersionPrefix)
    /// </summary>
    [Output] public string VersionPrefix { get; set; } = "1.0";

    /// <summary>
    /// MSBuild $(PackageVersion)
    /// nuget 패키지 만들때 사용되는 버전 : $(VersionPrefix)-$(VersionSuffix)
    /// </summary>
    [Output] public string PackageVersion { get; set; } = "1.0";

    /// <summary>
    /// MSBuild $(Version)
    /// $(VersionPrefix)-$(VersionSuffix)-$(SourceRevisionId)
    /// </summary>
    [Output] public string Version { get; set; } = "1.0";

    #endregion


    #region ---- INPUT Parameters ----

    /// <summary>
    /// 현재 버전 : IsTimeFormat == false 경우 사용
    /// </summary>
    public string BaseVersionPrefix { get; set; } = "";

    /// <summary>
    /// 버전 뒤에 붙일 BETA/RC 등
    /// </summary>
    public string VersionSuffix { get; set; } = "";

    /// <summary>
    /// 버전 뒤에 붙일 GIT 커밋번호 등 소스관리 정보
    /// </summary>
    public string SourceRevisionId { get; set; } = "";

    /// <summary>
    /// 버전 형식 지정
    ///  - true  : yy.MM.dd.HHmmss 형식
    ///  - false : major.minor.n(days).n(seconds)
    ///    - major.minor : BaseVersionPrefix 에서 추출
    ///    - n(days) : 2000.01.01 이후 오늘까지의 날수
    ///    - n(seconds) : 오늘 00:00:00 이후 현재 까지의 초수/2
    /// </summary>
    [Obsolete]
    public bool IsTimeFormat { get; set; } = true;

    /// <summary>
    /// 0 : major.minor.n(days).n(seconds)
    /// 1 : yyyy.MM.dd.HHmm
    /// 2 : major.minor-yy.MM.dd.HHmmss
    /// 3 : major.minor-yyMMddHHmmss
    /// </summary>
    public int FormatId { get; set; } = 0;

    public bool UsingUTC { get; set; } = false;

    #endregion


#if DEBUG
    /// <summary>
    /// 테스트를 위한 클럭
    /// </summary>    
    public NodaTime.IClock? Clock { get; set; }
#endif

    public override bool Execute()
    {
        if (string.IsNullOrWhiteSpace(BaseVersionPrefix)) BaseVersionPrefix = "1.0";

        var now = this.now();
        VersionPrefix = FormatId switch
        {
            0 => prefix_MajorMinorDaysSeconds(BaseVersionPrefix, now),
            1 => prefix_DateTime(now),
            _ => BaseVersionPrefix,
        };

        var suffixTetxt = string.IsNullOrWhiteSpace(VersionSuffix) ? "" : $"-{VersionSuffix}";
        var suffix = FormatId switch
        {
            2 => $"{suffixTetxt}-{suffix_DateTime(now)}",
            3 => $"{suffixTetxt}-{suffix_DateTime_Package(now)}",
            _ => suffixTetxt
        };
        Version = $"{VersionPrefix}{suffix}";

        //PackageVersion
        var packMiddle = "";
        if (FormatId == 3)
        {
            V.TryParse(VersionPrefix, out var v);
            if (v.Build == -1) packMiddle = ".0";
        }
        PackageVersion = $"{VersionPrefix}{packMiddle}{suffix}";

        //이건 ProductVersion
        //var src = string.IsNullOrWhiteSpace(SourceRevisionId) ? "" : $"+{SourceRevisionId}";

        log($"[in] BaseVersionPrefix={BaseVersionPrefix}");
        log($"[in] VersionSuffix={VersionSuffix}");
        log($"[in] SourceRevisionId={SourceRevisionId}");
        log($"[out] VersionPrefix={VersionPrefix}");
        log($"[out] PackageVersion={PackageVersion}");
        log($"[out] Version={Version}");
        return true;
    }

    /// <summary>
    /// Major.Minor 형식
    /// </summary>
    /// <param name="baseVersinString"></param>
    /// <returns></returns>
    static string prefix_MajorMinorDaysSeconds(string baseVersinString, DateTime now)
    {
        var baseVersion = V.Parse(baseVersinString);

        var _baseDay = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var build = (now - _baseDay).Days;
        var revision = (int)now.TimeOfDay.TotalSeconds / 2;
        return new V(baseVersion.Major, baseVersion.Minor, build, revision).ToString();
    }

    /// <summary>
    /// 현재 날짜 시각 형식
    /// </summary>
    /// <returns></returns>
    static string prefix_DateTime(DateTime now)
    {
        //return new V(now.Year % 100, now.Month, now.Day, now.Hour * 100 + now.Minute).ToString();
        return $"{now:yyyy.MM.dd.HHmm}";
    }

    /// <summary>
    /// 현재 날짜 시각 형식
    /// </summary>
    /// <returns></returns>
    static string suffix_DateTime(DateTime now)
    {
        //return new V(now.Year % 100, now.Month, now.Day, now.Hour * 100 + now.Minute).ToString();
        return now.ToString("yyyy.MM.dd.HHmmss");
    }
    static string suffix_DateTime_Package(DateTime now)
    {
        //return new V(now.Year % 100, now.Month, now.Day, now.Hour * 100 + now.Minute).ToString();
        return now.ToString("yyyy-MM-dd-HHmmss");
    }

    DateTime now()
    {
#if DEBUG

        var utc = Clock?.GetCurrentInstant().ToDateTimeUtc() ?? DateTime.UtcNow;
        var local = Clock?.GetCurrentInstant().ToDateTimeUtc().ToLocalTime() ?? DateTime.Now;
        return UsingUTC ? utc : local;
#else
        return UsingUTC ? DateTime.UtcNow : DateTime.Now;
#endif
    }

    [Conditional("DEBUG")]
    void log(string msg) => Debug.WriteLine(msg);

    public override string ToString() => Version;
}
