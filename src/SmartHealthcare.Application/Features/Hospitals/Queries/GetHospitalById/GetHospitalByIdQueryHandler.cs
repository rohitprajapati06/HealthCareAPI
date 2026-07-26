

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
            var hospital = await context.Hospitals.Where(x => x.IsActive).FirstOrDefaultAsync(x => x.Id == request.Id);

            if (hospital == null) {
                throw new NotFoundException("Hospital not found");
            }

            return new HospitalResponse
            {
                 Id = hospital.Id,
                 RohiniCode =  hospital.RohiniCode,
                 Name = hospital.Name,
                 Address = hospital.Address,
                 City = hospital.City,
                 State = hospital.State,
                 Country = hospital.Country,
                 
            };
        }
    }
}
