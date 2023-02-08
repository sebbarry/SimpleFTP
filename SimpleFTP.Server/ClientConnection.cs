
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ClientConnection
{


    public string _userName;
    public TcpClient _controlClient;
    public IPEndPoint _dataEndpoint;
    public DataConnectionType _dataConnectionType = DataConnectionType.Active;
    public enum DataConnectionType
    {
        Passive,
        Active,
    }
    public enum TransferType
    {
        Ascii,
        Ebcdic,
        Image,
        Local,
    }

    public ClientConnection(TcpClient client, TcpListener listener)
    {
        NetworkStream stream = null;
        try {
            using( client = listener.AcceptTcpClient())
            {
                using( stream = client.GetStream() )
                {
                    HandleClient(client, stream);
                }
            }

        } catch(Exception e) {
            Console.WriteLine(e);
            stream.Close();
        }

    }

    public void HandleClient(TcpClient client, NetworkStream stream)
    {

        using(StreamWriter writer = new StreamWriter(stream, Encoding.ASCII))
        {
            using(StreamReader reader = new StreamReader(stream, Encoding.ASCII))
            {
                writer.WriteLine("220 Readery");
                writer.Flush();

                string line = null;
                while(!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    string[] command = line.Split(' ');

                    string cmd = command[0].ToUpperInvariant();

                    Console.WriteLine(cmd);

                    string arguments = command.Length > 1 ? line.Substring( command[0].Length + 1 ) : null ;
                    string response = null;

                    // switch on commands.
                    if(response == null)
                    {
                        switch(cmd)
                        {
                            case "USER":
                                response = "331 Username ok, need password";
                                break;
                            case "PASS":
                                response = "230 user logged in";
                                break;
                            case "CWD":
                                response = ChangeWorkingDirectory(arguments);
                                break;
                            case "PWD":
                                response = "257 \"/\" is current directory.";
                                break;
                            case "CDUP":
                                response = ChangeWorkingDirectory("..");
                                break;
                            case "PORT":
                                response = Port(arguments);
                                break;
                            case "PASSV":
                                response = Passive();
                                break;
                            case "TYPE":
                                string[] splitArgs = arguments.Split(" ");
                                response = Type(splitArgs[0], splitArgs.Length > 1 ? splitArgs[1] : null);
                                break;
                            case "QUIT":
                                response = "221 Service closing control";
                                break;

                            default:
                                response = "502 Command not implemented.";
                                break;
                        }
                    }

                    writer.WriteLine(response);
                    writer.Flush();
                }

            }

        }


    }



    #region FTP commands
    private string Passive()
    {
        _dataConnectionType = DataConnectionType.Passive;
        IPAddress localIp = ((IPEndPoint)_controlClient.Client.LocalEndpoint).Address;

        _passiveListener = new TcpListener(localIp, 0);
        _passiveListener.Stop();
        IPEndPoint passiveListenerEndpoint = (IPEndPoint)_passiveListener.LocalEndpoint;
        byte[] address = passiveListenerEndpoint.Address.GetAddressBytes();
        short port = (short)passiveListenerEndpoint.Port;
        byte[] portArray = BitConverter.GetBytes(port);

        if(BitConverter.IsLittleEndian)
            Array.Reverse(portArray);

        return string.Format("227 Entering Passive Mode ({0}, {1}, {2}, {3}, {4}, {5})", address[0], address[1], address[2], address[3], portArray[0], portArray[1]);
    }
    private string Type(string typeCode, string formatControl)
    {
        string response = "";
        switch (typeCode)
        {
            case "A":
                response = "200 OK";
                break;
            case "I":
            case "E":
            case "L":
            default:
                response = "504 Command not implemented for that parameter.";
                break;
        }

        if (formatControl != null)
        {
            switch (formatControl)
            {
                case "N":
                    response = "200 OK";
                    break;
                case "T":
                case "C":
                default:
                    response = "504 Command not implemented for that parameter.";
                    break;
            }
        }

        return response;
    }
    private static string User(string username)
    {
        //_userName = username;
        return "230 User logged in";
    }
    private static string Password(string password)
    {
        return "230 User logged in";
    }
    private static string ChangeWorkingDirectory(string pathname)
    {
        return "250 Changed to new directory";
    }
    private string Port(string hostPort)
    {
        _dataConnectionType = DataConnectionType.Active;
        string[] ipAndPort = hostPort.Split(',');
        byte[] ipAddress = new byte[4];
        byte[] port = new byte[2];
        for(int i = 0; i < 4; i++)
            ipAddress[i] = Convert.ToByte(ipAndPort[i]);
        for(int i = 4; i < 6; i++)
            port[i - 4] = Convert.ToByte(ipAndPort[i]);
        if(BitConverter.IsLittleEndian)
            Array.Reverse(port);
        _dataEndpoint = new IPEndPoint(new IPAddress(ipAddress), BitConverter.ToInt16(port, 0));
        return "200 Data Connection Established";
    }
    #endregion


}
