namespace WAMP_DS.Models
{
    public class MagentoLanguage
    {
        public string Name { get; set; } = "";

        public string Code { get; set; } = "";

        public override string ToString()
        {
            return Name;
        }
    }
}