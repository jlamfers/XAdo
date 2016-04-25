using System;
using System.Runtime.Serialization;

namespace XAdo.Core
{
    [Serializable]
    public class AdoBindingException : AdoException
    {

        public AdoBindingException()
        {
        }

        public AdoBindingException(string message) 
            : base(message)
        {
        }

        public AdoBindingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected AdoBindingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}