using System;

namespace fbognini.Core.Feedbacks
{
    public class ProcessFeedback: Feedback
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
