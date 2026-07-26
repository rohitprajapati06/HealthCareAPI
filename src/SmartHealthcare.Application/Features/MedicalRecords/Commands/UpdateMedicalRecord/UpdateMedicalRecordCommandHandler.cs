using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;



namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord
{
    public class UpdateMedicalRecordCommandHandler : IRequestHandler<UpdateMedicalRecordCommand,Unit>
    {
        private readonly IApplicationDbContext context;

        public UpdateMedicalRecordCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Unit> Handle(UpdateMedicalRecordCommand request , CancellationToken cancellationToken)
        {
            var recordId = await context.MedicalRecords.FirstOrDefaultAsync(x => x.Id == request.Id);

            if(recordId == null)
            {
                throw new NotFoundException("No Medical Record has been found");
            }

                recordId.FileName = request.FileName;
                recordId.FileUrl = request.FileUrl;
                recordId.RecordType = request.RecordType;
            
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
