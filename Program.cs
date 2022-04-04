using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SmartFormat;
using SmartFormat.Core.Settings;

string? protocol = null;

try
{
    IConfiguration configuration = GetConfiguration();

    protocol = configuration["Protocol"];
    EnsureProtocolIsRegistered(protocol);

    Dictionary<string, Handler> handlers = configuration.GetSection("Handlers").Get<Dictionary<string, Handler>>(); 
    HandleUrl(args.FirstOrDefault(), protocol, handlers);
    return 0;
}
catch(Exception exc)
{
    MessageBox.Show(exc.Message, "ProtocolHandler", MessageBoxButtons.OK, MessageBoxIcon.Error);
    return 1;
}

static void HandleUrl(string? arg, string protocol, Dictionary<string, Handler> handlers)
{
    if (string.IsNullOrEmpty(arg))
    {
        throw new ArgumentException("Url not provided");
    }

    Uri uri = new Uri(arg);
    if (!uri.Scheme.Equals(protocol))
    {
        throw new Exception($"Can handle only {protocol}:// urls");
    }

    if (!handlers.ContainsKey(uri.Host))
    {
        throw new Exception($"Handler {uri.Host} not found in configuration");
    }

    Handler handler = handlers[uri.Host];
    if (string.IsNullOrEmpty(handler.Executable))
    {
        throw new Exception($"Handler {uri.Host} has no associated executable");
    }

    string arguments = FormatCommandTemplate(handler.Arguments ?? string.Empty, uri.Query);
    Process.Start(handler.Executable, arguments);
}

static string FormatCommandTemplate(string commandTemplate, string? query)
{
    Dictionary<string, StringValues> values = QueryHelpers.ParseQuery(query);
    SmartSettings settings = new SmartSettings();
    settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;
    settings.Parser.ConvertCharacterStringLiterals = false;
    var formatter = Smart.CreateDefaultSmartFormat(settings);
    return formatter.Format(commandTemplate, values);
}

static IConfiguration GetConfiguration()
{
    return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false)
            .Build();
}

static void EnsureProtocolIsRegistered(string protocol)
{
    if (string.IsNullOrWhiteSpace(protocol))
    {
        throw new Exception("Protocol can't be empty");
    }

    using RegistryKey key = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Classes\\{protocol}");
    string? applicationLocation = Environment.ProcessPath;
    if (string.IsNullOrWhiteSpace(applicationLocation))
    {
        throw new Exception("Could not determine executable location");
    }

    key.SetValue("", "URL:ProtocolHandler");
    key.SetValue("URL Protocol", "");

    RegistryKey defaultIcon = key.CreateSubKey("DefaultIcon");
    defaultIcon.SetValue("", applicationLocation + ",1");

    using RegistryKey commandKey = key.CreateSubKey(@"shell\open\command");
    commandKey.SetValue("", $"\"{applicationLocation}\" \"%1\"");
}

record Handler 
{
    public string? Executable { get; init; }
    public string? Arguments { get; init; }
}