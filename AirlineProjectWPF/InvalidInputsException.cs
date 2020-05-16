using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AirlineProjectWPF
{
    public class InvalidInputsException : Exception
    {
        public InvalidInputsException()
        {
        }

        public InvalidInputsException(string message) : base(message)
        {
        }

        public InvalidInputsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidInputsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
