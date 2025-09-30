namespace Project_X_Data.Models.Rest
{
    public class RestStatus
    {
        public String Phrase { get; set; } = "Ok";
        public int Code { get; set; } = 200;
        public bool IsOk { get; set; } = true;

        public static RestStatus Status400 = new RestStatus { IsOk = false, Code = 400, Phrase = "Bad Request" };
        public static RestStatus Status401 = new RestStatus { IsOk = false, Code = 401, Phrase = "UnAuthorized" };


    }
}
