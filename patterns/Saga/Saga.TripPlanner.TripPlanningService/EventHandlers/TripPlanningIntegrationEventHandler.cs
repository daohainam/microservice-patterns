using MediatR;
using Saga.TripPlanner.IntegrationEvents;

namespace Saga.TripPlanner.HotelService.EventHandlers;
public class TripPlanningIntegrationEventHandler(IEventPublisher eventPublisher,
    ILogger<TripPlanningIntegrationEventHandler> logger) :
    IRequestHandler<HotelRoomBookingPendingIntegrationEvent>
{
    public async Task Handle(HotelRoomBookingPendingIntegrationEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling trip hotel room booked event: {bookId}", request.TripId);

        await eventPublisher.PublishAsync(new HotelRoomBookingPendingIntegrationEvent
        {
            TripId = request.TripId,
        });
    }
}
