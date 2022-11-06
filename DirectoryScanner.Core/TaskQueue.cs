using System.Collections.Concurrent;

namespace DirectoryScanner.Core;

public class TaskQueue : IDisposable
{
    private readonly Queue<Action?> _tasks = new();
    private readonly List<Thread> _threads = new();
    private readonly CancellationTokenSource _tokenSource;

    public TaskQueue(ushort maxCount, CancellationTokenSource tokenSource)
    {
        for (var i = 0; i < maxCount; i++)
        {
            var thread = new Thread(ThreadFunc)
            {
                IsBackground = true
            };

            _threads.Add(thread);
            thread.Start();
        }
        _tokenSource = tokenSource;
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

    private void EnqueueTask(Action? task)
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
            while (_tasks.Count == 0)
            {
                Monitor.Wait(_tasks);
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