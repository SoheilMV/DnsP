using CommandLine;

internal class ArgumentsOption
{
    [Option('a', "add", HelpText = "Add DNS to the list.", SetName = "add")]
    public string? Add { get; set; }

    [Option('n', "name", HelpText = "Choose a name for DNS.", Hidden = true)]
    public string? Name { get; set; }

    [Option('r', "remove", HelpText = "Remove DNS from the list.")]
    public string? Remove { get; set; }

    [Option('b', "block", HelpText = "Add hosts to blacklist")]
    public string? Block { get; set; }

    [Option('l', "unblock", HelpText = "Remove hosts from the blacklist.")]
    public string? Unblock { get; set; }

    [Option('s', "skip", HelpText = "Skip dns.")]
    public string? Skip { get; set; }

    [Option('k', "unskip", HelpText = "Undo skip dns.")]
    public string? Unskip { get; set; }

    [Option("log", HelpText = "Display the list of DNS.")]
    public bool Log { get; set; }

    [Option("run", HelpText = "Run local DNS.")]
    public bool Run { get; set; }

    [Option("clear", HelpText = "Clear all dns from the list.")]
    public bool Clear { get; set; }
}
