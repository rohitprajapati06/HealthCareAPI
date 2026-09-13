using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SmartHealthcare.Application.Contracts.Services;
using SmartHealthcare.Application.Features.Hospitals.Commands.ImportHospitals;

namespace SmartHealthcare.UnitTests.Features.Hospitals
{
    public class ImportHospitalsCommandHandlerTests
    {
        private readonly Mock<IHospitalImportService> hospitalImportServiceMock = new();
        private readonly Mock<ILogger<ImportHospitalsCommandHandler>> loggerMock = new();

        private ImportHospitalsCommandHandler CreateHandler()
            => new(hospitalImportServiceMock.Object, loggerMock.Object);

        private static Mock<IFormFile> CreateFormFileMock(Stream stream)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            return fileMock;
        }

        [Fact]
        public async Task Handle_ValidFile_DelegatesToImportServiceAndReturnsUnit()
        {
            using var stream = new MemoryStream("id,name\n1,City Hospital"u8.ToArray());
            var file = CreateFormFileMock(stream);

            hospitalImportServiceMock
                .Setup(x => x.ImportHospitalsAsync(stream, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            var handler = CreateHandler();

            var result = await handler.Handle(new ImportHospitalsCommand { File = file.Object }, CancellationToken.None);

            result.Should().Be(Unit.Value);
            hospitalImportServiceMock.Verify(x => x.ImportHospitalsAsync(stream, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ImportServiceThrows_PropagatesException()
        {
            using var stream = new MemoryStream("bad data"u8.ToArray());
            var file = CreateFormFileMock(stream);

            hospitalImportServiceMock
                .Setup(x => x.ImportHospitalsAsync(stream, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidDataException("Malformed CSV"));

            var handler = CreateHandler();

            var act = () => handler.Handle(new ImportHospitalsCommand { File = file.Object }, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidDataException>().WithMessage("Malformed CSV");
        }
    }
}
