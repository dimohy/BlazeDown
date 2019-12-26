using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace BlazeDown.Server
{
    public class Program
    {
        public static IWebHost BlazeDownWebHost { get; private set; }

        public static void Main(string[] args)
        {
            BlazeDownWebHost = BuildWebHost(args);
            BlazeDownWebHost.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder()
                    .AddCommandLine(args)
                    .Build())
                .UseStartup<Startup>()
                .UseUrls("http://*:30002")
                .Build();
    }

    public class Daemon
    {
        public static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                var netDllPath = typeof(Program).Assembly.Location;
                var dllFileName = Path.GetFileName(netDllPath);
                var serviceName = "dotnet-" + Path.GetFileNameWithoutExtension(dllFileName).ToLower();
                switch (args[0])
                {
                    case "install":
                        InstallService(netDllPath, true);
                        return;
                    case "uninstall":
                        InstallService(netDllPath, false);
                        return;
                    case "start":
                        ControlService(serviceName, "start");
                        return;
                    case "stop":
                        ControlService(serviceName, "stop");
                        return;
                }
            }

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
            };


            Program.Main(args);
        }

        static int InstallService(string netDllPath, bool doInstall)
        {
            var serviceFile = @"
[Unit]
Description={0} running on {1}
[Service]
WorkingDirectory={2}
ExecStart={3} {4}
KillSignal=SIGINT
SyslogIdentifier={5}
[Install]
WantedBy=multi-user.target
";

            var dllFileName = Path.GetFileName(netDllPath);
            var osName = Environment.OSVersion.ToString();

            FileInfo fi = null;

            try
            {
                fi = new FileInfo(netDllPath);
            }
            catch { }

            if (doInstall == true && fi != null && fi.Exists == false)
            {
                WriteLog("NOT FOUND: " + fi.FullName);
                return 1;
            }

            var serviceName = "dotnet-" + Path.GetFileNameWithoutExtension(dllFileName).ToLower();

            var exeName = Process.GetCurrentProcess().MainModule.FileName;

            var workingDir = Path.GetDirectoryName(fi.FullName);
            var fullText = string.Format(serviceFile, dllFileName, osName, workingDir,
                    exeName, fi.FullName, serviceName);

            string serviceFilePath = $"/etc/systemd/system/{serviceName}.service";

            if (doInstall == true)
            {
                File.WriteAllText(serviceFilePath, fullText);
                WriteLog(serviceFilePath + " Created");
                ControlService(serviceName, "enable");
                ControlService(serviceName, "start");
            }
            else
            {
                if (File.Exists(serviceFilePath) == true)
                {
                    ControlService(serviceName, "stop");
                    File.Delete(serviceFilePath);
                    WriteLog(serviceFilePath + " Deleted");
                }
            }

            return 0;
        }

        static int ControlService(string serviceName, string mode)
        {
            string servicePath = $"/etc/systemd/system/{serviceName}.service";

            if (File.Exists(servicePath) == false)
            {
                WriteLog($"No service: {serviceName} to {mode}");
                return 1;
            }

            var psi = new ProcessStartInfo();
            psi.FileName = "systemctl";
            psi.Arguments = $"{mode} {serviceName}";
            var child = Process.Start(psi);
            child.WaitForExit();
            return child.ExitCode;
        }

        static void WriteLog(string text)
        {
            Console.WriteLine(text);
        }
    }
}

