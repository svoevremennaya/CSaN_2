using System;

namespace tracert_utility
{
    class Program
    {
        static void Main(string[] args)
        {
            bool show_dns = true;
            string host_name;

            if (args.Length == 1)
            {
                host_name = args[0];
            }
            else if (args.Length == 2 && args[0] == "-d")
            {
                host_name = args[1];
                show_dns = false;
            }
            else
            {
                Console.Write("tracert ");
                host_name = Console.ReadLine();
            }

            Tracert tracert = new Tracert();
            tracert.Start(host_name, show_dns);
        }
    }
}
