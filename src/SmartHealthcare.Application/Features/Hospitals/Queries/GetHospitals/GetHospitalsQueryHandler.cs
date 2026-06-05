using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Hospitals.Responses;


namespace SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitals
{
    public class GetHospitalsQueryHandler : IRequestHandler<GetHospitalsQuery,List<HospitalResponse>>
    {
        private readonly IApplicationDbContext context;

        public GetHospitalsQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<HospitalResponse>> Handle(GetHospitalsQuery request , CancellationToken cancellationToken)
        {
            return await context.Hospitals
                .Where(x => x.IsActive)
                .Select(x => new HospitalResponse
                {
                    Id = x.Id,
                    RohiniCode = x.RohiniCode,
                    Name = x.Name,
                    Address = x.Address,
                    City = x.City,
                    State = x.State,
                    Country = x.Country
                })
                .ToListAsync(cancellationToken);
        }
    }
}
