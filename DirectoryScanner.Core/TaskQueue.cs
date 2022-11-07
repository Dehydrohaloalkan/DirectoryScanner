namespace DirectoryScanner.Core;

public class TaskQueue : IDisposable
{
    private readonly Queue<Action?> _tasks = new();
    private readonly List<Thread> _threads = new();
    private readonly CancellationTokenSource _tokenSource;
    private ushort _count;
    private ushort _maxCount;

    public TaskQueue(ushort maxCount, CancellationTokenSource tokenSource)
    {
        _tokenSource = tokenSource;
        _count = 0;
        _maxCount = maxCount;
        for (var i = 0; i < maxCount; i++)
        {
            var thread = new Thread(ThreadFunc)
            {
                IsBackground = true
            };

            _threads.Add(thread);
            thread.Start();
        }
    }

    private void ThreadFunc()
    {
        while (!_tokenSource.Token.IsCancellationRequested)
        {
            var task = DequeueTask();
            try
            {
                task?.Invoke();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    public void EnqueueTask(Action? task)
    {
        lock (_tasks)
        {
            _tasks.Enqueue(task);
            Monitor.Pulse(_tasks);
        }
    }

    private Action? DequeueTask()
    {
        lock (_tasks)
        {
            while (_tasks.Count == 0 && !_tokenSource.Token.IsCancellationRequested)
            {
                _count++;
                Monitor.Wait(_tasks);
                _count--;
            }
            try
            {
                return _tasks.Dequeue();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public void WaitEnd()
    {
        while (_count != _maxCount && !_tokenSource.Token.IsCancellationRequested) { }
    }

    public void Dispose()
    {
        _tokenSource.Cancel();
        Monitor.PulseAll(_tasks);
        foreach (var thread in _threads)
        {
            thread.Join();
        }
    }
}