using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace conanlauncher
{
    public class ConanServer
    {        
        private string hostname;
        private string port;
        private string queryport;
        private string name;

        private List<string> modlist = new List<string>();
    
        public ConanServer()
        {            
        }

        public ConanServer(string host, string port)
        {
            this.hostname = host;
            this.port = port;
            this.queryport = "27015";
        }

        public ConanServer(string host, string port, string queryport)
        {
            this.hostname = host;
            this.port = port;
            this.queryport = queryport;
        }

        public void Update()
        {
            // Query query port
            this.queryport = RconQuery.QueryServerInfo(hostname, Steam.CONAN_EXILES_APP_ID).ToString();

            RconQuery query = new RconQuery(hostname, queryport);

            SourceServerInfo info = query.QueryInfo();
            Dictionary<string, string> rules = query.QueryRules();

            if (rules != null) {
                // Modlist is stored in a variable called S17_s
                // it contains as first line a number of mods enabled, and then
                // a list of workshop IDs
                string modlist = rules.GetValueOrDefault("S17_s", "0:0\n");
                string[] mods = modlist.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                if (mods.Length > 1) {
                    // we have some mods, not just a list
                    mods = mods.Skip(1).ToArray();
                    this.modlist.Clear();
                    this.modlist.AddRange(mods);
                }
            }

            if (info != null) {
                this.name = info.Name;
            }
        }

        public string Hostname 
        {
            get { return hostname; }
            set { hostname = value; }
        }

        public string Port 
        {
            get { return port; }
            set { port = value; }
        }

        public string QueryPort 
        {
            get { return queryport; }
            set { queryport = value; }
        }

        public string Name 
        {
            get { return name; }
            set { name = value; }
        }

        public List<string> ModList
        {
            get { return modlist; }
            set { modlist = value; }
        }
    }
}