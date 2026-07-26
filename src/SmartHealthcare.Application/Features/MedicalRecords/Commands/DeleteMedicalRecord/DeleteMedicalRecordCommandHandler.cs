

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.DeleteMedicalRecord
{
    public class DeleteMedicalRecordCommandHandler : IRequestHandler<DeleteMedicalRecordCommand,Unit>
    {
        private readonly IApplicationDbContext context;

        public DeleteMedicalRecordCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Unit> Handle(DeleteMedicalRecordCommand request , CancellationToken cancellationToken)
        {
            var recordId = await context.MedicalRecords.FirstOrDefaultAsync(x => x.Id == request.Id);

            if(recordId == null)
            {
                throw new NotFoundException("Medical Record not found");
            }

             context.MedicalRecords.Remove(recordId);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;

        } 
    }
}
