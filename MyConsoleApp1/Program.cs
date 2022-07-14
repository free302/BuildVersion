using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Universe.MyConsoleApp1
{
    internal class Program
    {
        public static void Main()
        {
            Console.WriteLine("running...press any key...");

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            dir = Path.GetDirectoryName(dir);
            dir = Path.GetDirectoryName(dir);
            dir = Path.GetDirectoryName(dir);
            dir = Path.GetDirectoryName(dir);
            var fn = @$"{dir}\publish\Universe.Version.dll";
            var assembly = Assembly.LoadFrom(fn);
            var version = assembly.CreateInstance("Universe.Version.BuildAppVersion");
            Console.WriteLine($"version= {version}");
            var fullName = version.GetType().FullName;
            Console.WriteLine($"FullName= {fullName}");
            //Debug.Assert(version != null);

            var now = DateTime.UtcNow;
            //NodaTime.IClock clock = new NodaTime.Testing.FakeClock(NodaTime.Instant.FromDateTimeUtc(now));
            //a = new BuildAppVersion();
            //a.Clock = clock;
            //a.BaseVersionPrefix = "1.2.3";
            //a.VersionSuffix = "beta3";
            //a.SourceRevisionId = "aabbcc";
            //a.IsTimeFormat = true;


            var dom = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var a in dom)
                Console.WriteLine(a.Location);

            Console.ReadKey();
        }
    }
}
