using MediatR;
using SmartHealthcare.Application.Features.AvailabilitySlots.Responses;


namespace SmartHealthcare.Application.Features.AvailabilitySlots.Queries.GetDoctorSlots;

public record GetDoctorSlotsQuery(Guid DoctorId) : IRequest<List<AvailabilitySlotResponse>>;
