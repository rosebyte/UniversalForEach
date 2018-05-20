using System.Collections.Generic;
using NUnit.Framework;
using Rosebyte.UniversalForEach.Internal;

namespace RoseByte.UniversalForEach.Tests.Internal
{
    [TestFixture]
    public class CircularityRuleTests
    {
        [Test]
		public void ShouldThrowThatTreeHasNoLeaves()
		{
			var tree = new Dictionary<string, List<string>>
			{
				{"A", new List<string>{"B"}},
				{"B", new List<string>{"C"}},
				{"C", new List<string>{"A"}}
			};

			Assert.That(
				() => CircularityRule.Test(tree),
				Throws.Exception.TypeOf<ReferenceException>().With.Message.EqualTo("All dependencies are circular."));
		}

		[Test]
		public void ShouldThrowCircularReference()
		{
			var tree = new Dictionary<string, List<string>>
			{
				{"A", new List<string>{"B"}},
				{"B", new List<string>{"C"}},
				{"C", new List<string>{"A"}},
				{"D", new List<string>{"C"}}
			};

			Assert.That(
				() => CircularityRule.Test(tree),
				Throws.Exception.TypeOf<ReferenceException>().With.Message.EqualTo("Circular reference: C, A, B."));
		}

		[Test]
		public void ShouldThrowInvalidReference()
		{
			var tree = new Dictionary<string, List<string>>
			{
				{"A", new List<string>{"B"}},
				{"B", new List<string>{"C"}},
				{"C", new List<string>{"D"}}
			};

			Assert.That(
				() => CircularityRule.Test(tree),
				Throws.Exception.TypeOf<ReferenceException>().With.Message.EqualTo("Invalid reference: D."));
		}

		[Test]
		public void ShouldThrowSelfReference()
		{
			var tree = new Dictionary<string, List<string>>
			{
				{"A", new List<string>{"B"}},
				{"B", new List<string>{"C"}},
				{"C", new List<string>{"C"}}
			};

			Assert.That(
				() => CircularityRule.Test(tree),
				Throws.Exception.TypeOf<ReferenceException>().With.Message.EqualTo("Circular reference: C."));
		}

		[Test]
		public void ShouldPassTreeEdge()
		{
			var tree = new Dictionary<string, List<string>>
			{
				{"A", new List<string>{"B"}},
				{"B", new List<string>{"C"}},
				{"C", new List<string>{"D"}},
				{"D", new List<string>()}
			};

			Assert.That(() => CircularityRule.Test(tree), Throws.Nothing);
		}

		[Test]
		public void ShouldPassForwardEdge()
		{
			var tree = new Dictionary<string, List<string>>
			{
				{"A", new List<string>{"B", "D"}},
				{"B", new List<string>{"C"}},
				{"C", new List<string>{"D"}},
				{"D", new List<string>()}
			};

			Assert.That(() => CircularityRule.Test(tree), Throws.Nothing);
		}

		[Test]
		public void ShouldPassCrossEdge()
		{
			var tree = new Dictionary<string, List<string>>
			{
				{"A", new List<string>{"B", "C"}},
				{"B", new List<string>{"D"}},
				{"C", new List<string>{"D"}},
				{"D", new List<string>()}
			};

			Assert.That(() => CircularityRule.Test(tree), Throws.Nothing);
		}
		
		[Test]
		public void ShouldPassCrossLevelEdge()
		{
			var tree = new Dictionary<string, List<string>>
			{
				{"A", new List<string>()},
				{"B", new List<string>{"A"}},
				{"C", new List<string>{"B"}},
				{"D", new List<string>{"A", "C"}}
			};
			
			var expected = new Dictionary<string, List<string>>
			{
				{"A", new List<string>{"B", "D"}},
				{"B", new List<string>{"C"}},
				{"C", new List<string>{"D"}},
				{"D", new List<string>()}
			};

			Assert.That(() => CircularityRule.Test(tree), Throws.Nothing);
		}
    }
}