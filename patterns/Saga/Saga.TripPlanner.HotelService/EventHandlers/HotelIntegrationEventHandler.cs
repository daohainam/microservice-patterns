using MediatR;
using Saga.TripPlanner.HotelService.Infrastructure.Entity;
using Saga.TripPlanner.IntegrationEvents;

namespace Saga.TripPlanner.HotelService.EventHandlers;
public class HotelIntegrationEventHandler(HotelDbContext dbContext, IEventPublisher eventPublisher,
    ILogger<HotelIntegrationEventHandler> logger) :
    IRequestHandler<TripCreatedIntegrationEvent>
{
    public async Task Handle(TripCreatedIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling trip created event: {bookId}", request.TripId);

        // create booking for each room
        foreach (var room in request.HotelRooms)
        {
            // we need to make sure that the room is available for the booking dates
            var bookings = dbContext.Bookings.Where(b => b.RoomId == room.RoomId &&
                (b.CheckInDate >= room.CheckInDate && b.CheckInDate <= room.CheckOutDate) ||
                (b.CheckOutDate >= room.CheckInDate && b.CheckOutDate <= room.CheckOutDate)).ToList();

            if (bookings.Count != 0)
            {
                logger.LogInformation("Room is not available for the booking dates");
            }

            var booking = new Booking
            {
                RoomId = room.RoomId,
                TripId = request.TripId,
                BookingDate = room.BookingDate,
                CheckInDate = room.CheckInDate,
                CheckOutDate = room.CheckOutDate,
                Status = BookingStatus.Pending
            };

            bookings.Add(booking);

            logger.LogInformation("Booking {id} created successfully", booking.Id);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new HotelRoomBookingPendingIntegrationEvent
        {
            TripId = request.TripId,
        });
    }
}
