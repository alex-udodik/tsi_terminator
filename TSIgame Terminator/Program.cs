using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Threading;

//created by shrimp.
namespace TSIgame_Terminator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            bool isGameRunning = false;

            while (true)
            {
                var allProcesses = Process.GetProcesses();

                List<Process> pubgProcesses = new List<Process>();
                Process mainGame = null;
                long mainGameMemSize = 0;


                foreach (var process in allProcesses)
                {
                    if (process.ProcessName.ToLower().Contains("tslgame"))
                    {
                        Console.WriteLine("Detected instance of PUBG running");
                        isGameRunning= true;
                        break;
                    }
                    else
                    {
                        //Console.WriteLine("No instance of PUBG running");
                        isGameRunning = false;
                    }
                }

                if (isGameRunning)
                {
                    Console.WriteLine("Waiting for game to initialize");
                    Thread.Sleep (30000);
                    foreach (var process in allProcesses)
                    {
                        string procName = process.ProcessName.ToLower();
                        string tslgame = "TslGame".ToLower();

                        if (procName.Contains(tslgame) && !procName.Contains("BE".ToLower()))
                        {

                            string instanceName = GetProcessInstanceName(process.Id);
                            if (instanceName == null || instanceName == "")
                            {
                                continue;
                            }

                            pubgProcesses.Add(process);
                            long process_size = 0;
                            var counter = new PerformanceCounter("Process", "Working Set - Private", instanceName, true);
                            process_size = Convert.ToInt64(counter.NextValue()) / 1024;

                            if (process_size > mainGameMemSize)
                            {
                                mainGameMemSize = process_size;
                                mainGame = process;
                            }


                            Console.WriteLine(process.ProcessName + " " + process.Id + " " + process_size);
                        }
                    }

                    if (mainGame != null)
                    {
                        foreach (var process in pubgProcesses)
                        {
                            if (process.Id != mainGame.Id)
                            {
                                var processToKill = Process.GetProcessById(process.Id);

                                Console.WriteLine("Attempting to kill lower memory task(s) of tslgame");
                                processToKill.Kill();
                                Console.WriteLine("Killed: " + processToKill.ProcessName + " PID: ");
                                Console.WriteLine("PID: " + processToKill.Id);

                            }
                        }
                    }

                    Console.WriteLine("Continuing scan");
                }
               
            }
            
        }

        public static string GetProcessInstanceName(int process_id)
        {
            PerformanceCounterCategory cat = new PerformanceCounterCategory("Process");
            string[] instances = cat.GetInstanceNames();
            
            foreach (string instance in instances)
            {
                using (PerformanceCounter cnt = new PerformanceCounter("Process", "ID Process", instance, true))
                {
                    if (instance.ToLower().Contains("tslgame"))
                    {
                        int val = (int)cnt.RawValue;
                        if (val == process_id)
                            return instance;
                    }
                }
            }
            throw new Exception("Could not find performance counter.");
        }
    }
}
