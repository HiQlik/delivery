using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.CourierAggregate;

/// <summary>
///     Курьер
/// </summary>
public class Courier : Aggregate
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Courier()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="transport">Транспорт</param>
    private Courier(string name, Transport transport) : this()
    {
        Id = Guid.NewGuid();
        Name = name;
        Transport = transport;
        Location = Location.MinLocation;
        CourierStatus = CourierStatus.NotAvailable;
    }

    /// <summary>
    ///     Имя
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     Вид транспорта
    /// </summary>
    public Transport Transport { get; }

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public Location Location { get; private set; }

    /// <summary>
    ///     Статус курьера
    /// </summary>
    public CourierStatus CourierStatus { get; private set; }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="transport">Транспорт</param>
    /// <returns>Результат</returns>
    public static Result<Courier, Error> Create(string name, Transport transport)
    {
        if (string.IsNullOrEmpty(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if (transport == null) return GeneralErrors.ValueIsRequired(nameof(transport));

        return new Courier(name, transport);
    }

    /// <summary>
    ///     Изменить местоположение
    /// </summary>
    /// <param name="targetLocation">Геопозиция</param>
    /// <returns>Результат</returns>
    public Result<object, Error> Move(Location targetLocation)
    {
        if (targetLocation == null) return GeneralErrors.ValueIsRequired(nameof(targetLocation));

        var difX = targetLocation.X - Location.X;
        var difY = targetLocation.Y - Location.Y;

        var newX = Location.X;
        var newY = Location.Y;

        var cruisingRange = Transport.Speed;

        if (difX > 0)
        {
            if (difX >= cruisingRange)
            {
                newX += cruisingRange;
                Location = Location.Create(newX, newY).Value;
                return new object();
            }

            if (difX < cruisingRange)
            {
                newX += difX;
                Location = Location.Create(newX, newY).Value;
                if (Location == targetLocation) return new object();
                cruisingRange -= difX;
            }
        }

        if (difX < 0)
        {
            if (Math.Abs(difX) >= cruisingRange)
            {
                newX -= cruisingRange;
                Location = Location.Create(newX, newY).Value;
                return new object();
            }

            if (Math.Abs(difX) < cruisingRange)
            {
                newX -= Math.Abs(difX);
                Location = Location.Create(newX, newY).Value;
                if (Location == targetLocation) return new object();
                cruisingRange -= Math.Abs(difX);
            }
        }

        if (difY > 0)
        {
            if (difY >= cruisingRange)
            {
                newY += cruisingRange;
                Location = Location.Create(newX, newY).Value;
                return new object();
            }

            if (difY < cruisingRange)
            {
                newY += difY;
                Location = Location.Create(newX, newY).Value;
                if (Location == targetLocation) return new object();
            }
        }

        if (difY < 0)
        {
            if (Math.Abs(difY) >= cruisingRange)
            {
                newY -= cruisingRange;
                Location = Location.Create(newX, newY).Value;
                return new object();
            }

            if (Math.Abs(difY) < cruisingRange)
            {
                newY -= Math.Abs(difY);
                Location = Location.Create(newX, newY).Value;
                if (Location == targetLocation) return new object();
            }
        }

        Location = Location.Create(newX, newY).Value;
        return new object();
    }

    /// <summary>
    ///     Начать работать
    /// </summary>
    /// <returns>Результат</returns>
    public Result<object, Error> StartWork()
    {
        if (CourierStatus == CourierStatus.Busy) return Errors.TryStartWorkingWhenAlreadyStarted();
        CourierStatus = CourierStatus.Ready;
        return new object();
    }

    /// <summary>
    ///     Взять работу
    /// </summary>
    /// <returns>Результат</returns>
    public Result<object, Error> InWork()
    {
        if (CourierStatus == CourierStatus.NotAvailable) return Errors.TryAssignOrderWhenNotAvailable();
        if (CourierStatus == CourierStatus.Busy) return Errors.TryAssignOrderWhenCourierHasAlreadyBusy();
        CourierStatus = CourierStatus.Busy;
        return new object();
    }

    /// <summary>
    ///     Завершить работу
    /// </summary>
    /// <returns>Результат</returns>
    public Result<object, Error> CompleteOrder()
    {
        CourierStatus = CourierStatus.Ready;
        return new object();
    }

    /// <summary>
    ///     Закончить работать
    /// </summary>
    /// <returns>Результат</returns>
    public Result<object, Error> StopWork()
    {
        if (CourierStatus == CourierStatus.Busy) return Errors.TryStopWorkingWithIncompleteDelivery();
        CourierStatus = CourierStatus.NotAvailable;
        return new object();
    }

    /// <summary>
    ///     Рассчитать время до точки
    /// </summary>
    /// <param name="location">Конечное местоположение</param>
    /// <returns>Результат</returns>
    public Result<double, Error> CalculateTimeToLocation(Location location)
    {
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        var distance = Location.DistanceTo(location).Value;
        var time = (double) distance / Transport.Speed;
        return time;
    }

    public static class Errors
    {
        public static Error TryStopWorkingWithIncompleteDelivery()
        {
            return new Error($"{nameof(Courier).ToLowerInvariant()}.try.stop.working.with.incomplete.delivery",
                "Нельзя прекратить работу, если есть незавершенная доставка");
        }

        public static Error TryStartWorkingWhenAlreadyStarted()
        {
            return new Error($"{nameof(Courier).ToLowerInvariant()}.try.start.working.when.already.started",
                "Нельзя начать работу, если ее уже начали ранее");
        }

        public static Error TryAssignOrderWhenNotAvailable()
        {
            return new Error($"{nameof(Courier).ToLowerInvariant()}.try.assign.order.when.not.available",
                "Нельзя взять заказ в работу, если курьер не начал рабочий день");
        }

        public static Error TryAssignOrderWhenCourierHasAlreadyBusy()
        {
            return new Error($"{nameof(Courier).ToLowerInvariant()}.try.assign.order.when.courier.has.already.busy",
                "Нельзя взять заказ в работу, если курьер уже занят");
        }
    }
}
