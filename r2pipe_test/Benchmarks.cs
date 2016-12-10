using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace r2pipe_test
{
    public class Benchmarks
    {
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        public Benchmarks()
        {
            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }
        public float getCurrentCpuUsage(){
            return cpuCounter.NextValue();
        }
        public string getAvailableRAM(){
            return ramCounter.NextValue()+"MB";
        }
    }
}
