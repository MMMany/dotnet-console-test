using System.Diagnostics;
using System.Reflection;

namespace AdbLib;

public static class Adb
{
  public static void StartServer(bool restart = false)
  {
    Process proc = StartProcess("start-server");
    proc.WaitForExit();

    string response = proc.StandardOutput.ReadToEnd().Trim() ?? "";
    if (response.Length == 0)
    {
      if (restart)
      {
        KillServer();
        StartServer();
      }
    }
    else if (!response.Contains("daemon started successfully"))
    {
      throw new Exception("Failed start adb server");
    }
  }

  public static void KillServer()
  {
    Process proc = StartProcess("kill-server");
    proc.WaitForExit();

    string response = proc.StandardOutput.ReadToEnd().Trim() ?? "";
    if (response.Length != 0)
    {
      throw new Exception("Failed kill adb server");
    }
  }

  public static DeviceInfo[] GetDevices()
  {
    return GetDevicesAsync().Result;
  }

  public static Task<DeviceInfo[]> GetDevicesAsync()
  {
    Process proc = StartProcess("devices");
    proc.WaitForExit();

    string errors = proc.StandardError.ReadToEnd().Trim() ?? "";
    if (errors.Length > 0) Console.WriteLine("[ADB] Errors: " + errors);

    string response = proc.StandardOutput.ReadToEnd().Trim() ?? "";
    string[] parsed = response.Split("List of devices attached");
    List<DeviceInfo> devices = new(10);
    if (parsed.Length == 2)
    {
      string[] items = parsed.Last().Trim().Split('\n');
      foreach (string it in items)
      {
        string[] info = it.Split('\t');
        if (info.Length == 2)
        {
          devices.Add(new DeviceInfo(info[0], info[1]));
        }
      }
    }
    return Task.FromResult(devices.ToArray());
  }

  public static string Shell(string command)
  {
    return ShellAsync(command).Result;
  }

  public static Task<string> ShellAsync(string command)
  {
    Process proc = StartProcess($"shell \"{command}\"");
    proc.WaitForExit();

    string errors = proc.StandardError.ReadToEnd().Trim() ?? "";
    if (errors.Length > 0) Console.WriteLine("[ADB] Errors: " + errors);

    string response = proc.StandardOutput.ReadToEnd().Trim() ?? "";
    return Task.FromResult(response);
  }

  #region Private
  private static string _adbPath
  {
    get
    {
      var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      if (assemblyPath is null)
      {
        Console.WriteLine("Cannot found assembly path");
        throw new DirectoryNotFoundException("Cannot found assembly path");
      }
      var path = Path.Combine(assemblyPath, "platform-tools", "adb", "adb.exe");
      if (!File.Exists(path))
      {
        Console.WriteLine("There is no 'adb.exe' file");
        Console.WriteLine(path);
        throw new FileNotFoundException("There is no 'adb.exe' file");
      }
      return path;
    }
  }

  private static ProcessStartInfo MakeProcessStartInfo(string arguments)
  {
    return new()
    {
      FileName = _adbPath,
      Arguments = arguments,
      CreateNoWindow = true,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true
    };
  }

  private static Process StartProcess(ProcessStartInfo psi)
  {
    if (Process.Start(psi) is Process proc)
    {
      return proc;
    }
    throw new Exception("Failed open process");
  }
  private static Process StartProcess(string arguments)
  {
    return StartProcess(MakeProcessStartInfo(arguments));
  }
  #endregion // Private
}
