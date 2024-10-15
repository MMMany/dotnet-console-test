using System;
using System.Diagnostics;
using System.Reflection;
using AdbLib;

namespace ConsoleTest;

internal class Program
{
  [STAThread]
  static void Main(string[] args)
  {
    Initialize();

    Adb.StartServer();
    Console.WriteLine("Hello World!");
    var devices = Adb.GetDevices();
    if (devices.Length == 0)
    {
      Console.WriteLine("no devices");
    }
    foreach (var dev in devices)
    {
      Console.WriteLine(string.Join('\n', [
        "[Device Info]",
        $"Name: {dev.Name}",
        $"Serial: {dev.Serial}",
        ""
      ]));
    }
  }

  private static void Initialize()
  {
    AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
  }

  private static void OnProcessExit(object? sender, EventArgs e)
  {
    Console.WriteLine("Program Exit");
    Adb.KillServer();
  }

  private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
  {
    Console.WriteLine("Occurred unhandled exception");
    if (e.ExceptionObject is Exception ex)
    {
      Console.WriteLine("Error: " + ex.Message);
      if (ex.StackTrace is string stackTrace)
      {
        Console.WriteLine("Call Stack\n" + stackTrace);
      }
    }
    Environment.Exit(1);
  }
}