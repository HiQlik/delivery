using DeliveryApp.Core.Domain.CourierAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class CourierRepository : ICourierRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CourierRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Courier> AddAsync(Courier courier)
    {
        if (courier.Transport != null) _dbContext.Attach(courier.Transport);

        if (courier.CourierStatus != null) _dbContext.Attach(courier.CourierStatus);

        var result = await _dbContext.Couriers.AddAsync(courier);
        return result.Entity;
    }

    public void Update(Courier courier)
    {
        if (courier.Transport != null) _dbContext.Attach(courier.Transport);

        if (courier.CourierStatus != null) _dbContext.Attach(courier.CourierStatus);

        _dbContext.Couriers.Update(courier);
    }

    public async Task<Courier> GetAsync(Guid courierId)
    {
        var courier = await _dbContext
            .Couriers
            .Include(x => x.Transport)
            .Include(x => x.CourierStatus)
            .FirstOrDefaultAsync(o => o.Id == courierId);
        return courier;
    }

    public IEnumerable<Courier> GetAllReady()
    {
        var couriers = _dbContext
            .Couriers
            .Include(x => x.Transport)
            .Include(x => x.CourierStatus)
            .Where(o => o.CourierStatus == CourierStatus.Ready);
        return couriers;
    }
}