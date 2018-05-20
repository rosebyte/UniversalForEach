using System;
using System.Collections.Generic;
using System.Linq;

namespace Rosebyte.UniversalForEach.Internal
{
    internal static class CircularityRule
    {
        public static void Test<T>(IEnumerable<T> elements, Func<T, IEnumerable<T>> depends)
        {
            Test(elements.ToDictionary(x => x, x => depends(x).ToList()));
        }

        public static void Test<T>(Dictionary<T, List<T>> tree)
        {
            if (!tree.Any())
            {
                return;
            }
			
            var leaves = tree.Where(x => !tree.Values.Any(y => y.Contains(x.Key))).ToList();

            if (!leaves.Any())
            {
                throw new ReferenceException("All dependencies are circular.");
            }
            
            leaves.ForEach(x => Test(x.Key, new Stack<T>(), tree));
        }

        private static void Test<T>(T element, Stack<T> stack, IReadOnlyDictionary<T, List<T>> tree)
        {
            if (stack.Contains(element))
            {
                var path = string.Join(", ", stack.Reverse().Skip(stack.Reverse().ToList().IndexOf(element)));
                throw new ReferenceException($"Circular reference: {path}.");
            }
            
            if (!tree.TryGetValue(element, out var value))
            {
                throw new ReferenceException($"Invalid reference: {element.ToString()}.");
            }
            
            stack.Push(element);
            
            value.ForEach(x => Test(x, stack, tree));
            
            stack.Pop();
        }
    }
}