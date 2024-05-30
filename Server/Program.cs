using System.Net.Sockets;

namespace Server
{
    class Program
    {
        const int portNo = 25000;

        static void Main(string[] args)
        {
            System.Net.IPAddress localAdd = System.Net.IPAddress.Parse("127.0.0.1");
            TcpListener listener = new TcpListener(localAdd, portNo); // listen at the local address
            listener.Start();

            while (true)
            {
                ChatClient user = new ChatClient(listener.AcceptTcpClient());
            }
        }
    }
}
