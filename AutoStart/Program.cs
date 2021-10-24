using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoStartYUR
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var mappings = File.ReadAllLines(@"config.ini");
            foreach (var mapping in mappings)
            {
                var split = mapping.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                Task.Run(async () => await AutoStartAsync(split.First(), split.Last()));
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create the form to display the notification icon and run the application
            var form = new Form();
            Application.Run();
        }

        /// <summary>
        /// Auto start <paramref name="startingProcessName"/> when <paramref name="listeningProcessName"/> is running.
        /// </summary>
        /// <param name="listeningProcessName">Process to listen for.</param>
        /// <param name="startingProcessName">Process to start.</param>
        /// <returns></returns>
        private static async Task AutoStartAsync(string listeningProcessName, string startingProcessName)
        {
            const int waitInMs = 1000 * 15; // wait for 15 seconds

            while (true)
            {
                var listeningProcess = Process.GetProcessesByName(listeningProcessName).FirstOrDefault();
                var steamApp = startingProcessName.StartsWith("steam") ? true : false;
                if (listeningProcess != default &&
                    (steamApp ||
                    !Process.GetProcessesByName(Path.GetFileNameWithoutExtension(startingProcessName)).Any()))
                {
                    var startingProcess = Process.Start(new ProcessStartInfo(startingProcessName)
                    {
                        UseShellExecute = steamApp ? true : false,
                        WindowStyle = steamApp ? ProcessWindowStyle.Normal : ProcessWindowStyle.Minimized,
                    });
                    await listeningProcess.WaitForExitAsync();
                    if (steamApp)
                    {
                        startingProcess.Close();
                    }
                    else
                    {
                        startingProcess.CloseMainWindow();
                    }
                }

                await Task.Delay(waitInMs);
            }
        }
    }
}
