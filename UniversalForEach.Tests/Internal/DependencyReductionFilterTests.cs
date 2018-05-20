using System.Collections.Generic;
using NUnit.Framework;
using Rosebyte.UniversalForEach.Internal;

namespace RoseByte.UniversalForEach.Tests.Internal
{
    [TestFixture]
    public class DependencyReductionFilterTests
    {
        [Test]
		public void ShouldLeavePlainTree()
		{
			var tree = new Dictionary<string, List<string>>
			{
				{"A", new List<string>{"B"}},
				{"B", new List<string>{"C"}},
				{"C", new List<string>()}
			};

			var result = DependencyReductionFilter.Filter(tree);

			Assert.That(result["A"], Is.EquivalentTo(new List<string>{"B"}));
			Assert.That(result["B"], Is.EquivalentTo(new List<string>{"C"}));
			Assert.That(result["C"], Is.EquivalentTo(new List<string>()));
		}

	    [Test]
	    public void ShouldReduceRedundantNodes()
	    {
		    var tree = new Dictionary<string, List<string>>
		    {
			    {"A", new List<string>()},
			    {"B", new List<string>{"A"}},
			    {"C", new List<string>{"B"}},
			    {"D", new List<string>{"A", "C"}}
		    };

		    var result = DependencyReductionFilter.Filter(tree);

		    Assert.That(result["A"], Is.EquivalentTo(new List<string>()));
		    Assert.That(result["B"], Is.EquivalentTo(new List<string>{"A"}));
		    Assert.That(result["C"], Is.EquivalentTo(new List<string>{"B"}));
		    Assert.That(result["D"], Is.EquivalentTo(new List<string>{"C"}));
	    }
    }
}