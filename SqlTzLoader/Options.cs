using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using System.Configuration;

namespace SqlTzLoader
{

    internal class Options
    {
        private readonly HeadingInfo headingInfo = new HeadingInfo(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        
        [Option('c', "connectionString", Required = true, HelpText = "Connectionstring of database to update.\n\n Server=myServerAddress;Database=myDataBase;Trusted_Connection=True; \n Server=myServerName\\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;")]
        public string ConnectionString { get; set; }

        [Option('v', "verbose", HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [Option('s', "ProxyServer", Required = false, HelpText = "Proxy Server.")]
        public string ProxyServer { get; set; }

        [Option('p', "ProxyPort", Required = false, HelpText = "Proxy Port.")]
        public int ProxyPort { get; set; }


        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }
}
