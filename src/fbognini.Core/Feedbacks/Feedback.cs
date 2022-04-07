using System;

namespace fbognini.Core.Feedbacks
{
    public class Feedback
    {
        public Feedback()
        {
            Success = true;
        }

        public bool Success { get; set; }
        public Exception Exception { get; set; }
    }
}
