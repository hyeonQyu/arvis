using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class ThreadManager
{
    private Thread _thread;
    public Thread Thread
    {
        set
        {
            _thread = value;
        }
        get
        {
            return _thread;
        }
    }
    private int _ticket;
    private int _pass;
    private int _stride;
    public int Stride
    {
        get
        {
            return _stride;
        }
    }
    private const int Lcm = 3;

    public ThreadManager(int ticket)
    {
        _ticket = ticket;
        _pass = Lcm / _ticket;
    }

    public void IncreaseStride()
    {
        _stride += _pass;
    }
}
