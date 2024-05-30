using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Server
{
    internal class ChatClient
    {
        public static Hashtable AllClients = new Hashtable(); // contains a list of all the clients

        // information about the client
        private TcpClient _client;
        private string _clientIP;
        private string _clientNick;

        private byte[] data; // used for sending/receiving data

        private bool ReceiveNick = true;

        public ChatClient(TcpClient client)
        {
            _client = client;

            _clientIP = client.Client.RemoteEndPoint.ToString(); // get the client IP address

            AllClients.Add(_clientIP, this); // add the current client to the hash table

            // start reading data from the client in a separate thread
            data = new byte[_client.ReceiveBufferSize];
            client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), ReceiveMessage, null);
        }

        public void ReceiveMessage(IAsyncResult ar)
        {
            // read from the client
            int bytesRead;
            try
            {
                lock (_client.GetStream())
                {
                    bytesRead = _client.GetStream().EndRead(ar);
                }

                // client has disconnected
                if (bytesRead < 1)
                {
                    AllClients.Remove(_clientIP);
                    Broadcast(_clientNick + " has left the chat.");
                    return;
                }
                else
                {
                    string messageReceived = System.Text.Encoding.ASCII.GetString(data, 0, bytesRead); // get the message sent

                    // client is sending its nickname
                    if (ReceiveNick)
                    {
                        _clientNick = messageReceived;

                        Broadcast(_clientNick + " has joined the chat."); // tell everyone client has entered the chat
                        ReceiveNick = false;
                    }
                    else
                    {
                        Broadcast(_clientNick + ">" + messageReceived); // broadcast the message to everyone
                    }
                }

                // continue reading from client
                lock (_client.GetStream())
                {
                    _client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), ReceiveMessage, null);
                }
            }
            catch (Exception ex)
            {
                AllClients.Remove(_clientIP);
                Broadcast(_clientNick + " has left the chat.");
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                System.Net.Sockets.NetworkStream ns; // send the text
                
                // lock is used to stop the network stream having access to multiple threads at once
                lock (_client.GetStream())
                {
                    ns = _client.GetStream();
                }

                byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(message);
                ns.Write(bytesToSend, 0, bytesToSend.Length);
                ns.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Broadcast(string message)
        {
            Console.WriteLine(message); // log it locally

            foreach (DictionaryEntry c in AllClients)
            {
                ((ChatClient)(c.Value)).SendMessage(message + Environment.NewLine); // broadcast message to all users
            }
        }
    }
}
