using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace WAMP_DS.Managers
{
    public enum MySQLStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Failed
    }

    public class MySQLManager
    {
        private Process? mysqlProcess;

        private readonly string mysqlDirectory;
        private readonly string mysqlExecutable;

        private const int mysqlPort = 3306;

        public MySQLStatus Status { get; private set; } =
            MySQLStatus.Stopped;

        public int Port =>
            mysqlPort;

        public string Version =>
            "8.4.11";

        public event EventHandler? StatusChanged;

        public bool IsRunning =>
            mysqlProcess != null &&
            !mysqlProcess.HasExited;

        public MySQLManager()
        {
            mysqlDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "runtimes",
                "mysql",
                "8.4.11"
            );

            mysqlExecutable = Path.Combine(
                mysqlDirectory,
                "bin",
                "mysqld.exe"
            );
        }

        public async Task StartAsync()
        {
            if (IsRunning ||
                Status == MySQLStatus.Starting)
                return;

            SetStatus(MySQLStatus.Starting);

            if (!File.Exists(mysqlExecutable))
            {
                SetStatus(MySQLStatus.Failed);

                throw new FileNotFoundException(
                    "MySQL server executable was not found.",
                    mysqlExecutable
                );
            }

            mysqlProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = mysqlExecutable,
                    Arguments = $"--datadir=\"{DataDirectory}\"",
                    WorkingDirectory = mysqlDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                mysqlProcess.Start();

                await WaitForServerAsync();

                SetStatus(MySQLStatus.Running);
            }
            catch
            {
                SetStatus(MySQLStatus.Failed);

                mysqlProcess?.Dispose();
                mysqlProcess = null;

                throw;
            }
        }


        // ============================================================
        // RESET MYSQL
        // ============================================================

        public async Task ResetAsync()
        {
            if (IsRunning)
            {
                Stop();
            }

            await StartAsync();
        }


        // ============================================================
        // STOP MYSQL
        // ============================================================

        public void Stop()
        {
            if (Status == MySQLStatus.Stopped)
                return;

            if (Status == MySQLStatus.Stopping)
                return;

            SetStatus(MySQLStatus.Stopping);

            string mysqlAdminExecutable = Path.Combine(
                mysqlDirectory,
                "bin",
                "mysqladmin.exe"
            );

            try
            {
                if (File.Exists(mysqlAdminExecutable))
                {
                    using Process shutdownProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = mysqlAdminExecutable,
                            Arguments =
                                $"--basedir=\"{mysqlDirectory}\" --datadir=\"{DataDirectory}\" shutdown",
                            WorkingDirectory = mysqlDirectory,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    shutdownProcess.Start();

                    // Do not hang WAMP-DS closing forever
                    shutdownProcess.WaitForExit(5000);
                }

                if (mysqlProcess != null &&
                    !mysqlProcess.HasExited)
                {
                    // Give MySQL time to shut down cleanly
                    if (!mysqlProcess.WaitForExit(10000))
                    {
                        // Force termination if it refuses
                        mysqlProcess.Kill(true);
                        mysqlProcess.WaitForExit(5000);
                    }
                }

                mysqlProcess?.Dispose();
                mysqlProcess = null;

                SetStatus(MySQLStatus.Stopped);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"MySQL shutdown error: {ex.Message}"
                );

                mysqlProcess?.Dispose();
                mysqlProcess = null;

                SetStatus(MySQLStatus.Failed);
            }
        }

        private async Task WaitForServerAsync()
        {
            const int timeout = 30;
            const int delay = 500;

            for (int i = 0; i < timeout * 1000 / delay; i++)
            {
                if (mysqlProcess != null &&
                    mysqlProcess.HasExited)
                {
                    throw new Exception(
                        "MySQL stopped unexpectedly while starting."
                    );
                }

                if (await IsPortOpenAsync())
                    return;

                await Task.Delay(delay);
            }

            throw new TimeoutException(
                "MySQL did not become ready within the expected time."
            );
        }

        private async Task<bool> IsPortOpenAsync()
        {
            try
            {
                using TcpClient client = new TcpClient();

                await client.ConnectAsync(
                    "127.0.0.1",
                    mysqlPort
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SetStatus(MySQLStatus status)
        {
            if (Status == status)
                return;

            Status = status;

            StatusChanged?.Invoke(
                this,
                EventArgs.Empty
            );
        }

        public string MySQLDirectory =>
            mysqlDirectory;

        public string ConfigurationFile =>
            Path.Combine(
                mysqlDirectory,
                "my.ini"
            );

        public string DataDirectory =>
            Path.Combine(
                mysqlDirectory,
                "data"
            );
    }
}