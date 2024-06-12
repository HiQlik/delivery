using DeliveryApp.Core.Domain.OrderAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Order> AddAsync(Order order)
    {
        if (order.OrderStatus != null) _dbContext.Attach(order.OrderStatus);

        var result = await _dbContext.Orders.AddAsync(order);
        return result.Entity;
    }

    public void Update(Order order)
    {
        if (order.OrderStatus != null) _dbContext.Attach(order.OrderStatus);

        _dbContext.Orders.Update(order);
    }

    public async Task<Order> GetAsync(Guid orderId)
    {
        var order = await _dbContext
            .Orders
            .Include(x => x.OrderStatus)
            .SingleOrDefaultAsync(o => o.Id == orderId);
        return order;
    }

    public IEnumerable<Order> GetAllCreated()
    {
        var orders = _dbContext
            .Orders
            .Include(x => x.OrderStatus)
            .Where(o => o.OrderStatus == OrderStatus.Created);
        return orders;
    }

    public IEnumerable<Order> GetAllAssigned()
    {
        var orders = _dbContext
            .Orders
            .Include(x => x.OrderStatus)
            .Where(o => o.OrderStatus == OrderStatus.Assigned);
        return orders;
    }
}