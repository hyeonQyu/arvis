using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using UnityEngine.XR;

public class Client
{
    //private static string _ip = "127.0.0.1";
    private static string _ip = "192.168.0.18";
    private static IPAddress _ipAddress;
    private static IPEndPoint _remoteEP;
    private static Socket _socket;

    private static Thread _thread;
    public static bool IsConnected { private set; get; }

    private const int MaxDataLength = 1024;
    private const int Ack = 1;

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
        IsConnected = true;
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

            // ack 수신시까지 계속해서 전송
            byte[] ack = new byte[1];
            ack[0] = 44;
            while(ack[0] != 1)
            {
                _socket.Send(trimData);
                _socket.Receive(ack, 1, SocketFlags.None);
            }

            index += MaxDataLength;
            restDataLength -= sendingLength;
        }
    }

    public static bool Receive(HandBoundary handBoundary)
    {
        byte[] bytes = new byte[16];
        int bytesRec = _socket.Receive(bytes, 16, SocketFlags.None);

        // 인식이 제대로 이루어지지 않음
        if(bytesRec == 1)
            return false;

        int[] datas = new int[4];
        for(int i = 0; i < 4; i++)
        {
            datas[i] = BitConverter.ToInt32(bytes, i * 4);
        }

        handBoundary.SetBoundary(datas);
        Debug.Log("Hand Boundary " + handBoundary.Left + " " + handBoundary.Right + " " + handBoundary.Top + " " + handBoundary.Bottom);
        return true;
    }

    public static void Close()
    {
        IsConnected = false;
        _socket.Close();
    }
}
