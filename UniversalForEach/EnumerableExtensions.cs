using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Rosebyte.UniversalForEach.Internal;

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
                new Scheduler<T>(elements, action, ready, threads).Run();
            }
        }
        
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action, Func<T, IEnumerable<T>> depends, 
            int threads = 1)
        {
            var tree = source.ToDictionary(x => x, x => depends(x).ToList());
            CircularityRule.Test(tree);
            tree = DependencyReductionFilter.Filter(tree);
            
            var elements = new ConcurrentDictionary<T, bool>(tree.Keys.ToDictionary(x => x, x => false));
            
            elements.Keys.ForEach(x => {action(x);elements[x] = true;}, x => tree[x].All(y => elements[y]), threads);
        }
    }
}