namespace Core.Tool.Json
{
    public class JsonTools
    {
        public IAppJsonSerializer Serializer => new NewtonsoftJsonSerializer();
    }
}
