namespace LibraryManageSystemApi.MiddlerWare
{
    public class InvalidParamError : Exception
    {
        public InvalidParamError(string? message) : base(message)
        {
        }
    }
}
