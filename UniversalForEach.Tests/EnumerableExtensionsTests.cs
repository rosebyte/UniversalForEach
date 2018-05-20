using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using RoseByte.UniversalForEach.Tests.Internal;

namespace RoseByte.UniversalForEach.Tests
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        private readonly List<string> _names = new List<string>
        {
	        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S"
        };
	    
		private const int Sleep = 100;
	
		private static int Difference(TestElement left, TestElement right)
		{
			return (right.Finished - left.Finished).Milliseconds;
		}

	    private List<TestElement> GetElements(int count)
	    {
		    return _names.Take(count).Select(x => new TestElement(x)).ToList();
	    }

	    private void SleepAndSetTime(TestElement item)
	    {
		    Thread.Sleep(Sleep); 
		    item.Finished = DateTime.Now;
	    }

		[Test]
		public void ShouldRunOneThread()
		{
			var elements = GetElements(5);
			
			elements.ForEach(SleepAndSetTime, 1);

			Assert.That(Difference(elements[0], elements[1]), Is.GreaterThanOrEqualTo(Sleep));
			Assert.That(Difference(elements[1], elements[2]), Is.GreaterThanOrEqualTo(Sleep));
			Assert.That(Difference(elements[2], elements[3]), Is.GreaterThanOrEqualTo(Sleep));
			Assert.That(Difference(elements[3], elements[4]), Is.GreaterThanOrEqualTo(Sleep));
		}

		[Test]
		public void ShouldRunMultipleThreads()
		{
			var elements = GetElements(12);
			
			elements.ForEach(SleepAndSetTime, 12);
			
			Assert.That(Difference(elements.First(), elements.Last()), Is.LessThan(Sleep * elements.Count / 2));
		}
	    
	    [Test]
	    public void ShouldRunMultipleThreadsWithReady()
	    {
		    var elements = GetElements(24);
			
		    elements.ForEach(SleepAndSetTime, x => true, 4);
			
		    Assert.That(Difference(elements.First(), elements.Last()), Is.LessThan(Sleep * elements.Count / 2));
	    }

		[Test]
		public void ShouldWaitForReady()
		{
			var elements = GetElements(5);

			var dependencies = elements.ToDictionary(x => x, x => new List<TestElement>());
			dependencies[elements[0]] = new List<TestElement>(elements.Skip(1));
			dependencies[elements[1]] = new List<TestElement>{elements[2]};
			dependencies[elements[2]] = new List<TestElement>{elements[3]};
			dependencies[elements[3]] = new List<TestElement>{elements[4]};
			
			elements.ForEach(SleepAndSetTime, x => dependencies[x].All(y => y.Finished != DateTime.MinValue), 5);

			Assert.That(
				Difference(elements[4], elements[0]), 
				Is.GreaterThan(Sleep * (elements.Count - 1)),
				string.Join("\n", elements.Select(x => x.Finished.ToString("hh:mm:ss,ffff"))));
		}
	    
	    [Test]
	    public void ShouldWaitForDependencies()
	    {
		    var elements = GetElements(5);

		    var dependencies = elements.ToDictionary(x => x, x => new List<TestElement>());
		    dependencies[elements[0]] = new List<TestElement>(elements.Skip(1));
		    dependencies[elements[1]] = new List<TestElement>{elements[2]};
		    dependencies[elements[2]] = new List<TestElement>{elements[3]};
		    dependencies[elements[3]] = new List<TestElement>{elements[4]};
			
		    elements.ForEach(SleepAndSetTime, x => dependencies[x], 5);

		    Assert.That(Difference(elements[4], elements[0]), Is.GreaterThan(Sleep * (elements.Count - 1)));
	    }
	    
	    [Test]
	    public void ShouldThrowInRegress()
	    {
		    var elements = GetElements(5);

		    var dependencies = elements.ToDictionary(x => x, x => new List<TestElement>());
		    dependencies[elements[0]] = new List<TestElement>(elements.Skip(1));
		    dependencies[elements[1]] = new List<TestElement>{elements[2]};
		    dependencies[elements[2]] = new List<TestElement>{elements[3]};
		    dependencies[elements[3]] = new List<TestElement>{elements[4]};
		    dependencies[elements[4]] = new List<TestElement>{elements[0]};
			
		    Assert.That(
			    () => elements.ForEach(SleepAndSetTime, x => dependencies[x], 5), 
			    Throws.Exception.With.Message.EqualTo("All dependencies are circular."));
	    }
	}
}