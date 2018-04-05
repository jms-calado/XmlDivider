using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace XmlDivider
{
    class Program
    {        
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        public static string watcherFolder = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
        static void Main(string[] args)
        {            
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Ignoring arguments (no arguments/incorrect arguments)");
            }
            else
            {
                if (args[0] == "-path" || args[0] == "-p")
                {
                    if (args[1] != String.Empty)
                    {
                        watcherFolder = args[1].ToString();
                        Console.WriteLine("Program Loaded with arguments: " + args[0] + " " + args[1]);
                    }
                    else
                    {
                        Console.WriteLine("2nd argument is missing");
                    }
                }
            }
            Console.WriteLine("Starting");
            Console.WriteLine(watcherFolder);
            FileAnalyser fileAnalyser = new FileAnalyser();
            Task.Run(()=>fileAnalyser.FileWatcher(watcherFolder));
            resetEvent.WaitOne();
        }
    }
}
