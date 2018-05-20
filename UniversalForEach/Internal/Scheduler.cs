using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rosebyte.UniversalForEach.Internal
{
    internal class Scheduler<T>
	{
		private readonly BlockingCollection<T> _tasks = new BlockingCollection<T>();
		private readonly ConcurrentDictionary<T, bool> _sheduled;
		private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
		private readonly ManualResetEvent _wake = new ManualResetEvent(false);
		private readonly Action<T> _action;
		private readonly Func<T, bool> _ready;
		private readonly Task[] _workers;
		private Exception _exception;

		private void Worker()
		{
			while (true)
			{
				try
				{
					var task = _tasks.Take(_cancellationSource.Token);
					_action(task);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception exception)
				{
					if (!(exception is OperationCanceledException))
					{
						_exception = exception;
					}

					break;
				}
				finally
				{
					_wake.Set();
				}
			}
		}
		
		public Scheduler(IEnumerable<T> commands, Action<T> action, Func<T, bool> ready, int threads)
		{
			_ready = ready;
			_sheduled = new ConcurrentDictionary<T, bool>(commands.ToDictionary(x => x, x => false));
			_action = action;
			_workers = Enumerable.Range(1, Math.Min(threads, _sheduled.Count))
				.Select(x => Task.Factory.StartNew(Worker, _cancellationSource.Token))
				.ToArray();
		}

		private void Enqueue(T task)
		{
			_sheduled[task] = true;
			_tasks.Add(task);
		}

		private void Cancel()
		{
			try
			{
				_cancellationSource.Cancel(false);
				Task.WaitAll(_workers);
				if (_exception != null)
				{
					throw new Exception("Child threw exception", _exception);
				}
			}
			catch (AggregateException exception)
			{
				if (!exception.InnerExceptions.All(x => x is TaskCanceledException))
				{
					throw;
				}
			}
		}
		
		public void Run()
		{
			while (_exception == null && _sheduled.Any(x => !x.Value))
			{
				_sheduled.Where(x => !x.Value && _ready(x.Key)).ForEach(x => Enqueue(x.Key));
				_wake.WaitOne();
			}

			while (_exception == null && _tasks.Any())
			{
				_wake.WaitOne();
			}
			
			Cancel();
		}
	}
}