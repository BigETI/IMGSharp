using System;
using System.Runtime.Serialization;

/// <summary>
/// IMG sharp namespace
/// </summary>
namespace IMGSharp
{
    /// <summary>
    /// Invalid data exception class
    /// </summary>
    public class InvalidDataException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InvalidDataException() : base()
        {
            //
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public InvalidDataException(string message) : base(message)
        {
            //
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="innerException">Inner exception</param>
        public InvalidDataException(string message, Exception innerException) : base(message, innerException)
        {
            //
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public InvalidDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            //
        }
    }
}
