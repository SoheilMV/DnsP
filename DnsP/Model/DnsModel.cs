using System.Formats.Asn1;
using System.Globalization;
using System;
using CsvHelper;
using CsvHelper.Configuration;
using DnsP;

internal class DnsModel
{
    private readonly FileInfo _info;
    private List<DnsTable> _list;

    public DnsModel(string path)
    {
        _info = new FileInfo(path);
        _list = new List<DnsTable>();
    }

    public List<DnsTable> GetDnsList()
    {
        try
        {
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim(),
                HasHeaderRecord = false
            };
            using (var reader = new StreamReader(_info.FullName))
            using (var csv = new CsvReader(reader, config))
            {
                var records = csv.GetRecords<DnsTable>();
                foreach (var record in records)
                {
                    _list.Add(record);
                }
            }
        }
        catch
        {
        }
        return _list;
    }
}