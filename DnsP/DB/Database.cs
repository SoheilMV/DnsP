using Newtonsoft.Json;

internal class Database
{
    private static string _path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    public string Protocol { get; set; }
    public string Mode { get; set; }
    public List<DNS> list { get; set; }
    public List<string> blacklist { get; set; }

    public Database()
    {
        Protocol = "UDP";
        Mode = "Racer";
        list = new List<DNS>();
        blacklist = new List<string>();
    }

    public bool Add(string dns, string name)
    {
        bool result = false;
        if (!(list.Where(d => d.dns == dns).ToArray().Length > 0))
        {
            if (list.Count > 0)
            {
                DNS d = list[list.Count - 1];
                list.Add(new DNS()
                {
                    id = d.id + 1,
                    name = name,
                    skip = false,
                    dns = dns
                });
            }
            else
            {
                list.Add(new DNS()
                {
                    id = 0,
                    name = name,
                    skip = false,
                    dns = dns
                });
            }
            result = true;
            Write();
        }    
        return result;
    }

    public bool Remove(string dnsOrId)
    {
        bool result = false;
        DNS[]? dns = list.Where(d => d.dns == dnsOrId || d.id.ToString() == dnsOrId).ToArray();
        if (dns != null || dns?.Length > 0)
        {
            foreach (var item in dns)
            {
                list.Remove(item);
                result = true;
            }
            Write();
        }
        return result;
    }

    public void Clear()
    {
        list.Clear();
        blacklist.Clear();
        Write();
    }

    public bool Block(string host)
    {
        bool result = false;
        if (!blacklist.Contains(host))
        {
            blacklist.Add(host);
            Write();
            result = true;
        }
        return result;
    }

    public bool Unblock(string host)
    {
        bool result = false;
        if (blacklist.Contains(host))
        {
            blacklist.Remove(host);
            Write();
            result = true;
        }
        return result;
    }

    public bool Skip(string dnsOrId)
    {
        bool result = false;
        var dns = list.Where(d => d.dns == dnsOrId || d.id.ToString() == dnsOrId);
        foreach (var item in dns)
        {
            item.skip = true;
            result = true;
        }
        Write();
        return result;
    }

    public void Skip()
    {
        foreach (var dns in list)
        {
            dns.skip = true;
        }
        Write();
    }

    public bool Unskip(string dnsOrId)
    {
        bool result = false;
        var dns = list.Where(d => d.dns == dnsOrId || d.id.ToString() == dnsOrId);
        foreach (var item in dns)
        {
            item.skip = false;
            result = true;
        }
        Write();
        return result;
    }

    public void Unskip()
    {
        foreach (var dns in list)
        {
            dns.skip = false;
        }
        Write();
    }

    public ClientsProtocol GetProtocol()
    {
        return Protocol == "UDP" ? ClientsProtocol.UDP : ClientsProtocol.TCP;
    }

    public ClientsProtocol ChangeProtocol()
    {
        if (Protocol == "UDP")
            Protocol = "TCP";
        else
            Protocol = "UDP";
        Write();
        return Protocol == "UDP" ? ClientsProtocol.UDP : ClientsProtocol.TCP;
    }

    public ClientsMode GetMode()
    {
        return Mode == "Racer" ? ClientsMode.Racer : ClientsMode.Random;
    }

    public ClientsMode ChangeMode()
    {
        if (Mode == "Racer")
            Mode = "Random";
        else
            Mode = "Racer";
        Write();
        return Mode == "Racer" ? ClientsMode.Racer : ClientsMode.Random;
    }

    public bool HostIsBlock(string host)
    {
        return blacklist.Contains(host);
    }

    public void Write()
    {
        string dir = GetFolder();
        string? json = JsonConvert.SerializeObject(this);
        File.WriteAllText(Path.Combine(dir, "database.json"), json);
    }

    public static Database Initialize()
    {
        string dir = GetFolder();
        string path = Path.Combine(dir, "database.json");
        if (File.Exists(path))
        {
            string? json = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(json))
            {
                Database? db = JsonConvert.DeserializeObject<Database>(json);
                return db ?? new Database();
            }
        }
        return new Database();
    }

    public static string GetFolder()
    {
        string path = Path.Combine(_path, "dnsp");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return path;
    }
}
