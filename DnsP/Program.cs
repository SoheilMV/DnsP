using System.Net;
using CommandLine;
using ConsoleTables;
using Ae.Dns.Server;
using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Ae.Dns.Client.Filters;

bool _run = false;
string _name = "DnsP";
CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

if (args.Length == 0)
    args = new string[] { "--help" };

try
{
    Console.Title = _name;
    Database db = Database.Initialize();
    var arguments = ArgumentsParser.Parse(args);
    var result = Parser.Default.ParseArguments<ArgumentsOption>(arguments);
    result.WithParsed(options =>
    {
        if (!string.IsNullOrEmpty(options.Add))
        {
            IPAddress? ip = options.Add.GetAddress();
            string name = "-";
            if (!string.IsNullOrEmpty(options.Name))
                name = options.Name;

            if (ip != null)
            {
                if (db.Add(ip.ToString(), name))
                    Logger.Info($"DNS was added to the list.");
                else
                    Logger.Error("DNS already exists, enter another DNS.");
            }
            else
                Logger.Error("The IP address entered is not valid.");
        }
        else if (!string.IsNullOrEmpty(options.Remove))
        {
            if (db.Remove(options.Remove))
                Logger.Info($"DNS was removed from the list.");
            else
                Logger.Error("DNS is not available in the list, choose DNS from the list.");
        }
        else if (!string.IsNullOrEmpty(options.Block))
        {
            if (db.Block(options.Block))
                Logger.Info($"{options.Block} was added to the blocklist.");
            else
                Logger.Error($"{options.Block} is already blocked, choose another host.");
        }
        else if (!string.IsNullOrEmpty(options.Unblock))
        {
            if (db.Unblock(options.Unblock))
                Logger.Info($"{options.Unblock} was removed from the blocklist.");
            else
                Logger.Error($"{options.Unblock} is not already blocked, select the host from the blocklist");
        }
        else if (!string.IsNullOrEmpty(options.Skip))
        {
            if (db.Skip(options.Skip))
                Logger.Info($"DNS is skipped.");
            else
                Logger.Error("DNS is not available in the list, choose DNS from the list.");
        }
        else if (!string.IsNullOrEmpty(options.Unskip))
        {
            if (db.Unskip(options.Unskip))
                Logger.Info($"DNS is used.");
            else
                Logger.Error("DNS is not available in the list, choose DNS from the list.");
        }
        else if (options.Clear)
        {
            db.Clear();
            Logger.Warn("DNS list was completely cleared.");
        }
        else if (options.Log)
        {
            if (db.list != null)
            {
                var table = new ConsoleTable("ID", "DNS", "Skip", "Name");
                foreach (var item in db.list)
                    table.AddRow(item.id, item.dns, item.skip, item.name);

                Logger.Debug(table.ToMarkDownString());
            }
        }
        else if (options.Run)
        {
            _run = true;

            Console.ForegroundColor = ConsoleColor.Magenta;
            _name.ToLogo();
            "https://github.com/SoheilMV".ToCenter();
            Console.ResetColor();

            ClosingHandler.Create(onClosing, onClosed);
            Logger.Warn("Press CTRL + C to exit the program.");
        }
        else
        {
            Logger.Error("The command is unknown.");
        }
    });

    if (_run)
    {

        List<IDnsClient> dnsClients = new List<IDnsClient>();
        foreach (DNS dns in db.list)
        {
            if (!dns.skip)
                dnsClients.Add(new DnsUdpClient(IPAddress.Parse(dns.dns)));
        }
        using IDnsClient RacerClient = new DnsRacerClient(dnsClients.ToArray());
        IDnsFilter dnsFilter = new DnsDelegateFilter((message) =>
        {
            bool result = true;
            string host = message.Header.Host;
            if (db.HostIsBlock(host))
            {
                Logger.Error(host);
                result = false;
            }
            else
                Logger.Debug(host);

            return result;
        });
        using IDnsClient filterClient = new DnsFilterClient(dnsFilter, RacerClient);
        var serverOptions = new DnsUdpServerOptions
        {
            Endpoint = new IPEndPoint(IPAddress.Any, 53)
        };
        using IDnsRawClient rawClient = new DnsRawClient(filterClient);
        using IDnsServer server = new DnsUdpServer(rawClient, serverOptions);

        Logger.Warn($"DNS was implemented on the '{IPAddress.Any}'.");
        if (Utility.IsRunAsAdmin())
        {
            Logger.Warn("Please wait...");
            var network = Utility.GetNetworkInterface();
            Utility.RunCommand("netsh", $"interface ipv4 add dns name=\"{network.Name}\" address=127.0.0.1 index=1");
            //Utility.RunCommand("netsh", $"interface ipv4 add dns name=\"{network.Name}\" address=127.0.0.1 index=2");
            Logger.Warn("DNS settings changed successfully.");
            Console.WriteLine();
        }
        else
        {
            Logger.Error("DNS registration requires elevation (Run as administrator).");
            Console.WriteLine();
        }

        await server.Listen(_cancellationTokenSource.Token);
    }

}
catch (Exception ex)
{
    Logger.Error(ex.Message);
}

void onClosing()
{
    _cancellationTokenSource.Cancel();
}

void onClosed()
{
    if (Utility.IsRunAsAdmin())
    {
        Console.WriteLine();
        var network = Utility.GetNetworkInterface();
        Utility.RunCommand("netsh", $"interface ipv4 set dns name=\"{network.Name}\" source=dhcp");

        Logger.Warn("DNS has been successfully disabled.");
    }
}

