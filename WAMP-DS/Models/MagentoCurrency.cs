namespace WAMP_DS.Models
{
    public class MagentoCurrency
    {
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";

        public override string ToString()
        {
            return $"{Name} ({Code})";
        }
    }
}