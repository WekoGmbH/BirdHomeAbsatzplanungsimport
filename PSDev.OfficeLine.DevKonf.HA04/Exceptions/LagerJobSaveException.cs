using System;

namespace WEKO.BirdHome.Absatzplanungimport
{
    [Serializable]
    public class LagerJobSaveException : Exception
    {
        public LagerJobSaveException()
        {
        }

        public LagerJobSaveException(string message) : base(message)
        {
        }

        public LagerJobSaveException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LagerJobSaveException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}