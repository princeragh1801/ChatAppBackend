namespace ChatApp.Dtos
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; } 
        public string? ErrorMessage { get; set; }
        public ApiResponse() { }
        public ApiResponse(T data) 
        {
            this.Data = data;
        }
        public ApiResponse(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }
    }
}
