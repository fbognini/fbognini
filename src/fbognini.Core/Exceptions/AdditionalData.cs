namespace fbognini.Core.Exceptions
{
    public abstract class AdditionalData
    {
        public abstract string Entity { get; }
        public string Error { get; }

        protected AdditionalData(string error)
        {
            Error = error;
        }
    }
}
