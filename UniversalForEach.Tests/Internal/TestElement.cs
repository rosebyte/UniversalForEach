using System;

namespace RoseByte.UniversalForEach.Tests.Internal
{
    public class TestElement
    {
        public string Name { get; }
        public DateTime Finished { get; set; } = DateTime.MinValue;

        public TestElement(string name)
        {
            Name = name;
        }
    }
}