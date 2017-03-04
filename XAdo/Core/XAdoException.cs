using System;
using System.Runtime.Serialization;

namespace XAdo.Core
{
    [Serializable]
    public class XAdoException : Exception
    {

        public XAdoException()
        {
        }

        public XAdoException(string message) : base(message)
        {
        }

        public XAdoException(string message, Exception inner) : base(message, inner)
        {
        }

        protected XAdoException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}