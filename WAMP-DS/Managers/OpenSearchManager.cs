using System.Diagnostics;
using System.IO;
using System.Net.Http;
using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class OpenSearchManager
    {
        private Process? _process;

        public event EventHandler? StatusChanged;

        public OpenSearchStatus Status { get; private set; }
            = OpenSearchStatus.Stopped;


        public string Version =>
            "3.8.0";


        public int Port =>
            9200;


        public bool IsRunning =>
            _process != null &&
            !_process.HasExited;


        private void SetStatus(OpenSearchStatus status)
        {
            Status = status;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task StartAsync(
            string openSearchPath
        )
        {
            if (Status == OpenSearchStatus.Starting)
            {
                Stop();
                KillJavaProcesses();
            }

            if (IsRunning)
                return;

            string javaPath =
                Path.Combine(
                    openSearchPath,
                    "jdk");


            string batchFile =
                Path.Combine(
                    openSearchPath,
                    "bin",
                    "opensearch.bat"
                );

            string javaExecutable =
                Path.Combine(
                    javaPath,
                    "bin",
                    "java.exe");

            if (!File.Exists(javaExecutable))
            {
                throw new FileNotFoundException(
                    "The OpenSearch bundled JDK was not found.",
                    javaExecutable);
            }


            if (!File.Exists(batchFile))
            {
                throw new FileNotFoundException(
                    "OpenSearch batch file not found.",
                    batchFile
                );
            }


            SetStatus(
                OpenSearchStatus.Starting
            );


            ProcessStartInfo startInfo =
                new()
                {
                    FileName = "cmd.exe",

                    Arguments =
                        $"/c \"\"{batchFile}\"\"",

                    WorkingDirectory =
                        openSearchPath,

                    UseShellExecute =
                        false,

                    CreateNoWindow =
                        true,

                    WindowStyle =
                        ProcessWindowStyle.Hidden
                };


            startInfo.EnvironmentVariables["JAVA_HOME"] =
                javaPath;


            _process =
                Process.Start(
                    startInfo
                );

            if (_process == null)
                return;

            _process.EnableRaisingEvents = true;

            _process.Exited += (s, e) =>
            {
                SetStatus(OpenSearchStatus.Stopped);
            };


            if (_process == null)
            {
                SetStatus(
                    OpenSearchStatus.Failed
                );

                throw new Exception(
                    "Failed to start OpenSearch."
                );
            }


            bool ready =
                await WaitForReadyAsync(
                    300
                );


            if (ready)
            {
                SetStatus(
                    OpenSearchStatus.Running
                );

                Debug.WriteLine(
                    "OpenSearch ready."
                );
            }
            else
            {
                SetStatus(
                    OpenSearchStatus.Failed
                );

                Debug.WriteLine(
                    "OpenSearch failed to start."
                );

                Stop();

                KillJavaProcesses();
            }
        }


        public void Stop()
        {
            try
            {
                if (_process != null &&
                    !_process.HasExited)
                {
                    Debug.WriteLine("Stopping OpenSearch...");

                    _process.Kill(true);

                    if (!_process.WaitForExit(10000))
                    {
                        Debug.WriteLine(
                            "OpenSearch did not stop within timeout."
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"OpenSearch stop failed: {ex.Message}"
                );
            }
            finally
            {
                _process?.Dispose();
                _process = null;

                SetStatus(OpenSearchStatus.Stopped);
            }
        }

        public void Kill()
        {
            try
            {
                if (_process != null &&
                    !_process.HasExited)
                {
                    Debug.WriteLine("Killing OpenSearch...");

                    _process.Kill(true);
                    _process.WaitForExit(2000);
                }

                // Kill any orphaned Java processes
                foreach (Process process in Process.GetProcessesByName("java"))
                {
                    try
                    {
                        Debug.WriteLine(
                            $"Killing Java process {process.Id}..."
                        );

                        process.Kill(true);
                        process.WaitForExit(2000);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            $"Unable to kill Java process: {ex.Message}"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"OpenSearch kill failed: {ex.Message}"
                );
            }
            finally
            {
                _process?.Dispose();
                _process = null;
            }
        }

        public async Task<bool> WaitForReadyAsync(
            int timeoutSeconds = 120)
        {
            using HttpClient client =
                new();


            DateTime start =
                DateTime.Now;


            while (
                (DateTime.Now - start).TotalSeconds
                < timeoutSeconds)
            {
                try
                {
                    HttpResponseMessage response =
                        await client.GetAsync(
                            $"http://localhost:{Port}"
                        );


                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                catch
                {
                }


                await Task.Delay(
                    1000
                );
            }


            return false;
        }

        private void KillJavaProcesses()
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("java"))
                {
                    try
                    {
                        process.Kill(true);

                        Debug.WriteLine(
                            $"Killed Java process {process.Id}"
                        );
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"Java cleanup failed: {ex.Message}"
                );
            }
        }
    }
}