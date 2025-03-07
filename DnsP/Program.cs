using System.Net;
using System.Diagnostics;
using QRCoder;
using CommandLine;
using ConsoleTables;
using Ae.Dns.Server;
using Ae.Dns.Client;
using Ae.Dns.Protocol;
using Ae.Dns.Client.Filters;

bool _run = false;
bool _check = false;
string _name = "DnsP";
string _url = "https://github.com/SoheilMV";
string _site = string.Empty;
int _timeout = 5000;
CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

try
{
    Console.Title = _name;
    Database db = Database.Initialize();
    if (args.Length == 0)
    {
        Logger.Info("Usage: dnsp [command] [options]");
        Logger.Info("For help, use:\r\n  dnsp --help  ");
    }
    else
    {
        var arguments = ArgumentsParser.Parse(args);
        var result = Parser.Default.ParseArguments<ArgumentsOption>(arguments);
        result.WithParsed(options =>
        {
            if (!string.IsNullOrEmpty(options.Add))
            {
                if (File.Exists(options.Add))
                {
                    DnsModel model = new DnsModel(options.Add);
                    var dnsList = model.GetDnsList();
                    foreach (var dns in dnsList)
                    {
                        var ip = dns.DNS.GetAddress();
                        string name = dns.Name.Length > 1 ? dns.Name : "-";
                        IPAddress? iPAddress1 = ip[0];
                        IPAddress? iPAddress2 = ip[1];
                        if (iPAddress1 != null && iPAddress2 != null)
                        {
                            db.Add(iPAddress1.ToString(), iPAddress2.ToString(), name);
                        }
                    }
                    Logger.Info($"DNS was added to the list.");
                }
                else
                {
                    IPAddress?[] ip = options.Add.GetAddress();
                    string name = "-";
                    if (!string.IsNullOrEmpty(options.Name))
                        name = options.Name;

                    IPAddress? iPAddress1 = ip[0];
                    IPAddress? iPAddress2 = ip[1];
                    if (iPAddress1 != null && iPAddress2 != null)
                    {
                        if (db.Add(iPAddress1.ToString(), iPAddress2.ToString(), name))
                            Logger.Info($"DNS was added to the list.");
                        else
                            Logger.Error("DNS already exists, enter another DNS.");
                    }
                    else
                        Logger.Error("The IP address entered is not valid.");
                }
            }
            else if (!string.IsNullOrEmpty(options.Remove))
            {
                if (options.Remove == "all")
                {
                    db.Clear();
                    Logger.Info("DNS list was completely cleared.");
                }
                else if (db.Remove(options.Remove))
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
                    Logger.Error($"{options.Unblock} is not already blocked, select the host from the blocklist.");
            }
            else if (!string.IsNullOrEmpty(options.Check))
            {
                _check = true;
                _site = options.Check.Trim();
                _timeout = options.Timeout;
            }
            else if (options.Flush)
            {
                Utility.RunCommand("ipconfig", $"/flushdns");
                Logger.Info("Successfully flushed.");
            }
            else if (options.Protocol)
            {
                var protocol = db.ChangeProtocol();
                if (protocol == ClientsProtocol.UDP)
                    Logger.Info("DNS protocol changed to UDP.");
                else
                    Logger.Info("DNS protocol changed to TCP.");
            }
            else if (options.Mode)
            {
                var mode = db.ChangeMode();
                if (mode == ClientsMode.Racer)
                    Logger.Info("DNS mode changed to Racer.");
                else
                    Logger.Info("DNS mode changed to Random.");
            }
            else if (options.Visit)
            {
                string url = $"{_url}/DnsP";
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(new PayloadGenerator.Url(url));
                    using (AsciiQRCode qrCode = new AsciiQRCode(qrCodeData))
                    {
                        Utility.OpenUrl(url);
                        Logger.Custom(qrCode.GetGraphic(1, drawQuietZones: false), ConsoleColor.DarkYellow);
                    }
                }
            }
            else if (options.Environment)
            {
                var isOk = Utility.AddPathToUserEnvironment();
                if (isOk)
                    Logger.Info("DnsP has been successfully added to the user's PATH environment variable.");
                else
                    Logger.Error("DnsP is already present in the user's PATH environment variable.");
            }
            else if (options.Log)
            {
                Console.WriteLine();
                Logger.Info($"DNS Mode: {db.GetMode()}");
                Logger.Info($"DNS Protocol: {db.GetProtocol()}");
                Console.WriteLine();
                var table = new ConsoleTable("ID", "Name", "Primary", "Secondary");
                foreach (var item in db.list)
                {
                    table.AddRow(item.id, item.name, item.primary, item.secondary);
                }
                Logger.Info(table.ToMarkDownString());
            }
            else if (!string.IsNullOrEmpty(options.Run))
            {
                var dns = db.Find(options.Run);
                if (dns != null)
                {
                    _run = true;
                    db.Select(dns);
                    Logger.Custom(_name.ToLogo(), ConsoleColor.Magenta);
                    Logger.Custom(_url.ToCenter(), ConsoleColor.Magenta);
                    ClosingHandler.Create(onClosing, onClosed);
                }
                else
                {
                    Logger.Error("The provided DNS or ID was not found.\nPlease enter a valid value or use `dnsp --log` to view available options.");
                }
            }
            else
            {
                Logger.Error("The command is unknown.");
            }
        });
    }

    if (_run)
    {
        Logger.Warn("Press 'CTRL+C' to exit.");
        Logger.Warn($"DNS was implemented on the '{IPAddress.Any}'.");
        if (Utility.IsRunAsAdmin())
        {
            var network = Utility.GetNetworkInterface();
            Utility.RunCommand("netsh", $"interface ipv4 add dns name=\"{network.Name}\" address=127.0.0.1 index=1");
            Logger.Warn("DNS settings changed successfully.");
            Console.WriteLine();
        }
        else
        {
            if (Utility.RestartParentProcessAsAdmin(db.selectedDns))
            {
                Environment.Exit(0);
            }
            else
            {
                Logger.Error("DNS registration requires elevation (Run as administrator).");
                Console.WriteLine();
            }
        }

        ClientsMode mode = db.GetMode();
        ClientsProtocol protocol = db.GetProtocol();
        var clients = GetDnsClients(db.selectedDns, protocol);
        using IDnsClient clientMode = GetClientMode(mode, clients);
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
        using IDnsClient filterClient = new DnsFilterClient(dnsFilter, clientMode);
        using IDnsRawClient rawClient = new DnsRawClient(filterClient);
        using IDnsServer server = GetServer(rawClient, protocol);
        await server.Listen(_cancellationTokenSource.Token);
    }
    else if (_check)
    {
        Dictionary<DNS, bool> dnsMap = new Dictionary<DNS, bool>();
        int index = 1;
        foreach (var dns in db.list)
        {
            int count = 0;
            var dnsClients = GetDnsClients(dns, db.GetProtocol());
            string ip = string.Empty;
            foreach (var client in dnsClients)
            {
                var uri = new UriBuilder(client.ToString()!);
                ip = uri.Host;
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                try
                {
                    using HttpClient httpClient = new HttpClient(new DnsDelegatingHandler(client)
                    {
                        InnerHandler = new SocketsHttpHandler()
                        {
                            ConnectTimeout = TimeSpan.FromMilliseconds(_timeout),
                        }
                    });
                    httpClient.Timeout = TimeSpan.FromMilliseconds(_timeout);
                    HttpResponseMessage response = await httpClient.GetAsync(_site);
                    stopwatch.Stop();
                    if (response.StatusCode != HttpStatusCode.Forbidden)
                    {
                        Logger.Debug($"({index}/{db.list.Count * 2}) {uri.Host} successfully connected. {stopwatch.ElapsedMilliseconds}ms");
                        count++;
                    }
                    else
                    {
                        Logger.Error($"({index}/{db.list.Count * 2}) {uri.Host} is banned.");
                    }
                }
                catch
                {
                    stopwatch.Stop();
                    Logger.Error($"({index}/{db.list.Count * 2}) {uri.Host} is banned.");
                }
                index++;
            }
            dnsMap.Add(dns, count == 2);
        }
        Console.WriteLine();
        var table = new ConsoleTable("ID", "Name", "Primary", "Secondary", "Status");
        foreach (var item in dnsMap)
        {
            if (item.Value)
                table.AddRow(item.Key.id, item.Key.name, item.Key.primary, item.Key.secondary, "Connected");
            else
                table.AddRow(item.Key.id, item.Key.name, item.Key.primary, item.Key.secondary, "Banned");
        }
        Logger.Info(table.ToMarkDownString());
    }
}
catch (Exception ex)
{
    Logger.Error(ex.Message);
}

IDnsClient[] GetDnsClients(DNS? dns, ClientsProtocol protocol = ClientsProtocol.UDP)
{
    List<IDnsClient> dnsClients = new List<IDnsClient>();
    if (dns != null)
    {
        if (protocol == ClientsProtocol.TCP)
        {
            dnsClients.Add(new DnsTcpClient(IPAddress.Parse(dns.primary)));
            dnsClients.Add(new DnsTcpClient(IPAddress.Parse(dns.secondary)));
        }
        else
        {
            dnsClients.Add(new DnsUdpClient(IPAddress.Parse(dns.primary)));
            dnsClients.Add(new DnsUdpClient(IPAddress.Parse(dns.secondary)));
        }
    }
    return dnsClients.ToArray();
}

IDnsClient GetClientMode(ClientsMode mode, IDnsClient[] clients)
{
    if(mode == ClientsMode.Racer)
        return new DnsRacerClient(clients);
    else
        return new DnsRandomClient(clients);
}

IDnsServer GetServer(IDnsRawClient rawClient, ClientsProtocol protocol = ClientsProtocol.UDP)
{
    if (protocol == ClientsProtocol.UDP)
    {
        var serverOptions = new DnsUdpServerOptions
        {
            Endpoint = new IPEndPoint(IPAddress.Any, 53)
        };
        return new DnsUdpServer(rawClient, serverOptions);
    }
    else
    {
        var serverOptions = new DnsTcpServerOptions
        {
            Endpoint = new IPEndPoint(IPAddress.Any, 53)
        };
        return new DnsTcpServer(rawClient, serverOptions);
    }
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

