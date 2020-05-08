using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

public class Client
{
    //private static string _ip = "127.0.0.1";
    private static string _ip = "192.168.0.18";
    private static IPAddress _ipAddress;
    private static IPEndPoint _remoteEP;
    private static Socket _socket;

    private static Thread _thread;
    private static bool _isThreadRun;

    private const int MaxDataLength = 1024;

    public static void Setup()
    {
        _ipAddress = IPAddress.Parse(_ip);
        _remoteEP = new IPEndPoint(_ipAddress, 4000);

        //_thread = new Thread(new ThreadStart(Run));
        //_isThreadRun = true;
        //_thread.Start();
    }

    public static void Connect()
    {
        _socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(_remoteEP);
        Debug.Log("서버에 접속");
    }

    public static void Send(byte[] data)
    {
        // jpg 크기 전송
        if(data.Length == 4)
        {
            _socket.Send(data);
            return;
        }

        // jpg 전송
        int index = 0;
        int restDataLength = data.Length;

        // 여러번에 걸쳐 jpg 1장 전송, 한 번 전송 최대 크기: 1024
        for(int i = 0; i < data.Length / MaxDataLength + 1; i++)
        {
            int sendingLength = Math.Min(MaxDataLength, restDataLength);

            byte[] trimData = new byte[sendingLength];
            Array.Copy(data, index, trimData, 0, sendingLength);
            _socket.Send(trimData);

            index += MaxDataLength;
            restDataLength -= sendingLength;
        }
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
        _socket.Close();
    }
}
