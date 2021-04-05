using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace tracert_utility
{
    class Tracert
    {
        const int TIMEOUT = 3000;
        const int MAX_HOPS = 30;
        const int PACKAGE_NUMBER = 3;

        public string GetDNS(EndPoint endPoint)
        {
            return Dns.GetHostEntry(IPAddress.Parse(endPoint.ToString().Split(':')[0])).HostName;
        }

        public void Start(string host_name, bool show_dns)
        {
            IPHostEntry ipHost;

            try
            {
                ipHost = Dns.GetHostByName(host_name);
            }
            catch (SocketException)
            {
                return;
            }

            IPEndPoint ipEnd = new IPEndPoint(ipHost.AddressList[0], 0);
            EndPoint endPoint = ipEnd;

            ICMP icmp_package = new ICMP();
            icmp_package.data_size = icmp_package.data.Length;
            icmp_package.GetCheckSum();

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, TIMEOUT);
            byte[] buffer;
            int ttl = 1;
            bool finish = false;

            Console.WriteLine("Трассировка маршрута к " + host_name + " с максимальным числом прыжков 30:");

            for (int i = 0; i < MAX_HOPS; i++)
            {
                int error_count = 0;
                Console.Write("{0, 2}", i + 1);
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ttl++);

                for (int j = 0; j < PACKAGE_NUMBER; j++)
                {
                    buffer = new byte[2048];
                    DateTime time_start = DateTime.Now;

                    try
                    {
                        socket.SendTo(icmp_package.ICMPToBytes(), icmp_package.data_size + 8, SocketFlags.None, ipEnd);
                        int received_bytes = socket.ReceiveFrom(buffer, ref endPoint);

                        TimeSpan response_time = DateTime.Now - time_start;

                        ICMP response = new ICMP(buffer, received_bytes);

                        if (response.type == 11)
                        {
                            Console.Write("{0, 10}", response_time.Milliseconds + "ms");
                        }

                        if (response.type == 0)
                        {
                            Console.Write("{0, 10}", response_time.Milliseconds + "ms");
                            finish = true;
                        }
                    }
                    catch (SocketException)
                    {
                        Console.Write("{0, 10}", "*");
                        error_count++;
                    }
                    icmp_package.Sequence();
                }

                if (error_count == 3)
                {
                    Console.WriteLine(" Превышен интервал ожидания для запроса.");
                }
                else if (show_dns)
                {
                    try
                    {
                        Console.WriteLine($" { GetDNS(endPoint) } [ { endPoint } ]");
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine($" { endPoint }");
                    }
                }
                else
                {
                    Console.WriteLine($" { endPoint }");
                }

                if (finish)
                {
                    Console.WriteLine("Трассировка завершена.");
                    return;
                }
            }
            socket.Close();
        }
    }
}
