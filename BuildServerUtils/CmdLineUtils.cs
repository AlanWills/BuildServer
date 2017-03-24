using System;
using System.Diagnostics;
using System.IO;
 
namespace BuildServerUtils
{
    /// <summary>
    /// Utility class for performing git operations from within C#
    /// </summary>
    public static class CmdLineUtils
    {
        public static string Git = @"C:\Program Files\Git\bin\git.exe";
        public static string MSBuild = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe";
        public static string VSTest = @"C:\Program Files(x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe";

        /// <summary>
        /// Executes a windows command prompt command in a windowless process.
        /// Can provide a callback which will be run after the process is complete.
        /// Asynchronously prints out the standard error and standard output to the Console.
        /// </summary>
        /// <param name="commandAndArgs"></param>
        /// <param name="onCommandCompleteCallback"></param>
        /// <returns></returns>
        public static void PerformCommand(
            string fileName, 
            string workingDirectory,
            string args = "", 
            TextWriter outputWriter = null, 
            EventHandler onCommandCompleteCallback = null)
        {
            ProcessStartInfo cmdInfo = CreateCmdLineProcessStartInfo(workingDirectory, args, outputWriter != null);
            cmdInfo.FileName = fileName;
            RunProcess(cmdInfo, onCommandCompleteCallback, outputWriter);
        }

        /// <summary>
        /// Creates the process start info for running a command in a windowless process in the current working directory with the inputted arguments.
        /// StdError and StdOutput are redirected since it is windowless.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static ProcessStartInfo CreateCmdLineProcessStartInfo(string workingDirectory, string arguments = "", bool redirectOutput = false)
        {
            ProcessStartInfo cmdInfo = new ProcessStartInfo();
            cmdInfo.UseShellExecute = false;
            cmdInfo.Arguments = arguments;
            cmdInfo.WorkingDirectory = workingDirectory;

            if (redirectOutput)
            {
                cmdInfo.CreateNoWindow = true;
                cmdInfo.RedirectStandardError = true;
                cmdInfo.RedirectStandardOutput = true;
            }

            return cmdInfo;
        }

        /// <summary>
        /// Creates a process which outputs error and output asynchronously to the standard output and will block until complete.
        /// </summary>
        /// <param name="processInfo"></param>
        /// <param name="onCommandCompleteCallback"></param>
        private static void RunProcess(ProcessStartInfo processInfo, EventHandler onCommandCompleteCallback, TextWriter outputWriter = null)
        {
            Process process = new Process();
            process.StartInfo = processInfo;

            if (outputWriter != null)
            {
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => { outputWriter.WriteLine(e.Data); };
                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => { outputWriter.WriteLine(e.Data); };
            }

            if (onCommandCompleteCallback != null)
            {
                process.Disposed += onCommandCompleteCallback;
            }

            process.Start();

            if (outputWriter != null)
            {
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
            }

            process.WaitForExit();
            process.Close();
        }
    }
}
