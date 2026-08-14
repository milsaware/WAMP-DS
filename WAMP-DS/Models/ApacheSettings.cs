namespace WAMP_DS.Models
{
    public class ApacheSettings
    {
        public bool HttpEnabled { get; set; }

        public bool HttpsEnabled { get; set; }

        public bool RewriteEnabled { get; set; }

        public bool HeadersEnabled { get; set; }

        public bool DeflateEnabled { get; set; }
    }
}