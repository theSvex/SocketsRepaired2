using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SocketsRepaired2
{
    class Program
    {
        static void Main(string[] args)
        {

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            Socket internalSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            byte[] recBuffer = new byte[256];
            List<int> lista = new List<int>();

            string selfId = "127.0.0.1:8888";
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));

            clientSocket.Connect("127.0.0.1", 8080);
            clientSocket.Send(Encoding.ASCII.GetBytes(CreateEmptyMessage()+"$"));


            while (true)
            {
                Console.WriteLine("Listening...");
                serverSocket.Listen(1);
                if (!internalSocket.Connected)
                {
                    internalSocket = serverSocket.Accept();
                    Console.WriteLine("Client connected!");
                }   
                internalSocket.Receive(recBuffer);
                string message = Encoding.ASCII.GetString(recBuffer);
                message = message.Substring(0, message.IndexOf('$'));
                Console.WriteLine(message);
                string[] protocol = message.Split(new char[] { '_' });
                //1 - from IP, 2 - action, 3 - index, 4 - int
                //2 - action: [A]dd, [D]elete, [S]how
                //*****PASS ALONG*******
                if(protocol[0] == "0" || protocol[0] == selfId)
                {
                    Console.WriteLine("\n New action (1) or empty message (2) ?");
                    string response = Console.ReadLine();
                    switch (int.Parse(response))
                    {
                        case 1:
                            {
                                Console.WriteLine("\nAction: 1.Add - 2.Delete - 3.Show");
                                int action = int.Parse(Console.ReadLine());
                                switch (action)
                                {
                                    case 1:
                                        {
                                            Console.Write("What add: ");
                                            int addAction = int.Parse(Console.ReadLine());
                                            Console.Write("\nWhat index: ");
                                            int addIndexAction = int.Parse(Console.ReadLine());
                                            lista.Insert(addIndexAction, addAction);
                                            message = CreateMessage(selfId, "A", addIndexAction, addAction);
                                            break;
                                        }
                                    case 2:
                                        {
                                            displayList(lista);
                                            Console.Write("What index delete: ");
                                            int deleteAction = int.Parse(Console.ReadLine());
                                            lista.RemoveAt(deleteAction);
                                            message = CreateMessage(selfId, "D", deleteAction, null);
                                            break;
                                        }
                                    case 3:
                                        {
                                            message = CreateMessage(selfId, "S", null, null);
                                            break;
                                        }
                                    default: Console.WriteLine("Pick one of 3 actions");
                                        break;
                                }
                                break;
                            }
                        case 2: message = CreateEmptyMessage(); break;
                        default: Console.WriteLine("Pick one action"); break;
                    }

                    //*******SEND NEW MESSAGE********
                    try
                    {
                        clientSocket.Connect("127.0.0.1", 8080);
                    }
                    catch (Exception ex) { }
                    clientSocket.Send(Encoding.ASCII.GetBytes(message + "$"));
                }
                else if (protocol[0] != selfId)
                {
                    switch (protocol[1])
                    {
                        case ("A"): lista.Insert(int.Parse(protocol[2]), int.Parse(protocol[3])); break;
                        case ("D"): lista.RemoveAt(int.Parse(protocol[2])); break;
                        case ("S"): displayList(lista); break;
                        default: break;
                    }

                    Console.WriteLine("message : " + message + " \thas been completed!\n Passing along!");

                    //*******SEND NEW MESSAGE********
                    try
                    {
                        clientSocket.Connect("127.0.0.1", 8080);
                    }
                    catch (Exception ex) { }
                    clientSocket.Send(Encoding.ASCII.GetBytes(message + "$"));
                }
            }
        }

        private static string CreateEmptyMessage()
        {
            return "0_0_0_0";
        }

        private static void displayList(List<int> lista)
        {
            for (int i = 0; i < lista.Count; i++)
            {
                Console.WriteLine("{0}. {1}\t", i, lista[i]);
            };
            Console.WriteLine();
        }

        private static string CreateMessage(string Id, string action, int? index, int? var)
        {
            return Id + "_" + action + "_" + (index != null ? index.ToString() : "null") + "_" + (var != null ? var.ToString() : "null");
        }
    }
}
