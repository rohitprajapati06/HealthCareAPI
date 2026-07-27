using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord
{
    public class UpdateMedicalRecordCommandHandler : IRequestHandler<UpdateMedicalRecordCommand,Unit>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger logger;

        public UpdateMedicalRecordCommandHandler(IApplicationDbContext context , ILogger logger)
        {
            this.context = context;
            this.logger = logger;
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

            logger.LogInformation($" Medical record updated - {request.Id}");

            return Unit.Value;
        }
    }
}
