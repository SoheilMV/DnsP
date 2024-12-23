![DnsP](Images/dnsp.png)
![DnsP](Images/vscode.png)

# DnsP
DNS Provider

# :inbox_tray:Installation
1. Download the program and unzip it . 
2. Add the program path to the system environment.
3. Open **CMD**.
4. Enter **dnsp** to make sure it works.

# :books:Commands
| Short Name | Long Name  | Description                           |
|------------|------------|---------------------------------------|
| -a         | --add      | Add dns to the list.                  |
| -r         | --remove   | Remove dns from the list.             |
| -b         | --block    | Add host to blacklist.                |
| -l         | --unblock  | Remove host from the blacklist.       |
| -s         | --skip     | Skip dns.                             |
| -k         | --unskip   | Undo skip dns.                        |
| -c         | --check    | Find healthy dns.                     |
| -f         | --flush    | Flushing your previous dns addresses. |
| -p         | --protocol | Change the dns protocol.              |
| -m         | --mode     | Change the type of dns list usage.    |
| -v         | --visit    | Visit the project repository.         |
|            | --log      | Display the list of dns.              |
|            | --run      | Run local dns.                        |
|            | --help     | Display this help screen.             |
|            | --version  | Display version information.          |

# :computer:Using
> dnsp **[command]** **[value]**

### How to add dns?
> dnsp **[-a | --add] [DNS]**  
> dnsp **[-a | --add] [DNS] [-n | --name] [Name]**    
> dnsp **[-a | --add] [File]**
>
> **Example:**  
> dnsp -a 1.1.1.1,1.0.0.1   
> dnsp -a 1.1.1.1,1.0.0.1 -n cloudflare    
> dnsp -a C:\example.csv    

### How to remove dns?
> dnsp **[-r | --remove] [DNS or ID or all]**  
>
>
> **Example:**  
> dnsp -r 1.1.1.1    
> dnsp -r all    

### How to block the host?
> dnsp **[-b | --block] [Host]**  
>
> **Example:**  
> dnsp -b google.com

### How to unblock the host?
> dnsp **[-l | --unblock] [Host]**  
>
> **Example:**  
> dnsp -l google.com

### How to deactivate dns?
> dnsp **[-s | --skip] [DNS or ID or all]**  
>
> **Example:**  
> dnsp -s 1.1.1.1    
> dnsp -s all    

### How to activate dns?
> dnsp **[-k | --unskip] [DNS or ID or all]**  
>
> **Example:**  
> dnsp -k 1.1.1.1    
> dnsp -k all    

### How to check dns list?
> dnsp **[-c | --check] [Link]**  
> dnsp **[-c | --check] [Link] [-t | --timeout] [Millisecond]**  
>
> **Example:**  
> dnsp -c https://www.example.com/  
> dnsp -c https://www.example.com/ -t 2000

*Note: By default the timeout is set to 5000ms*

### How to use dns flushing?
> dnsp **[-f | --flush]**    

### How to change the dns protocol?
> dnsp **[-p | --protocol]**

### How to change the type of dns list usage?
> dnsp **[-m | --mode]**

### How do I get a report?
> dnsp --log  

### How to start local dns?
> dnsp --run  

# :bookmark:Credits
- [Ae.Dns](https://github.com/alanedwardes/Ae.Dns) (Pure C# implementation of UDP, TCP and HTTPS ("DoH") DNS clients + servers with configurable caching/filtering layers)
- [CommandLineParser](https://github.com/commandlineparser/commandline) (The best C# command line parser that brings standardized *nix getopt style, for .NET. Includes F# support)
- [ConsoleTables](https://github.com/khalidabuhakmeh/ConsoleTables) (Print out a nicely formatted table in a console application C#)
- [Figgle](https://github.com/drewnoakes/figgle) (ASCII banner generation for .NET)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) (Json.NET is a popular high-performance JSON framework for .NET)
- [QRCoder](https://github.com/codebude/QRCoder) (A pure C# Open Source QR Code implementation)
- [CsvHelper](https://github.com/JoshClose/CsvHelper) (Library to help reading and writing CSV files)