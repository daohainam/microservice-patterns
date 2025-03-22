namespace Saga.TripPlanner.TripPlanningService;
public class SagaServices(HttpClient hotelHttpClient, HttpClient ticketHttpClient, HttpClient paymentHttpClient)
{
    public HttpClient HotelHttpClient { get; } = hotelHttpClient;
    public HttpClient TicketHttpClient { get; } = ticketHttpClient;
    public HttpClient PaymentHttpClient { get; } = paymentHttpClient;
}
