using System.Linq;
using System.Threading.Tasks;
using UniversalForEach;

namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action, int threads = 1)
        {
            if (threads == 1)
            {
                foreach (var element in source)
                {
                    action(element);
                }
            }
            else
            {
                Parallel.ForEach(source, new ParallelOptions {MaxDegreeOfParallelism = threads}, action);
            }
        }
        
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action, Func<T, bool> ready, int threads = 1)
        {
            var elements = source.ToList();
            
            if (threads == 1)
            {
                while (true)
                {
                    var element = elements.FirstOrDefault(ready);

                    if (element == null)
                    {
                        if (elements.Any())
                        {
                            throw new Exception("Unresolvable order of elements.");
                        }
                        
                        break;
                    }

                    action(element);
                    elements.Remove(element);
                }
            }
            else
            {
                new Scheduler<T>().Run(elements, action, ready, threads);
            }
        }
    }
}