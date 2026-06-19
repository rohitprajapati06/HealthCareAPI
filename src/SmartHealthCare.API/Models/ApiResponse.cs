namespace SmartHealthCare.API.Models
{
    public class ApiResponse
    {
        public bool Success {  get; set; }

        public string Message { get; set; }

        public List<string> Errors = new List<string>();
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data {  get; set; }
    }
}
