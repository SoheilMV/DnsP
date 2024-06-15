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
            if (File.Exists(options.Add))
            {
                DnsModel model = new DnsModel(options.Add);
                var dns = model.GetDns();
                foreach (var csv in dns)
                {
                    IPAddress? ip = csv[0].GetAddress();
                    string name = csv.Length > 1 ? csv[1] : "-";
                    if (ip != null)
                    {
                        db.Add(ip.ToString(), name);
                    }
                }
                Logger.Info($"DNS was added to the list.");
            }
            else
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
        else if (!string.IsNullOrEmpty(options.Skip))
        {
            if (options.Skip == "all")
            {
                db.Skip();
                Logger.Info("All dns were skipped.");
            }
            else if (db.Skip(options.Skip))
                Logger.Info($"DNS is skipped.");
            else
                Logger.Error("DNS is not available in the list, choose DNS from the list.");
        }
        else if (!string.IsNullOrEmpty(options.Unskip))
        {
            if (options.Unskip == "all")
            {
                db.Unskip();
                Logger.Info("All dns are enabled.");
            }
            else if (db.Unskip(options.Unskip))
                Logger.Info($"DNS is used.");
            else
                Logger.Error("DNS is not available in the list, choose DNS from the list.");
        }
        else if (!string.IsNullOrEmpty(options.Check))
        {
            _check = true;
            _site = options.Check.Trim();
            _timeout = options.Timeout;
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
        else if (options.Log)
        {
            Console.WriteLine();
            Logger.Info($"DNS Mode: {db.GetMode()}");
            Logger.Info($"DNS Protocol: {db.GetProtocol()}");
            Console.WriteLine();
            var table = new ConsoleTable("ID", "DNS", "Skip", "Name");
            foreach (var item in db.list)
                table.AddRow(item.id, item.dns, item.skip, item.name);
            Logger.Info(table.ToMarkDownString());
        }
        else if (options.Run)
        {
            _run = true;

            Logger.Custom(_name.ToLogo(), ConsoleColor.Magenta);
            Logger.Custom(_url.ToCenter(), ConsoleColor.Magenta);

            ClosingHandler.Create(onClosing, onClosed);
            Logger.Warn("Press 'CTRL+C' to exit.");
        }
        else
        {
            Logger.Error("The command is unknown.");
        }
    });

    if (_run)
    {
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
            Logger.Error("DNS registration requires elevation (Run as administrator).");
            Console.WriteLine();
        }

        ClientsMode mode = db.GetMode();
        ClientsProtocol protocol = db.GetProtocol();
        var clients = GetDnsClients(db, protocol);
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
        Console.WriteLine();
        int index = 0;
        var dnsClients = GetDnsClients(db, db.GetProtocol(), false);
        foreach (var client in dnsClients)
        {
            index++;
            var uri = new UriBuilder(client.ToString()!);
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
                    db.Unskip(uri.Host);
                    Logger.Debug($"({index}/{dnsClients.Count}) {uri.Host} successfully connected. {stopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    db.Skip(uri.Host);
                    Logger.Error($"({index}/{dnsClients.Count}) {uri.Host} is banned.");
                }
            }
            catch
            {
                stopwatch.Stop();
                db.Skip(uri.Host);
                Logger.Error($"({index}/{dnsClients.Count}) {uri.Host} is banned.");
            }
        }
        Console.WriteLine();
        Logger.Warn($"DnsP is ready to use in '{_site}'");
    }
}
catch (Exception ex)
{
    Logger.Error(ex.Message);
}

List<IDnsClient> GetDnsClients(Database db, ClientsProtocol protocol = ClientsProtocol.UDP, bool useSkip = true)
{
    List<IDnsClient> dnsClients = new List<IDnsClient>();
    foreach (DNS dns in db.list)
    {
        if (useSkip)
        {
            if (!dns.skip)
            {
                if (protocol == ClientsProtocol.TCP)
                    dnsClients.Add(new DnsTcpClient(IPAddress.Parse(dns.dns)));
                else
                    dnsClients.Add(new DnsUdpClient(IPAddress.Parse(dns.dns)));
            }
        }
        else
        {
            if (protocol == ClientsProtocol.TCP)
                dnsClients.Add(new DnsTcpClient(IPAddress.Parse(dns.dns)));
            else
                dnsClients.Add(new DnsUdpClient(IPAddress.Parse(dns.dns)));
        }
    }
    return dnsClients;
}

IDnsClient GetClientMode(ClientsMode mode, List<IDnsClient> clients)
{
    if(mode == ClientsMode.Racer)
        return new DnsRacerClient(clients.ToArray());
    else
        return new DnsRandomClient(clients.ToArray());
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

