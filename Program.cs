// See https://aka.ms/new-console-template for more information

using System.Text;
using TarkovDMATest.Tarkov;
using Vmmsharp;

namespace TarkovDMATest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Vmm vmm = new Vmm("-printf", "-device", "fpga://algo=0", "-waitinitialize");
            VmmProcess process = vmm.Process("EscapeFromTarkov.exe");
            Memory._process = process;

            if (process is null)
                Console.WriteLine("Process Not found");
            else
            {
                Console.WriteLine($"EscapeFromTarkov.exe is running at PID {process.PID}");
            }
            
            var unityBase = process.GetModuleBase("UnityPlayer.dll");
            
            if (unityBase == 0)
                Console.WriteLine("UnityBase Not found");
            else
            {
                Console.WriteLine($"Found UnityPlayer.dll at 0x{unityBase.ToString("x")}");
            }

            var game = new Game(unityBase);
        }

        public static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}