using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ClientAgent
{
    public static class CPU_Diagnostic
    {
        private static PerformanceCounter cpuCounter; // globaler PerformanceCounter 

        public static void InitialisierePerformanceCounter() // Initialisieren
        {
            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total"; // "_Total" entspricht der gesamten CPU Auslastung, Bei Computern mit mehr als 1 logischem Prozessor: "0" dem ersten Core, "1" dem zweiten...
        }

        public static int GetCPUusage() // Liefert die aktuelle Auslastung zurück
        {
            double d = Math.Round(cpuCounter.NextValue()) ;
            return (int)d;
        }
    }
}
