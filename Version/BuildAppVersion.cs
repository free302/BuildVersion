using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NodaTime;

namespace Universe.Version;

using V = System.Version;

public class BuildAppVersion : Task
{
    public BuildAppVersion()
    {

    }


    #region ---- OUTPUT : ----

    /// <summary>
    /// MSBuild $(VersionPrefix)
    /// </summary>
    [Output] public string VersionPrefix { get; set; } = "1.0.0.0";

    /// <summary>
    /// MSBuild $(PackageVersion)
    /// nuget 패키지 만들때 사용되는 버전 : $(VersionPrefix)-$(VersionSuffix)
    /// </summary>
    [Output] public string PackageVersion { get; set; } = "1.0.0.0";

    /// <summary>
    /// MSBuild $(Version)
    /// $(VersionPrefix)-$(VersionSuffix)-$(SourceRevisionId)
    /// </summary>
    [Output] public string Version { get; set; } = "1.0.0.0";

    #endregion


    #region ---- INPUT Parameters ----

    /// <summary>
    /// 현재 버전 : IsTimeFormat == false 경우 사용
    /// </summary>
    public string BaseVersionPrefix { get; set; } = "1.0.0.0";

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
    public bool IsTimeFormat { get; set; } = true;

    #endregion

    /// <summary>
    /// 테스트를 위한 클럭
    /// </summary>
    public IClock? Clock { get; set; }

    public override bool Execute()
    {
        VersionPrefix = (IsTimeFormat ? calcPrefix_TimeFormat(Clock) : calcPrefix(BaseVersionPrefix, Clock)).ToString();
        //Version = SourceRevisionId == "" ? PackageVersion : $"{PackageVersion}+{SourceRevisionId}";
        Version = VersionSuffix == "" ? VersionPrefix : $"{VersionPrefix}-{VersionSuffix}";
        PackageVersion = Version;

        Debug.WriteLine($"[{nameof(BuildAppVersion)}] Prefix={BaseVersionPrefix}, Suffix={VersionSuffix}, RevisionId={SourceRevisionId}");
        Debug.WriteLine($"[{nameof(BuildAppVersion)}] new VersionPrefix={VersionPrefix}, PackageVersion={PackageVersion}, Version={Version}");
        return true;
    }

    /// <summary>
    /// Major.Minor 형식
    /// </summary>
    /// <param name="baseVersinString"></param>
    /// <returns></returns>
    static string calcPrefix(string baseVersinString, IClock? clock)
    {
        var baseVersion = V.Parse(baseVersinString);

        var _baseDay = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var now = clock?.GetCurrentInstant().ToDateTimeUtc() ?? DateTime.UtcNow;

        var build = (now - _baseDay).Days;
        var revision = (int)now.TimeOfDay.TotalSeconds / 2;
        return new V(baseVersion.Major, baseVersion.Minor, build, revision).ToString();
    }

    /// <summary>
    /// 현재 날짜 시각 형식
    /// </summary>
    /// <returns></returns>
    static string calcPrefix_TimeFormat(IClock? clock)
    {
        var now = clock?.GetCurrentInstant().ToDateTimeUtc() ?? DateTime.UtcNow;
        //return new V(now.Year % 100, now.Month, now.Day, now.Hour * 100 + now.Minute).ToString();
        return now.ToString("yyyy.MM.dd.HHmm");
    }

    public override string ToString() => PackageVersion;
}
