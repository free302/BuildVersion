using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Diagnostics;

[assembly: InternalsVisibleTo("Tester")]

namespace Universe.BuildVersion
{
    internal enum VersionType { Assembly, Setup };
    internal class Program
    {
        static string _dir;
        static string _filePath;
        const string _FileName = "AssemblyInfo.cs";
        static string _inputString;
        const string uninstallFileName = "uninstall.bat";

        static VersionType _whichVersion;
        static Version _oldVersion;
        static Version _newVersion;

        static string _productGuid;

        static Regex[] regEx =
        {
            //new Regex("AssemblyVersion(?:Attribute)?\\(\\s*?\"(?<version>(?<major>[0-9]+)\\.(?<minor>[0-9]+)\\.(?<build>[0-9]+)\\.(?<revision>[0-9]+))\"\\s*?\\)"),
            new Regex("AssemblyVersion(?:Attribute)?\\(\\s*?\"(?<version>(?<major>[0-9]+)\\.(?<minor>[0-9]+)\\.(?<build>[0-9]+)\\.(?<revision>[0-9]+))\"\\s*?\\)"),

            new Regex("AssemblyFileVersion(?:Attribute)?\\(\\s*?\"(?<version>(?<major>[0-9]+)\\.(?<minor>[0-9]+)\\.(?<build>[0-9]+)\\.(?<revision>[0-9]+))\"\\s*?\\)"),

            new Regex("(\"ProductVersion\" = \"8:)(?<version>\\d+(\\.\\d+)+)\"")
            //"(""ProductVersion"" = ""8:)(\d+(\.\d+)+)"""
        };
        static string[] _strReplace =
        {
            "AssemblyVersion(\"{0}\")",
            "AssemblyFileVersion(\"{0}\")",
            "\"ProductVersion\" = \"8:{0}\""
        };


        internal static void Main(string[] args)
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
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        static void printUsage()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"UpdateVersion {typeof(Program).Assembly.GetName().Version}");
            sb.AppendLine("Usage: UpdateVersion {A|S} FilePath");
            sb.AppendLine(" A : Assembly version");
            sb.AppendLine(" S : Setup project version");
            sb.AppendLine(" FilePath : AssemblyInfo.cs or xxx.csproj");
            Console.WriteLine(sb.ToString());
            Debug.WriteLine(sb.ToString());
        }
        internal static (VersionType type, string dir, string path) parseArgs(string[] args)
        {
            try
            {
                _dir = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? Environment.CurrentDirectory;
                _filePath = Path.Combine(_dir, _FileName);

                if (args.Length < 2) throw new ArgumentException("Insufficient arguments");

                parseType(args[0]);
                parseFile(args[1]);
            }
            catch(ArgumentException)
            {
                printUsage();
                throw;
            }
            finally
            {
                var msg = $"[UpdateVersionFx] _dir={_dir}\n[UpdateVersionFx] _filePath={_filePath}";
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
            }
            return (_whichVersion, _dir, _filePath);
        }

        private static void parseType(string typeArg)
        {
            _whichVersion = char.ToUpper(typeArg[0]) switch
            {
                'A' => VersionType.Assembly,
                'S' => VersionType.Setup,
                _ => VersionType.Assembly
            };
        }

        private static void parseFile(string fileArg)
        {
            if (Path.IsPathRooted(fileArg)) _dir = Path.GetDirectoryName(fileArg) ?? _dir;
            else
            {
                var dir = Path.GetDirectoryName(fileArg);
                if (!string.IsNullOrWhiteSpace(dir)) _dir = Path.Combine(_dir, dir);
            }
            var fn = Path.GetFileName(fileArg);
            _filePath = Path.Combine(_dir, string.IsNullOrWhiteSpace(fn) ? _FileName : fn);
        }

        internal static void calcCurrentVersion()
        {
            _inputString = File.ReadAllText(_filePath);

            Regex reg = regEx[(int)_whichVersion];
            Match match = reg.Match(_inputString);

            _oldVersion = new Version(match.Groups["version"].ToString());
        }

        internal static void calcNewVersion() => _newVersion = _whichVersion switch
        {
            VersionType.Assembly => GetCurrentBuildVersion.CalcVersion_BuildTime(_oldVersion),
            VersionType.Setup => GetCurrentBuildVersion.CalcSetupVerions(_oldVersion)
        };

        static string replaceGuid()
        {
            string strReg = "(\"{0}\" = \"8:{)(?<guid>\\S+)}\"";
            string strReplace = "\"{0}\" = \"8:{{1}}\"";

            string strReg1 = strReg.Replace("{0}", "ProductCode");
            string strReg2 = strReg.Replace("{0}", "PackageCode");

            Regex regex = new Regex(strReg.Replace("{0}", "ProductCode"));
            _productGuid = Guid.NewGuid().ToString().ToUpper();
            string replace = strReplace.Replace("{0}", "ProductCode");
            replace = replace.Replace("{1}", _productGuid);
            string outputString = regex.Replace(_inputString, replace, 1);

            regex = new Regex(strReg.Replace("{0}", "PackageCode"));
            replace = strReplace.Replace("{0}", "PackageCode");
            replace = replace.Replace("{1}", Guid.NewGuid().ToString().ToUpper());
            outputString = regex.Replace(outputString, replace, 1);

            return outputString;
        }

        internal static void writeNewVersion()
        {
            string outputString = "";
            var nv = _newVersion;
            switch (_whichVersion)
            {
                case VersionType.Assembly:
                    var newVer = nv.ToString();
                    outputString = regEx[0].Replace(_inputString, string.Format(_strReplace[0], newVer), 1);
                    outputString = regEx[1].Replace(outputString, string.Format(_strReplace[1], newVer), 1);
                    break;

                case VersionType.Setup:
                    newVer = $"{nv.Major}.{nv.Minor}.{nv.Build}";
                    outputString = regEx[2].Replace(replaceGuid(), string.Format(_strReplace[2], newVer), 1);
                    break;
            }
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
