using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Altium.Core;

class TaskSet : IAsyncDisposable
{
    private readonly int _capacity;
    private readonly CancellationToken _cancel;
    private List<Task> _workingTasks;
    private SemaphoreSlim _semaphore = new(1);

    public TaskSet(int capacity, CancellationToken cancel)
    {
        _capacity = capacity;
        _cancel = cancel;
        _workingTasks = new(capacity);
    }

    public async Task WaitAndAdd(Func<Task> run)
    {
        await _semaphore.WaitAsync(_cancel);

        try
        {
            await WaitEmptySlot();
            _workingTasks.Add(run());
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task WaitEmptySlot()
    {
        _workingTasks = _workingTasks.Where(x => !x.IsCompleted).ToList();

        while (_workingTasks.Count >= _capacity)
        {
            _cancel.ThrowIfCancellationRequested();

            await Task.WhenAny(_workingTasks);
            _workingTasks = _workingTasks.Where(x => !x.IsCompleted).ToList();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_workingTasks == null)
            return;
        await Task.WhenAll(_workingTasks);
        _workingTasks = null;
    }
}