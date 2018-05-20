using System;

namespace Rosebyte.UniversalForEach.Internal
{
    public class ReferenceException : Exception
    {
        public ReferenceException(string message) : base(message) { }
    }
}