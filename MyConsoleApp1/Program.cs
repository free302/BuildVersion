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
            var assembly = Assembly.GetExecutingAssembly();
            var prod = GetProductName(assembly);
            
            Console.WriteLine($"product={prod}, version= {assembly.GetName().Version}");
        }
        static string GetProductName(Assembly assembly)
        {
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            return attributes.Any() ? ((AssemblyProductAttribute)attributes[0]).Product : "";
        }

    }
}
