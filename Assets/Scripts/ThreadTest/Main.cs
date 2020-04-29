using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Main:MonoBehaviour
{
    private Thread t1, t2, t3, t4, t5;
    //private static Queue<Task> q1 = new Queue<Task>();
    //private static Queue<Task> q2 = new Queue<Task>();
    //private static Queue<Task> q3 = new Queue<Task>();
    //private static Queue<Task> q4 = new Queue<Task>();
    private static Queue<int> q1 = new Queue<int>();
    private static Queue<int> q2 = new Queue<int>();
    private static Queue<int> q3 = new Queue<int>();
    private static Queue<int> q4 = new Queue<int>();

    private object l1 = new object();
    private object l2 = new object();
    private object l3 = new object();
    private object l4 = new object();
    private bool isQuit = false;
    int frame = 0;

    private class Task
    {
        public int type;
        public int index;

        public Task(int type, int index)
        {
            this.type = type;
            this.index = index;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(t1 == null)
        {
            t1 = new Thread(new ThreadStart(Run1));
            t1.Priority = System.Threading.ThreadPriority.Highest;
            t1.Start();
        }
        if(t2 == null)
        {
            t2 = new Thread(new ThreadStart(Run2));
            t2.Priority = System.Threading.ThreadPriority.Highest;
            t2.Start();
        }
        if(t3 == null)
        {
            t3 = new Thread(new ThreadStart(Run3));
            t3.Priority = System.Threading.ThreadPriority.Lowest;
            t3.Start();
        }
        if(t4 == null)
        {
            t4 = new Thread(new ThreadStart(Run4));
            t4.Priority = System.Threading.ThreadPriority.Lowest;
            t4.Start();
        }
        //if(t5 == null)
        //{
        //    t5 = new Thread(new ThreadStart(Run5));
        //    t5.Priority = System.Threading.ThreadPriority.Lowest;
        //    t5.Start();
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if(q4.Count == 0)
            return;

        frame++;
        q4.Dequeue();
        Debug.Log("Yes Thread" + frame);

        //for(int i = 0; i < 100; i++)
        //{
        //    num[0]++;
        //    if(num[0] == 300)
        //    {
        //        for(int j = 0; j < 6; j++)
        //        {
        //            Debug.Log(num[j]);
        //        }
        //        return;
        //    }
        //}

        ////Debug.Log("6");
    }

    void OnApplicationQuit()
    {
        isQuit = true;
    }

    void Run1()
    {
        while(!isQuit)
        {
            int a = 0;

            for(int i = 0; i < 200000000; i++)
            {
                a++;
            }

            lock(l1)
            {
                q1.Enqueue(1);
            }
        }
    }


    void Run2()
    {
        while(!isQuit)
        {
            if(q1.Count > 0)
            {
                lock(l1)
                {
                    q1.Dequeue();
                }

                int a = 0;

                for(int i = 0; i < 200000000; i++)
                {
                    a++;
                }

                lock(l2)
                {
                    q2.Enqueue(2);
                }
            }
        }
    }

    void Run3()
    {
        while(!isQuit)
        {
            if(q2.Count > 0)
            {
                lock(l2)
                {
                    q2.Dequeue();
                }

                int a = 0;

                for(int i = 0; i < 200000000; i++)
                {
                    a++;
                }

                lock(l3)
                {
                    q3.Enqueue(3);
                }
            }
        }
    }

    void Run4()
    {
        while(!isQuit)
        {
            if(q3.Count > 0)
            {
                lock(l3)
                {
                    q3.Dequeue();
                }

                int a = 0;

                for(int i = 0; i < 200000000; i++)
                {
                    a++;
                }

                lock(l4)
                {
                    q4.Enqueue(2);
                }
            }
        }
    }

    //void Run5()
    //{
    //    while(!isQuit)
    //    {
    //        if(q4.Count > 0)
    //        {
    //            lock(l4)
    //            {
    //                q4.Dequeue();
    //            }

    //            int a = 0;

    //            for(int i = 0; i < 10000; i++)
    //            {
    //                a++;
    //            }

    //            lock(l5)
    //            {
    //                q5.Enqueue(5);
    //            }
    //        }
    //    }
    //}
}
