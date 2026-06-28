namespace OnlineJobRecruitmentSystem.Common
{
    public class ResponseModel<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ResponseModel<T> Ok(T data, string message = "Success")
        {
            return new ResponseModel<T> { Success = true, Message = message, Data = data };
        }

        public static ResponseModel<T> Fail(string message)
        {
            return new ResponseModel<T> { Success = false, Message = message };
        }
    }
}
