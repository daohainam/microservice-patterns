namespace Saga.OnlineStore.PaymentService.EventHandlers;
public class PaymentIntegrationEventHandlers(PaymentDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<PaymentIntegrationEventHandlers> logger) :
    INotificationHandler<OrderItemsReservedIntegrationEvent>
{
    public async Task Handle(OrderItemsReservedIntegrationEvent request, CancellationToken cancellationToken)
    {
        // this event is sent by Payment service when it approves payment for an order
        logger.LogInformation("Handling order items reserved event: {id}", request.OrderId);

        if (request.PaymentCardNumber.Length != 16)
        {
            logger.LogWarning("Invalid card number on order {id}: {cardNumber}", request.OrderId, MaskCardNumber(request.PaymentCardNumber));

            await eventPublisher.PublishAsync(new OrderPaymentRejectedIntegrationEvent()
            {
                OrderId = request.OrderId,
                Reason = "Invalid payment card",
            });
            return;
        }

        var card = await dbContext.Cards.Where(c => c.CardNumber == request.PaymentCardNumber).SingleOrDefaultAsync(cancellationToken);
        if (card == null)
        {
            logger.LogWarning("Card not found on order {id}: '{cardNumber}'", request.OrderId, MaskCardNumber(request.PaymentCardNumber));

            await eventPublisher.PublishAsync(new OrderPaymentRejectedIntegrationEvent()
            {
                OrderId = request.OrderId,
                Reason = "Invalid payment card",
            });
            return;
        }

        var newBalance = card.Balance - request.Items.Sum(i => i.Price * i.Quantity);

        if (newBalance < 0)
        {
            logger.LogWarning("Insufficient funds on card {id} for order {orderId}", card.Id, request.OrderId);
            await eventPublisher.PublishAsync(new OrderPaymentRejectedIntegrationEvent()
            {
                OrderId = request.OrderId,
                Reason = "Insufficient balance",
            });
            return;
        }
        else
        {
            card.Balance = newBalance;
            await dbContext.SaveChangesAsync(cancellationToken);

            await eventPublisher.PublishAsync(new OrderPaymentApprovedIntegrationEvent()
            {
                OrderId = request.OrderId
            });
        }
    }

    private static string MaskCardNumber(string cardNumber)
    {
        return cardNumber.Substring(0, 4) + new string('*', cardNumber.Length - 8) + cardNumber.Substring(cardNumber.Length - 4);
    }
}
