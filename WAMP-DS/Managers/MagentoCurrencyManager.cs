using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class MagentoCurrencyManager
    {
        public List<MagentoCurrency> GetCurrencies()
        {
            return new List<MagentoCurrency>
            {
                new() { Name = "British Pound", Code = "GBP" },
                new() { Name = "Euro", Code = "EUR" },
                new() { Name = "US Dollar", Code = "USD" },
                new() { Name = "Canadian Dollar", Code = "CAD" },
                new() { Name = "Australian Dollar", Code = "AUD" },
                new() { Name = "Japanese Yen", Code = "JPY" },
                new() { Name = "Chinese Yuan", Code = "CNY" },
                new() { Name = "Indian Rupee", Code = "INR" },
                new() { Name = "Swiss Franc", Code = "CHF" }
            }
            .OrderBy(x => x.Name)
            .ToList();
        }
    }
}