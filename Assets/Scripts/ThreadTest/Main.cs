using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Main:MonoBehaviour
{
    private Thread t1, t2, t3, t4, t5, t6;
    private static Queue<Task> q1 = new Queue<Task>();
    private static Queue<Task> q2 = new Queue<Task>();
    private static Queue<Task> q3 = new Queue<Task>();
    private static Queue<Task> q4 = new Queue<Task>();
    private object l1 = new object();
    private object l2 = new object();
    private object l3 = new object();
    private object l4 = new object();
    private object l5 = new object();
    private object l6 = new object();

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
            t1.Start();
        }
        if(t2 == null)
        {
            t2 = new Thread(new ThreadStart(Run2));
            t2.Start();
        }
        if(t3 == null)
        {
            t3 = new Thread(new ThreadStart(Run3));
            t3.Start();
        }
        if(t4 == null)
        {
            t4 = new Thread(new ThreadStart(Run4));
            t4.Start();
        }
        if(t5 == null)
        {
            t5 = new Thread(new ThreadStart(Run5));
            t5.Start();
        }
        if(t6 == null)
        {
            t6 = new Thread(new ThreadStart(Run6));
            t6.Start();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("in----------------------------------------------------------------------");
        if(q1.Count > 0)
        {
            Task task;
            lock(l1)
            {
                task = q1.Dequeue();
                Debug.Log(task.type + " dequeue " + task.index);
            }
        }
        if(q2.Count > 0)
        {
            Task task;
            lock(l2)
            {
                task = q2.Dequeue();
                Debug.Log(task.type + " dequeue " + task.index);
            }
        }
        if(q3.Count > 0)
        {
            Task task;
            lock(l3)
            {
                task = q3.Dequeue();
                Debug.Log(task.type + " dequeue " + task.index);
            }
        }
        if(q4.Count > 0)
        {
            Task task;
            lock(l4)
            {
                task = q4.Dequeue();
                Debug.Log(task.type + " dequeue " + task.index);
            }
        }
        Debug.Log("out----------------------------------------------------------------------");
    }

    void Run1()
    {
        int i = 0;
        while(true)
        {
            Task task = new Task(1, i);
            lock(l1)
            {
                q1.Enqueue(task);
            }
            i++;
            Debug.Log("1 enqueue");
        }
    }


    void Run2()
    {
        int i = 0;
        while(true)
        {
            Task task = new Task(2, i);
            lock(l2)
            {
                q2.Enqueue(task);
            }
            i++;
            while(true)
                Debug.Log("2 enqueue");
        }
    }

    void Run3()
    {
        int i = 0;
        while(true)
        {
            Task task = new Task(3, i);
            lock(l3)
            {
                q3.Enqueue(task);
            }
            i++;
            while(true)
                Debug.Log("3 enqueue");
        }
    }

    void Run4()
    {
        int i = 0;
        while(true)
        {
            Task task = new Task(4, i);
            lock(l4)
            {
                q4.Enqueue(task);
            }
            i++;
            while(true)
                Debug.Log("4 enqueue");
        }
    }

    void Run5()
    {
        while(true)
            Debug.Log("5");
    }

    void Run6()
    {
        while(true)
            Debug.Log("6");
    }
}
