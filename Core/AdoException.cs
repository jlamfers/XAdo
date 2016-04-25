using System;
using System.Runtime.Serialization;

namespace XAdo.Core
{
    [Serializable]
    public class AdoException : Exception
    {

        public AdoException()
        {
        }

        public AdoException(string message) : base(message)
        {
        }

        public AdoException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AdoException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}