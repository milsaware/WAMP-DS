using WAMP_DS.Models;

namespace WAMP_DS.Managers
{
    public class MagentoLanguageManager
    {
        public List<MagentoLanguage> GetLanguages()
        {
            return new List<MagentoLanguage>
            {
                new() { Name = "Afrikaans (South Africa)", Code = "af_ZA" },
                new() { Name = "Albanian (Albania)", Code = "sq_AL" },
                new() { Name = "Arabic (Algeria)", Code = "ar_DZ" },
                new() { Name = "Arabic (Egypt)", Code = "ar_EG" },
                new() { Name = "Arabic (Kuwait)", Code = "ar_KW" },
                new() { Name = "Arabic (Morocco)", Code = "ar_MA" },
                new() { Name = "Arabic (Saudi Arabia)", Code = "ar_SA" },

                new() { Name = "Chinese (Simplified Han, China)", Code = "zh_Hans_CN" },
                new() { Name = "Chinese (Traditional Han, Hong Kong SAR China)", Code = "zh_Hant_HK" },
                new() { Name = "Chinese (Traditional Han, Taiwan)", Code = "zh_Hant_TW" },

                new() { Name = "Danish (Denmark)", Code = "da_DK" },
                new() { Name = "Dutch (Belgium)", Code = "nl_BE" },
                new() { Name = "Dutch (Netherlands)", Code = "nl_NL" },

                new() { Name = "English (Australia)", Code = "en_AU" },
                new() { Name = "English (Canada)", Code = "en_CA" },
                new() { Name = "English (Ireland)", Code = "en_IE" },
                new() { Name = "English (New Zealand)", Code = "en_NZ" },
                new() { Name = "English (United Kingdom)", Code = "en_GB" },
                new() { Name = "English (United States)", Code = "en_US" },

                new() { Name = "French (France)", Code = "fr_FR" },
                new() { Name = "German (Germany)", Code = "de_DE" },

                new() { Name = "Italian (Italy)", Code = "it_IT" },
                new() { Name = "Japanese (Japan)", Code = "ja_JP" },
                new() { Name = "Korean (South Korea)", Code = "ko_KR" },

                new() { Name = "Norwegian Bokmål (Norway)", Code = "nb_NO" },

                new() { Name = "Polish (Poland)", Code = "pl_PL" },

                new() { Name = "Portuguese (Brazil)", Code = "pt_BR" },
                new() { Name = "Portuguese (Portugal)", Code = "pt_PT" },

                new() { Name = "Russian (Russia)", Code = "ru_RU" },

                new() { Name = "Spanish (Spain)", Code = "es_ES" },
                new() { Name = "Spanish (Mexico)", Code = "es_MX" },

                new() { Name = "Swedish (Sweden)", Code = "sv_SE" },

                new() { Name = "Turkish (Türkiye)", Code = "tr_TR" },

                new() { Name = "Ukrainian (Ukraine)", Code = "uk_UA" },

                new() { Name = "Vietnamese (Vietnam)", Code = "vi_VN" },

                new() { Name = "Welsh (United Kingdom)", Code = "cy_GB" }
            };
        }
    }
}