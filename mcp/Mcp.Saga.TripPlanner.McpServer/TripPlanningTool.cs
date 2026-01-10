using ModelContextProtocol.Server;
using System.Text.Json;

namespace Mcp.Saga.TripPlanner.McpServer;

[McpServerToolType]
public class TripPlanningTool
{
    [McpServerTool]
    public static async Task<string> GetTrips(ITripPlanningService tripPlanningService, CancellationToken cancellationToken)
    {
        var trips = await tripPlanningService.GetTrips(cancellationToken);
        return JsonSerializer.Serialize(trips);
    }

    [McpServerTool]
    public static async Task<string> GetTripDetails(ITripPlanningService tripPlanningService, string tripId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(tripId, out var parsedTripId))
        {
            throw new ArgumentException($"Invalid trip ID format: '{tripId}'. Expected a valid GUID.", nameof(tripId));
        }

        var trip = await tripPlanningService.GetTripById(parsedTripId, cancellationToken);
        return JsonSerializer.Serialize(trip);
    }

    [McpServerTool]
    public static async Task<string> CreateTrip(
        ITripPlanningService tripPlanningService,
        string tripName,
        string startDate,
        string endDate,
        string ticketTypeIds,
        string roomIds,
        string cardNumber,
        string cardHolderName,
        string expirationDate,
        string cvv,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tripName))
        {
            throw new ArgumentException("Trip name is required.", nameof(tripName));
        }

        if (!DateTime.TryParse(startDate, out var parsedStartDate))
        {
            throw new ArgumentException($"Invalid start date format: '{startDate}'.", nameof(startDate));
        }

        if (!DateTime.TryParse(endDate, out var parsedEndDate))
        {
            throw new ArgumentException($"Invalid end date format: '{endDate}'.", nameof(endDate));
        }

        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            throw new ArgumentException("Card number is required.", nameof(cardNumber));
        }

        if (string.IsNullOrWhiteSpace(cardHolderName))
        {
            throw new ArgumentException("Card holder name is required.", nameof(cardHolderName));
        }

        // Parse ticket type IDs (comma-separated GUIDs)
        var ticketBookings = new List<TicketBooking>();
        if (!string.IsNullOrWhiteSpace(ticketTypeIds))
        {
            var ticketTypeIdArray = ticketTypeIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var ticketTypeId in ticketTypeIdArray)
            {
                if (Guid.TryParse(ticketTypeId.Trim(), out var parsedTicketTypeId))
                {
                    ticketBookings.Add(new TicketBooking
                    {
                        TicketTypeId = parsedTicketTypeId
                    });
                }
            }
        }

        // Parse room IDs (comma-separated GUIDs)
        var hotelBookings = new List<HotelBooking>();
        if (!string.IsNullOrWhiteSpace(roomIds))
        {
            var roomIdArray = roomIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var roomId in roomIdArray)
            {
                if (Guid.TryParse(roomId.Trim(), out var parsedRoomId))
                {
                    hotelBookings.Add(new HotelBooking
                    {
                        RoomId = parsedRoomId,
                        CheckInDate = parsedStartDate,
                        CheckOutDate = parsedEndDate
                    });
                }
            }
        }

        var trip = new Trip
        {
            Name = tripName,
            StartDate = parsedStartDate,
            EndDate = parsedEndDate,
            TicketBookings = ticketBookings,
            HotelRoomBookings = hotelBookings,
            CardNumber = cardNumber,
            CardHolderName = cardHolderName,
            ExpirationDate = expirationDate,
            Cvv = cvv,
            Amount = amount
        };

        var createdTrip = await tripPlanningService.CreateTrip(trip, cancellationToken);
        return JsonSerializer.Serialize(createdTrip);
    }
}
