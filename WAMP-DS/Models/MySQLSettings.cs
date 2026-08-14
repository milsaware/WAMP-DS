namespace WAMP_DS.Models
{
    public class MySQLSettings
    {
        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 3306;

        public string Username { get; set; } = "root";

        public string Password { get; set; } = "";
    }
}