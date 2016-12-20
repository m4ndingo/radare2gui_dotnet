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
        private bool do_refresh = true;
        private float last_cpuUsage = 0;
        private float first_memUsed = -1;
        private float last_memUsed = 0;
        private int counter = 0;
        public Benchmarks()
        {
            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }
        public float getCurrentCpuUsage(){
            float current_cpuUsage = cpuCounter.NextValue();
            do_refresh = Math.Abs(last_cpuUsage - current_cpuUsage) > 40;
            if (do_refresh) counter = 10;
            last_cpuUsage = current_cpuUsage;
            return current_cpuUsage;
        }
        public string getAvailableRAM()
        {
            last_memUsed = ramCounter.NextValue();
            if (first_memUsed == -1)
                first_memUsed = last_memUsed;
            return last_memUsed + "MB";
        }
        public string getUsedRAM()
        {
            return (first_memUsed - last_memUsed).ToString() +"MB";
        }
        public Boolean shouldRefresh()
        {
            if (counter-- > 0) return true;
            return do_refresh;
        }
    }
}
