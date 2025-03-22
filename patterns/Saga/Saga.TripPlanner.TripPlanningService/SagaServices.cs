namespace Saga.TripPlanner.TripPlanningService;
public class SagaServices
{
    public HttpClient HotelHttpClient { get; }
    //public HttpClient TicketHttpClient { get; }
    //public HttpClient PaymentHttpClient { get; }

    public SagaServices(HttpClient hotelHttpClient/*, HttpClient ticketHttpClient, HttpClient paymentHttpClient*/)
    {
        HotelHttpClient = hotelHttpClient;
        //TicketHttpClient = ticketHttpClient;
        //PaymentHttpClient = paymentHttpClient;
    }
}
