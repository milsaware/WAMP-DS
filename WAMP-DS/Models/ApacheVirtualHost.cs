public class ApacheVirtualHost
{
    public string ServerName { get; set; } = "";

    public string DocumentRoot { get; set; } = "";

    public string Directory { get; set; } = "";

    public string CustomConfiguration { get; set; } = "";


    // Directory rules

    public string AllowOverride { get; set; } = "All";

    public string RequireValue { get; set; } = "None";

    public bool OptionsIndexes { get; set; }

    public bool OptionsFollowSymLinks { get; set; }

    public bool OptionsExecCGI { get; set; }

    public bool OptionsIncludes { get; set; }

    public bool OptionsMultiViews { get; set; }


    public bool RewriteEngine { get; set; }


    // HTTPS

    public bool HttpsEnabled { get; set; }

    public string CertificatePath { get; set; } = "";

    public string PrivateKeyPath { get; set; } = "";


    // Logging

    public string ErrorLog { get; set; } = "";

    public string CustomLog { get; set; } = "";


    public override string ToString()
    {
        return ServerName;
    }
}