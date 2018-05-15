using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UniversalForEach
{
    public class Scheduler<T>
	{
		private readonly BlockingCollection<T> _tasks = new BlockingCollection<T>();
		private Dictionary<T, bool> _commands;
		private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
		private Action<T> _action;
		private Func<T, bool> _ready;
		private Task[] _workers;
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
				catch (Exception exception)
				{
					if (!(exception is OperationCanceledException))
					{
						_exception = exception;
					}
					
					break;
				}
			}
		}

		private void Cancel()
		{
			_cancellationSource.Cancel(false);
			Task.WaitAll(_workers);
			if (_exception != null)
			{
				throw new Exception("Child threw exception", _exception);
			}
		}

		private void Enqueue(T element)
		{
			_tasks.Add(element);
			_commands[element] = true;
		}
		
		public void Run(IEnumerable<T> commands, Action<T> action, Func<T, bool> ready, 
			int threads)
		{
			_commands = commands.ToDictionary(x => x, x => false);
			_workers = Enumerable.Range(1, Math.Min(threads, _commands.Count))
				.Select(x => Task.Factory.StartNew(Worker, _cancellationSource.Token))
				.ToArray();
			_action = action;
			_ready = ready;
			
			while (true)
			{
				if(_exception != null || _commands.All(x => x.Value))
				{
					break;
				}
				
				_commands.Where(x => !x.Value && _ready(x.Key)).Select(x => x.Key).ToList().ForEach(Enqueue);
			}

			Cancel();
		}
	}
}