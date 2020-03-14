using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BuildVersion;

namespace UpdateVersion
{
    class Program
    {
        enum VersionType { Assembly, Setup };

        static string _filePath;
        static string _dir;
        static string inputString;
        const string uninstallFileName = "uninstall.bat";

        static VersionType whichVersion;
        static Version oldVersion;
        static Version _newVersion;

        static string productGuid;

        static Regex[] regEx =
        {
            //new Regex("AssemblyVersion(?:Attribute)?\\(\\s*?\"(?<version>(?<major>[0-9]+)\\.(?<minor>[0-9]+)\\.(?<build>[0-9]+)\\.(?<revision>[0-9]+))\"\\s*?\\)"),
            new Regex("AssemblyVersion(?:Attribute)?\\(\\s*?\"(?<version>(?<major>[0-9]+)\\.(?<minor>[0-9]+)\\.(?<build>[0-9]+)\\.(?<revision>[0-9]+))\"\\s*?\\)"),

            new Regex("AssemblyFileVersion(?:Attribute)?\\(\\s*?\"(?<version>(?<major>[0-9]+)\\.(?<minor>[0-9]+)\\.(?<build>[0-9]+)\\.(?<revision>[0-9]+))\"\\s*?\\)"),

            new Regex("(\"ProductVersion\" = \"8:)(?<version>\\d+(\\.\\d+)+)\"")
            //"(""ProductVersion"" = ""8:)(\d+(\.\d+)+)"""
        };
        static string[] strReplace =
        {
            "AssemblyVersion(\"{0}\")",
            "AssemblyFileVersion(\"{0}\")",
            "\"ProductVersion\" = \"8:{0}\""
        };


        static void Main(string[] args)
        {
            try
            {
                parseArgs(args);
                calcCurrentVersion();
                calcNewVersion();
                writeNewVersion();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        static void parseArgs(string[] args)
        {
            if (args.Length < 1) whichVersion = VersionType.Assembly;
            else
            {
                whichVersion = args[0].ToUpper()[0] switch
                {
                    'A' => VersionType.Assembly,
                    'S' => VersionType.Setup,
                    _ => VersionType.Assembly
                };
            }

            _dir = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? Environment.CurrentDirectory;
            _filePath = Path.Combine(_dir, "AssemblyInfo.cs");
            if (args.Length < 2) return;

            _filePath = args[1];
            if (Path.IsPathRooted(_filePath)) _dir = Path.GetDirectoryName(_filePath) ?? _dir;
            else
            {
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    _dir = Path.Combine(_dir, dir);
                    _filePath = Path.Combine(_dir, _filePath);
                }
            }
        }

        static void calcCurrentVersion()
        {
            inputString = File.ReadAllText(_filePath);

            Regex reg = regEx[(int)whichVersion];
            Match match = reg.Match(inputString);

            oldVersion = new Version(match.Groups["version"].ToString());
        }

        static void calcNewVersion()
        {
            
            switch (whichVersion)
            {
                case VersionType.Assembly:
                    _newVersion = GetCurrentBuildVersion.GetCurrentBuildVersionString(oldVersion);
                    break;
                case VersionType.Setup:
                    var major = DateTime.Now.Year % 100;
                    var minor = DateTime.Now.Month;
                    var build = calcSetupBuild();
                    //revision = DateTime.Now.Hour;
                    _newVersion = new Version(major, minor, build, 0);
                    break;
            }
        }
        static int calcSetupBuild()
        {
            int major = DateTime.Now.Year % 100;
            int minor = DateTime.Now.Month;
            int oldDay = oldVersion.Build / 100;
            int oldRev = oldVersion.Build % 100;
            int newDay = DateTime.Now.Day;

            int newBuild = (oldVersion.Major == major && oldVersion.Minor == minor && oldDay == newDay) 
                ? oldVersion.Build + 1 : newDay * 100 + 1;
            return newBuild;
        }

        static string replaceGuid()
        {
            string strReg = "(\"{0}\" = \"8:{)(?<guid>\\S+)}\"";
            string strReplace = "\"{0}\" = \"8:{{1}}\"";

            string strReg1 = strReg.Replace("{0}", "ProductCode");
            string strReg2 = strReg.Replace("{0}", "PackageCode");

            Regex regex = new Regex(strReg.Replace("{0}", "ProductCode"));
            productGuid = Guid.NewGuid().ToString().ToUpper();
            string replace = strReplace.Replace("{0}", "ProductCode");
            replace = replace.Replace("{1}", productGuid);
            string outputString = regex.Replace(inputString, replace, 1);

            regex = new Regex(strReg.Replace("{0}", "PackageCode"));
            replace = strReplace.Replace("{0}", "PackageCode");
            replace = replace.Replace("{1}", Guid.NewGuid().ToString().ToUpper());
            outputString = regex.Replace(outputString, replace, 1);

            return outputString;
        }

        static void writeNewVersion()
        {
            string replace = "";
            string strInput = "";

            switch (whichVersion)
            {
                case VersionType.Assembly:
                    replace = string.Format(strReplace[(int)whichVersion], _newVersion.ToString());
                    strInput = inputString;
                    break;

                case VersionType.Setup:
                    replace = string.Format(strReplace[(int)whichVersion], string.Format("{0}.{1}.{2:0000}", _newVersion.Major, _newVersion.Minor, _newVersion.Build));
                    strInput = replaceGuid();
                    break;
            }

            string outputString = regEx[(int)whichVersion].Replace(strInput, replace, 1);
            File.WriteAllText(_filePath, outputString, Encoding.UTF8);
        }

        static void writeUninstall(string guid)
        {
            string path = Path.Combine(_dir, uninstallFileName);
            if (File.Exists(path)) File.Delete(path);

            string outputString = "call msiexec /uninstall {{0}}\nexit".Replace("{0}", guid);
            File.WriteAllText(outputString, path, Encoding.UTF8);
        }

    }
}
