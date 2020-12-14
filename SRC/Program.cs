using System;

namespace Application
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var watcher = new Watcher.Watchdog())
            {
                if (!watcher.IsRestarted)
                    watcher.StartWatcher();


                Console.WriteLine("Press ENTER to stop watchdog");
                Console.ReadLine();

               // watcher.StopWatcher();
            } // Stop watchdog on leave this
                

            
        }
    }
}
