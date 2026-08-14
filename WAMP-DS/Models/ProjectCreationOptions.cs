namespace WAMP_DS.Models
{
    public class ProjectCreationOptions
    {
        public string ParentDirectory { get; set; } = "";

        public string ProjectName { get; set; } = "";

        public ProjectType ProjectType { get; set; }

        public bool CreateVirtualHost { get; set; }

        public string VirtualHostDomain { get; set; } = "";

        public bool EnableHttps { get; set; }

        public bool CreateDatabase { get; set; }

        public string DatabaseName { get; set; } = "";
    }
}