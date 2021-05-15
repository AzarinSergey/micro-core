using System.Collections.Generic;

namespace Core.Service.Host.Convention.Configuration
{
    public class ServiceSettings
    {
        public const string SectionName = "ServiceSettings";

        public string ServiceName { get; set; }

        public Dictionary<string,string> InternalServices { get; set; }
    }
}