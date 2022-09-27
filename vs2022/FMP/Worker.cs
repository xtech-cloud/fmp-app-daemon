using System.Diagnostics;
using System.IO.Compression;
using System.Text;

namespace FMP
{
    public class Worker
    {
        public class AppSettings
        {
            public class _Kestrel
            {
                public class _Endpoints
                {
                    public class _Http
                    {
                        public string? Url { get; set; }
                    }
                    public class _Https
                    {
                        public string? Url { get; set; }
                    }
                    public _Http? Http { get; set; }
                    public _Https? Https { get; set; }
                }
                public _Endpoints? Endpoints { get; set; }
            }
            public _Kestrel? Kestrel { get; set; }
        }

        public Worker(string _name, string _target, string _dir)
        {
            name_ = _name;
            target_ = _target;
            dir_ = _dir;
        }

        private string name_ { get; set; }
        private string target_ { get; set; }
        private string dir_ { get; set; }
        private string? version_;
        private string? grpcEndpoint_;
        private string? grpcsEndpoint_;
        private string? exception_;

        private Process? process_;
        private Queue<string> outputQueue_ = new Queue<string>();
        private Queue<string> errorQueue_ = new Queue<string>();

        public string GetStatus()
        {
            string active = "active";
            if (null == process_)
            {
                active = "dead";
            }
            else if (process_.HasExited)
            {
                active = string.Format("dead (ExitCode={0})", process_.ExitCode);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("{0}", name_));
            sb.AppendLine(String.Format("    Active: {0}", active));
            sb.AppendLine(String.Format("    Loaded: {0}", target_));
            sb.AppendLine(String.Format("    Version: {0}", version_));
            sb.AppendLine(String.Format("    Grpc: {0}", grpcEndpoint_));
            sb.AppendLine(String.Format("    Grpcs: {0}", grpcsEndpoint_));
            if (null != exception_)
                sb.AppendLine(String.Format("    Exception: {0}", exception_));
            if (active.Contains("dead"))
            {
                sb.AppendLine("    Output:");
                foreach (var msg in outputQueue_)
                {
                    sb.AppendLine(String.Format("            {0}", msg));
                }
                sb.AppendLine("    Error:");
                foreach (var msg in errorQueue_)
                {
                    sb.AppendLine(String.Format("            {0}", msg));
                }
            }
            return sb.ToString();
        }

        public void Start()
        {
            Console.WriteLine(String.Format("Start {0} ......", name_));
            try
            {
                string targetpath = Path.Combine(dir_, target_);
                if (!File.Exists(targetpath))
                {
                    target_ = "Not Found";
                    return;
                }

                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(targetpath);
                version_ = fvi.ProductVersion;
                //version_ = fvi.FileVersion;
                string appsettingsJson = Path.Combine(dir_, "appsettings.json");
                if (!File.Exists(appsettingsJson))
                {
                    return;
                }

                var appsettings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(appsettingsJson));
                grpcEndpoint_ = appsettings?.Kestrel?.Endpoints?.Http?.Url;
                grpcEndpoint_ = grpcEndpoint_?.Replace("http://", "");
                grpcsEndpoint_ = appsettings?.Kestrel?.Endpoints?.Https?.Url;
                grpcsEndpoint_ = grpcsEndpoint_?.Replace("https://", "");

                process_ = new Process();
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "dotnet";
                psi.Arguments = targetpath;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.WorkingDirectory = dir_;

                process_.OutputDataReceived += new DataReceivedEventHandler(this.handleOutputDataReceived);
                process_.ErrorDataReceived += new DataReceivedEventHandler(this.handleErrorDataReceived);
                process_.StartInfo = psi;
                process_.Start();
                process_.BeginOutputReadLine();
                process_.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                exception_ = ex.Message;
                Console.Write(ex.Message);
            }
        }

        public void Stop()
        {
            if (null == process_)
                return;
            if (process_.HasExited)
                return;
            process_.CancelErrorRead();
            process_.CancelOutputRead();
            process_.Close();
            process_.Dispose();
        }

        private void handleOutputDataReceived(object _sender, DataReceivedEventArgs _args)
        {
            if (string.IsNullOrEmpty(_args.Data))
                return;
            if (outputQueue_.Count > 30)
                outputQueue_.Dequeue();
            outputQueue_.Enqueue(_args.Data);
        }

        private void handleErrorDataReceived(object _sender, DataReceivedEventArgs _args)
        {
            if (string.IsNullOrEmpty(_args.Data))
                return;
            if (errorQueue_.Count > 30)
                errorQueue_.Dequeue();
            errorQueue_.Enqueue(_args.Data);
        }
    }

    public class WorkerManager
    {

        private static List<Worker> workers = new List<Worker>();

        public static void Start()
        {
            try
            {
                string dir = Path.Combine(AppContext.BaseDirectory, "apps");
                if (!Directory.Exists(dir))
                    return;
                foreach (var subdir in Directory.GetDirectories(dir))
                {
                    string dir_name = Path.GetFileName(subdir);
                    string[] strs = dir_name.Split(".");
                    if (2 != strs.Length)
                        continue;
                    string target = string.Format("fmp-{0}-{1}-service-grpc.dll", strs[0].ToLower(), strs[1].ToLower());
                    Worker worker = new Worker(dir_name, target, subdir);
                    workers.Add(worker);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            foreach (var worker in workers)
            {
                worker.Start();
            }
        }

        public static void Stop()
        {
            foreach (var worker in workers)
            {
                worker.Stop();
            }
            workers.Clear();
        }

        public static void Restart()
        {
            Stop();
            Start();
        }

        public static void Upgrade()
        {
            Stop();
            string app_dir = Path.Combine(AppContext.BaseDirectory, "apps");
            Directory.Delete(app_dir, true);
            Directory.CreateDirectory(app_dir);

            string dir = Path.Combine(AppContext.BaseDirectory, "upgrades");
            if (!Directory.Exists(dir))
                return;
            foreach (var file in Directory.GetFiles(dir))
            {
                ZipFile.ExtractToDirectory(file, "./apps");
            }
        }

        public static string GetStatus()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("******************************************************");
            sb.AppendLine("FMP Daemon");
            sb.AppendLine("******************************************************");
            foreach (var worker in workers)
            {
                sb.Append(worker.GetStatus());
            }
            return sb.ToString();
        }
    }
}
