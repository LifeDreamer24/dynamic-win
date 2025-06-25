﻿using System.Diagnostics;
using DynamicWin.Rendering.Primitives;
using LibreHardwareMonitor.Hardware;

namespace DynamicWin.Interop;

internal class HardwareMonitor
{
    System.Timers.Timer timer;

    public static string usageString = " ";

    public static HardwareMonitor instance;

    public HardwareMonitor()
    {
        instance = this;

        timer = new System.Timers.Timer();
        timer.Interval = 1000;
        timer.Elapsed += Timer_Elapsed;

        computer = new Computer()
        {
            IsMemoryEnabled = true,
            IsCpuEnabled = true // Enable CPU monitoring
        };

        timer.Start();
    }

    Computer computer;
    float lastCpu = 0;
    string lastRam = "";

    private static long _iPrevCall;
    private static bool _isBusy;
    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_isBusy)
        {
            // Skip this time.
            Debug.WriteLine($"HardwareMonitor SKIPPED when called again during #{_iPrevCall}");
            return;
        }

        _isBusy = true;
        _iPrevCall++;
        var iCall = _iPrevCall;
        try
        {
            Debug.WriteLine($"HardwareMonitor BEFORE #{iCall}");

            computer.Open();

            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    if (hardware == null) continue;
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total")
                        {
                            lastCpu = MathRendering.LimitDecimalPoints((float)sensor.Value.GetValueOrDefault(), 1);
                        }
                    }
                }

                if (hardware.HardwareType == HardwareType.Memory)
                {
                    if (hardware == null) continue;
                    hardware.Update();

                    float memUsed = 0;
                    float memFree = 0;

                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.Name == "Memory Used")
                        {
                            memUsed = MathRendering.LimitDecimalPoints((float)sensor.Value.GetValueOrDefault(), 1);
                        }
                        else if (sensor.Name == "Memory Available")
                        {
                            memFree = MathRendering.LimitDecimalPoints((float)sensor.Value.GetValueOrDefault(), 1);
                        }
                        lastRam = memUsed + "GB / " + MathRendering.LimitDecimalPoints(memFree + memUsed, 0) + "GB";
                    }
                }
            }

            usageString = $"CPU: {lastCpu}%    RAM: {lastRam}";

            instance.computer.Close();

            Debug.WriteLine($"HardwareMonitor AFTER #{iCall}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            Debug.WriteLine($"HardwareMonitor EXCEPTION #{iCall}");
            return;
        }
        finally
        {
            _isBusy = false;
        }
    }

    public static void Stop()
    {
        if( instance.computer != null )
        {
            instance.computer.Close();
        }
    }
}