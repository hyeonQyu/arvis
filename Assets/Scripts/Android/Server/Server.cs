using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

public class Server
{
    private static string _ip = "127.0.0.1";
    //private static string _ip = "192.168.0.28";
    private static IPAddress _ipAddress;
    private static IPEndPoint _ipEndPoint;
    private static Socket _listener;
    private static Socket _socket;

    private static Thread _thread;
    private static bool _isThreadRun;

    private static byte[] _data = new byte[1024];

    // 기기로부터 들어온 프레임 이미지 큐
    public static Queue<byte[]> ImgQueue = new Queue<byte[]>();
    // 연산을 통해 얻은 손의 위치 좌표
    public static Queue<byte[]> PointQueue = new Queue<byte[]>();

    public static void Open()
    {
        _thread = new Thread(new ThreadStart(Run));
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

    private static void Run()
    {
        Listen();

        Debug.Log("연결 대기중");
        _socket = _listener.Accept();

        while(_isThreadRun)
        {
            int bytesRec = 0;

            // 수신
            while(true)
            {
                bytesRec = _socket.Receive(_data);
                
                // 클라이언트로부터 데이터를 수신했다면
                if(bytesRec > 0)
                    break;
            }
            Debug.Log("데이터 배열 크기: " + bytesRec);

            if(bytesRec < 10)
            {
                //byte[] width = new byte[4];
                //byte[] height = new byte[4];

                //for(int i = 0; i < 4; i++)
                //    width[i] = _data[i];
                //for(int i = 0; i < 4; i++)
                //    height[i] = _data[i + 4];

                HandTracker.Width = BitConverter.ToInt32(_data, 0);
                HandTracker.Height = BitConverter.ToInt32(_data, 4);
                continue;
            }

            // 받은 데이터를 큐에 삽입
            ImgQueue.Enqueue(_data);

            // 송신할 데이터가 있다면 송신
            if(PointQueue.Count > 0)
            {
                byte[] msg = new byte[1];
                msg[0] = 4;
                _socket.Send(msg);
            }           
        }
    }

    public static void Close()
    {
        _isThreadRun = false;
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }
}
