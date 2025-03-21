﻿using System.Text.Json.Serialization;

namespace Saga.TripPlanner.TripPlanningService.Infrastructure.Entity;
public class Trip
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Name { get; set; } = default!;
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreationDate { get; set; }
    public List<TicketBooking> TicketBookings { get; set; } = [];
    public List<HotelBooking> HotelRoomBookings { get; set; } = [];
}
