using System.Collections.Generic;
using System.Linq;

namespace Rosebyte.UniversalForEach.Internal
{
    internal static class DependencyReductionFilter
    {
        public static Dictionary<T, List<T>> Filter<T>(Dictionary<T, List<T>> tree)
        {
            tree.Where(x => !tree.Values.Any(y => y.Contains(x.Key)))
                .ForEach(x => Visit(x.Key, new Dictionary<T, T>(), tree));

            return tree;
        }

        private static void Visit<T>(T element, IDictionary<T, T> visited, IReadOnlyDictionary<T, List<T>> tree)
        {
            foreach (var dependency in tree[element].ToList())
            {
                if (visited.TryGetValue(dependency, out var value))
                {
                    tree[value].Remove(dependency);
                }
                
                visited[dependency] = element;
                
                Visit(dependency, new Dictionary<T, T>(visited), tree);
            }
        }
    }
}