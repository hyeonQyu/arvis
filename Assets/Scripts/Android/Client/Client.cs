using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Client
{
    private static string _ip = "127.0.0.1";
    private static IPAddress _ipAddress;
    private static IPEndPoint _remoteEP;
    private static Socket _socket;

    public static void Setup()
    {
        _ipAddress = IPAddress.Parse(_ip);
        _remoteEP = new IPEndPoint(_ipAddress, 8080);

        _socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(_remoteEP);
        Debug.Log("Socket connect " + _socket.RemoteEndPoint.ToString());
    }

    public static void Send()
    {
        byte[] bytes = new byte[100];
        bytes[0] = 2;
        _socket.Send(bytes);
    }

    public static void Receive()
    {
        byte[] bytes = new byte[100];
        _socket.Receive(bytes, 100, SocketFlags.None);
        // 안드로이드 폰에서 디버깅 해보기
        //if(bytes[0] == 4)
        Debug.Log(bytes[0]);
    }
}
