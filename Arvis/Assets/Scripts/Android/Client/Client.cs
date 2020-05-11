using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using UnityEngine.XR;
using UnityEngine.UI;

public class Client
{
    //private static string _ip = "127.0.0.1";
    private static string _ip = "192.168.0.18";
    private static IPAddress _ipAddress;
    private static IPEndPoint _remoteEP;
    private static Socket _socket;

    private static Thread _thread;
    private static bool _isThreadRun;
    public static bool IsConnected { private set; get; }

    private const int MaxDataLength = 1024;
    private const byte Ack = 1;

    private static RawImage _display;

    private static HandDetector _handDetector;
    private static SkinDetector _skinDetector;

    public static void Setup()
    {
        _ipAddress = IPAddress.Parse(_ip);
        _remoteEP = new IPEndPoint(_ipAddress, 4000);

        _thread = new Thread(new ThreadStart(Run));

        ReceivedHandBoundary = new HandBoundary();
    }

    private static void Run()
    {
        while(_isThreadRun)
        {
            // 이미지를 JPG로 변환
            Texture2D img = (Texture2D)_display.texture;
            byte[] jpg = img.EncodeToJPG();
            Debug.Log("jpg " + jpg.Length);

            // jpg 전송
            Send(BitConverter.GetBytes(jpg.Length));
            Send(jpg);

            // 사각형 범위 수신, 제대로 수신하면 쓰레드 종료
            _handDetector.IsInitialized = Receive();
            _isThreadRun = !_handDetector.IsInitialized;
        }
        Close();
        Debug.Log("쓰레드 종료");
    }

    public static void Connect(RawImage display, HandDetector handDetector, SkinDetector skinDetector)
    {
        IsConnected = true;
        _socket = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(_remoteEP);
        Debug.Log("서버에 접속");

        _display = display;
        _handDetector = handDetector;
        _skinDetector = skinDetector;

        // Send 및 Receive 시작
        _isThreadRun = true;
        _thread.Start();
    }

    private static void Send(byte[] data)
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
            while(ack[0] != Ack)
            {
                _socket.Send(trimData);
                _socket.Receive(ack, 1, SocketFlags.None);
            }

            index += MaxDataLength;
            restDataLength -= sendingLength;
        }
    }

    private static bool Receive()
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

        _skinDetector.HandBoundary.SetBoundary(datas);
        Debug.Log("Hand Boundary " + _skinDetector.HandBoundary.Left + " " + _skinDetector.HandBoundary.Right + " " + _skinDetector.HandBoundary.Top + " " + _skinDetector.HandBoundary.Bottom);
        return true;
    }

    public static void Close()
    {
        IsConnected = false;
        _isThreadRun = false;
        _socket.Close();
        Debug.Log("소켓 닫음");
    }
}
