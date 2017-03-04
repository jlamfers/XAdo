using System;
using System.Runtime.Serialization;

namespace XAdo.Core
{
    [Serializable]
    public class XAdoBindingException : XAdoException
    {

        public XAdoBindingException()
        {
        }

        public XAdoBindingException(string message) 
            : base(message)
        {
        }

        public XAdoBindingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected XAdoBindingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}