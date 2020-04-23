using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Client
{
    private static string _ip = "127.0.0.1";
    //private static string _ip = "192.168.0.28";
    private static IPAddress _ipAddress;
    private static IPEndPoint _remoteEP;
    private static Socket _socket;

    private static Thread _thread;
    private static bool _isThreadRun;

    public static void Setup()
    {
        _ipAddress = IPAddress.Parse(_ip);
        _remoteEP = new IPEndPoint(_ipAddress, 8080);

        _socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(_remoteEP);
        Debug.Log("서버에 접속");
        //Debug.Log("Socket connect " + _socket.RemoteEndPoint.ToString());

        _thread = new Thread(new ThreadStart(Run));
        _isThreadRun = true;
        _thread.Start();
    }

    public static void Send(byte[] data)
    {
        _socket.Send(data);
    }

    public static void Receive()
    {
        byte[] bytes = new byte[100];
        _socket.Receive(bytes, 100, SocketFlags.None);
        Debug.Log(bytes[0]);
    }

    private static void Run()
    {
        while(_isThreadRun)
        {
            
        }
    }

    public static void Close()
    {
        _isThreadRun = false;
        //_socket.Close();
    }
}
