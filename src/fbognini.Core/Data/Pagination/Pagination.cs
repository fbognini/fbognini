namespace fbognini.Core.Data.Pagination
{
    public class Pagination
    {
        //public static implicit operator Pagination(FormCriteria criteria)
        //{
        //    return new Pagination()
        //    {
        //        PageNumber = criteria.PageNumber,
        //        PageSize = criteria.PageSize,
        //        Total = criteria.Total,
        //        ContinuationSince = criteria.ContinuationSince
        //    };
        //}

        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? Total { get; set; }
        public string ContinuationSince { get; set; }
        public int? PartialTotal { get; set; }
    }
}
