﻿namespace CatalogService.Api.Core.Application.ViewModels
{
    public class PaginatedItemsViewModel<TEntity>(int pageIndex, int pageSize, int count, IEnumerable<TEntity> data) where TEntity : class
    {
        public int PageIndex { get; private set; } = pageIndex;
        public int PageSize { get; private set; } = pageSize;
        public long Count { get; private set; } = count;
        public IEnumerable<TEntity> Data { get; private set; } = data;
    }
}
