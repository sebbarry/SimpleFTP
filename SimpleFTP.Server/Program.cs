using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SimpleFTP.Server;

public class SimpleFTPServer
{

    private static int CONTROL_PORT = 22;
    private static int DATA_PORT = 21;
    private const int BUFFSIZE = 32;
    private static TcpListener listener;

    private static ClientConnection clientConnection;

    public static void Main(string[] args)
    {

        try
        {
            listener = new TcpListener(IPAddress.Any, CONTROL_PORT);
            listener.Start();
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }


        byte[] rcvBuffer = new byte[BUFFSIZE];
        int bytesRcvd;
        string data = "";


        for(;;)
        {
            TcpClient client = null;
            clientConnection = new ClientConnection(client, listener);
        }

    }



}
