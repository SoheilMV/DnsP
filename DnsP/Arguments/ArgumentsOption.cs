﻿using CommandLine;

internal class ArgumentsOption
{
    [Option('a', "add", HelpText = "Add dns to the list.")]
    public string? Add { get; set; }

    [Option('n', "name", HelpText = "Choose a name for dns.", Hidden = true)]
    public string? Name { get; set; }

    [Option('r', "remove", HelpText = "Remove dns from the list.")]
    public string? Remove { get; set; }

    [Option('b', "block", HelpText = "Add host to blacklist")]
    public string? Block { get; set; }

    [Option('u', "unblock", HelpText = "Remove host from the blacklist.")]
    public string? Unblock { get; set; }

    [Option('c', "check", HelpText = "Find healthy dns.")]
    public string? Check { get; set; }

    [Option('f', "flush", HelpText = "Flushing your previous dns addresses.")]
    public bool Flush { get; set; }

    [Option('t', "timeout", HelpText = "Timeout when checking dns.", Default = 5000, Hidden = true)]
    public int Timeout { get; set; }

    [Option('p', "protocol", HelpText = "Change the dns protocol.")]
    public bool Protocol { get; set; }

    [Option('m', "mode", HelpText = "Change the type of dns list usage.")]
    public bool Mode { get; set; }

    [Option('v', "visit", HelpText = "Visit the project repository.")]
    public bool Visit { get; set; }

    [Option('e', "environment", HelpText = "Checks if dnsp is added to the system path.")]
    public bool Environment { get; set; }

    [Option('l', "log", HelpText = "Display the list of dns.")]
    public bool Log { get; set; }

    [Option("run", HelpText = "Run local dns.")]
    public string? Run { get; set; }
}
