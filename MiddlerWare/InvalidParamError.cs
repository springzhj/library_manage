namespace PloliticalScienceSystemApi.MiddlerWare
{
    public class InvalidParamError : Exception
    {
        public InvalidParamError(string? message) : base(message)
        {
        }
    }
}
