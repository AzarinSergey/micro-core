using System;
using Newtonsoft.Json;

namespace Core.Tool.Json
{
    public interface IAppJsonSerializer
    {
        string Serialize(object o, Formatting format = Formatting.None);
        T Deserialize<T>(string str);
        object Deserialize(string str, Type type);
    }
}