

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Hospitals.Responses;

namespace SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitalById
{
    public class GetHospitalByIdQueryHandler:IRequestHandler<GetHospitalByIdQuery,HospitalResponse>
    {
        private readonly IApplicationDbContext context;

        public GetHospitalByIdQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<HospitalResponse> Handle(GetHospitalByIdQuery request , CancellationToken cancellationToken)
        {
            var hospital = await context.Hospitals
                .AsNoTracking()
                .Where(x => x.IsActive && x.Id == request.Id)
                .Select(x => new HospitalResponse
                {
                    Id = x.Id,
                    RohiniCode = x.RohiniCode,
                    Name = x.Name,
                    City = x.City,
                    Address = x.Address,
                    State = x.State,
                    Country = x.Country,
                }).FirstOrDefaultAsync(cancellationToken);

            if (hospital == null)
            {
                throw new NotFoundException("Hospital not found.");
            }

            return hospital;
        }
    }
}
