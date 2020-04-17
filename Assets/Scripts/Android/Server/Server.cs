using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Server
{
    private static string _ip = "127.0.0.1";
    private static IPAddress _ipAddress;
    private static IPEndPoint _ipEndPoint;
    private static Socket _listener;
    private static Socket _socket;

    private static Thread _thread;
    private static bool _isThreadRun;

    private static byte[] bytes = new byte[1024];

    public static void StartServer()
    {
        Listen();

        _thread = new Thread(new ThreadStart(RunServer));
        _isThreadRun = true;
        _thread.Start();
    }

    private static void Listen()
    {
        _ipAddress = IPAddress.Parse(_ip);
        _ipEndPoint = new IPEndPoint(_ipAddress, 8080);

        _listener = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(_ipEndPoint);
        _listener.Listen(2);
    }

    private static void RunServer()
    {
        while(_isThreadRun)
        {
            Debug.Log("연결 대기중");
            _socket = _listener.Accept();

            // 수신
            while(true)
            {
                _socket.Receive(bytes);
                Debug.Log(bytes[0]);
                // 클라이언트로부터 데이터를 수신했다면
                if(bytes[0] == 2)
                    break;
            }

            // 송신
            byte[] msg = new byte[1];
            msg[0] = 4;
            _socket.Send(msg);
            //_socket.Shutdown(SocketShutdown.Both);
            //_socket.Close();
        }
    }

    public static void CloseServer()
    {
        _isThreadRun = false;
    }
}
