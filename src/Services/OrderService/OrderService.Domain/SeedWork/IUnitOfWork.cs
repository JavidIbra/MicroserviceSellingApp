namespace OrderService.Domain.SeedWork
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangeAsync(CancellationToken cancellationToken = default);
        Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    }
}
