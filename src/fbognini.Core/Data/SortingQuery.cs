namespace fbognini.Core.Data
{
    public class SortingQuery
    {
        public SortingQuery(
            string criteria,
            SortingDirection direction)
        {
            SortingCriteria = criteria;
            SortingDirection = direction;
        }

        /// <summary>
        /// [Sorting] - sorting criteri (field)
        /// </summary>
        /// <example>id</example>
        public string SortingCriteria { get; private set; }

        /// <summary>
        /// [Sorting] - sorting direction (DESCENDING -1, ASCENDING 1)
        /// </summary>
        /// <example>1</example>
        public SortingDirection SortingDirection { get; set; }
    }
}
