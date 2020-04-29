using System.Collections.Generic;

public class TaskQueue
{
    private Queue<Task> _tasks;
    public Queue<Task> Tasks
    {
        set
        {
            _tasks = value;
        }
        get
        {
            return _tasks;
        }
    }
    private object _lock;
    public object Lock
    {
        get
        {
            return _lock;
        }
    }

    public TaskQueue()
    {
        _tasks = new Queue<Task>();
        _lock = new object();
    }
}
