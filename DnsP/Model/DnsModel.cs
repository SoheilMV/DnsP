internal class DnsModel
{
    private readonly FileInfo _info;
    private List<string[]> _list;

    public DnsModel(string path)
    {
        _info = new FileInfo(path);
        _list = new List<string[]>();
    }

    public List<string[]> GetDns()
    {
        var lines = File.ReadAllLines(_info.FullName);
        foreach (var line in lines)
        {
            string[] csv = line.Trim().Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _list.Add(csv);
        }
        return _list;
    }
}