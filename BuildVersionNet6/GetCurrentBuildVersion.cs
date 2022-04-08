using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Universe.BuildVersion
{
    public class GetCurrentBuildVersion : Task
    {
        [Output]
        public string? Version { get; set; }

        public string? BaseVersion { get; set; }

        public override bool Execute()
        {
            var originalVersion = System.Version.Parse(BaseVersion ?? "1.0.0");
            Version = calcVersion(originalVersion).ToString();
            return true;
        }

        static readonly DateTime _baseDay = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static Version calcVersion(Version baseVersion)
        {
            var now = DateTime.UtcNow;
            var build = (now - _baseDay).Days;
            var revision = ((int)new TimeSpan(now.Hour, now.Minute, now.Second).TotalSeconds) / 2;
            return new Version(baseVersion.Major, baseVersion.Minor, build, revision);
        }


    }
}
