using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Telimena.Client
{
    public class TelimenaException : Exception
    {
        public TelimenaException(string message, Exception innerException, params KeyValuePair<Type, object>[]  requestObjects ) : base(message,innerException)
        {
            this.RequestObjects = requestObjects;
            if (innerException is AggregateException exception)
            {
                this.InnerExceptions = exception.InnerExceptions;
            }
            else
            {
                this.InnerExceptions = new ReadOnlyCollection<Exception>(new []{innerException});
            }
        }
        public KeyValuePair<Type, object>[] RequestObjects { get; }

        public ReadOnlyCollection<Exception> InnerExceptions { get;  }
    }
}