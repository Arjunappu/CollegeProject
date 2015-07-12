using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantBookingSystem.Infrastructure
{
    public class PaginatedList<T> : List<T>
    {
        /// <summary>
        /// Gets the Current 0 based page index for the Paginated List
        /// </summary>
        public int PageIndex { get; private set; }

        /// <summary>
        /// Gets the Current page number for the Paginated List
        /// </summary>
        public int PageNumber
        {
            get { return PageIndex + 1; }
        }

        /// <summary>
        /// Gets the Size of a page
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets the total number of items in this collection
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Gets the total number of pages in tis collection
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Gets wheather the the collection has a previous page
        /// </summary>
        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }

        /// <summary>
        /// Gets wheather the collection has a next page
        /// </summary>
        public bool HasNextPage
        {
            get
            {
                return (PageIndex + 1 < TotalPages);
            }
        }

        /// <summary>
        /// Initiializes a new Paginated List from a given collection with specified page index and page size
        /// </summary>
        /// <param name="source">The source collection that nedds pagination</param>
        /// <param name="pageNumber">The 1 based page number</param>
        /// <param name="pageSize">The size of page</param>
        public PaginatedList(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            if (pageNumber < 1 || pageNumber > TotalPages) pageNumber = 1;
            source = source.AsQueryable();
            PageIndex = pageNumber -1;
            PageSize = pageSize;

            AddRange(source.Skip(PageIndex * PageSize).Take(PageSize));
        }
    }
}