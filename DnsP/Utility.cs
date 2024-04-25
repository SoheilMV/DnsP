using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

internal static class Utility
{
    public static NetworkInterface GetNetworkInterface()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface networkInterface in networkInterfaces)
        {
            if (networkInterface.OperationalStatus == OperationalStatus.Up && (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
            {
                return networkInterface;
            }
        }
        throw new InvalidOperationException("Unable to find active local connection.");
    }

    public static void RunCommand(string command)
    {
        RunCommand(command, string.Empty);
    }

    public static void RunCommand(string command, string arguments)
    {
        RunCommand(command, arguments, false, false);
    }

    public static string RunCommand(string command, string arguments, bool redirectOutput, bool useShellExecute)
    {
        ProcessStartInfo processInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = redirectOutput,
            RedirectStandardError = false,
            UseShellExecute = useShellExecute,
            CreateNoWindow = true,
        };

        using (Process process = new Process())
        {
            process.StartInfo = processInfo;
            process.Start();
            if (redirectOutput)
            {
                process.WaitForExit();
                return process.StandardOutput.ReadToEnd();
            }
            else
                return string.Empty;
        }
    }

    public static bool IsRunAsAdmin()
    {
        bool isAdmin;
        try
        {
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(user);
            isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (UnauthorizedAccessException ex)
        {
            isAdmin = false;
        }
        catch (Exception ex)
        {
            isAdmin = false;
        }
        return isAdmin;
    }

    public static bool OpenUrl(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.OSDescription.Contains("microsoft-standard"))
            {
                try
                {
                    RunCommand(url);
                }
                catch
                {
                    RunCommand("cmd.exe", $"/c start {url.Replace("&", "^&")}");
                }

                return true;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                RunCommand("xdg-open", url);
                return true;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                RunCommand("open", url);
                return true;
            }
        }
        catch
        {
        }

        return false;
    }
}
