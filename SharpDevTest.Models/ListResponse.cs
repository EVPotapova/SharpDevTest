using System.Collections.Generic;

namespace SharpDevTest.Models
{
    public abstract class ListResponse<T> where T : class
    {
        //TODO: Пагинация на будущее
        //public int PageSize { get; set; }
        //public int Page { get; set; }
        public int TotalItemsCount { get; set; }
        public ICollection<T> Items { get; set; }
    }
}