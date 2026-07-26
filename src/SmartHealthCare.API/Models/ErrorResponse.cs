namespace SmartHealthCare.API.Models
{
    public class ErrorResponse
    {
        public string StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime TimeStamp { get; set; }

    }
}
