using System;
using NDesk.Options;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace conanlauncher
{
    class Program
    {

        static void SetSteamPath(string[] args)
        {
            GlobalConfiguration config = null;
            string path;

            if (args.Length < 1) {
                Console.WriteLine("error: no gamepath specified");
                Environment.Exit(1);
            }

            path = args[0];

            try {
                config = GlobalConfiguration.Load();
            } catch (Exception) {}

            if (config == null) {
                config = new GlobalConfiguration();
            }

            config.SteamPath = path;
            if (!Directory.Exists(config.GamePath)) {
                Console.WriteLine(String.Format("The path '{0}' was not found, please provide the correct path to the game", config.GamePath));
                Environment.Exit(1);
            }
            // Game path seems to be correct, so safe. And now try to find steam.exe
            config.Save();

            if (!File.Exists(Path.Combine(path, "steamcmd.exe")) && !File.Exists(Path.Combine(path, "steam.exe"))) {
                Console.WriteLine("'steamcmd.exe' and 'steam.exe' was not found in this path. Perhaps you have installed steam somewhere else?");
                Console.WriteLine("If so, please provide the path to 'steamcmd.exe' by doing: 'conanlauncher steam <path-to-steamcmd.exe>'");
            } else {
                config.Steam = path;
                config.Save();
            }
        }

        static void SetSteam(string[] args)
        {
            GlobalConfiguration config = null;
            string path;

            if (args.Length < 1) {
                Console.WriteLine("error: no gamepath specified");
                Environment.Exit(1);
            }

            path = args[0];

            try {
                config = GlobalConfiguration.Load();
            } catch (Exception) {}

            if (config == null) {
                config = new GlobalConfiguration();
            }

            if (File.Exists(Path.Combine(path, "steam.exe")) &&
                File.Exists(Path.Combine(path, "steam.exe"))) {
                config.Steam = path;
                config.Save();
            } else {
                Console.WriteLine("error: 'steam.exe' and 'steamcmd.exe' was not found in {0}", path);
                Environment.Exit(2);
            }
        }

        static void AddServer(string[] args)
        {
            GlobalConfiguration config = null;
            string hostname = null, port = "7777", queryport = "27015";

            var p = new OptionSet()
                .Add("h|host=", delegate(string v) { hostname = v; })
                .Add("p|port=", delegate(string v) { port = v; })
                .Add("q|query=", delegate(string v) { queryport = v; })
                ;

            List<string> extra = p.Parse(args);

            if (hostname == null && extra.Count > 0) {
                hostname = extra[0];
            }

            if (hostname == null) {
                throw new ApplicationException("no hostname set");
            }
            /* failure to load is not fatal in this step
             */
            try {
                config = GlobalConfiguration.Load();
            } catch (Exception) {}

            if (config == null) {
                config = new GlobalConfiguration();
            }

            ConanServer server = new ConanServer(hostname, port, queryport);
            try {
                server.Update();
                config.Servers.Add(server);
                config.Save();
            } catch (Exception e) {
                Console.WriteLine("error: failed to add and query port, probably query port is wrong");
                Console.WriteLine("details: " + e.Message);
                Environment.Exit(3);
            }
        }

        static void QueryServer(string[] args)
        {
            if (args.Length < 1) {
                Console.WriteLine("error: no server provided");
                Environment.Exit(1);
            }

            GlobalConfiguration config = GlobalConfiguration.Load();

            for (int i = 0; i < args.Length; i++) {
                var servers = from s in config.Servers
                              where s.Hostname.Equals(args[i], StringComparison.InvariantCultureIgnoreCase) || 
                                    s.Name.Contains(args[i], StringComparison.InvariantCultureIgnoreCase)
                              select s;

                foreach (var server in servers) {
                    server.Update();
                    Console.WriteLine("Server: " + server.Name);
                    Console.WriteLine("Port: " + server.Port);
                    Console.WriteLine("Query port: " + server.QueryPort);
                    if (server.ModList.Count() > 0) {
                        Console.WriteLine("Modlist:");
                        foreach (var mod in server.ModList) {
                            Console.WriteLine("  - " + mod);
                        }
                    }
                }
            }
        }

        static void WriteModList(GlobalConfiguration config, string[] modlist)
        {
            Stream stream = File.Create(config.ModList);
            StreamWriter writer = new StreamWriter(stream);

            foreach(string mod in modlist) {
                string modpath = Path.Combine(config.WorkshopPath, mod);
                string[] modfiles = Directory.GetFiles(modpath, "*.pak");
                
                foreach (string modfile in modfiles) {
                    writer.WriteLine(modfile);
                }
            }

            writer.Close();
        }

        static void Setup(string[] args)
        {
            if (args.Length < 1) {
                Console.WriteLine("error: no server provided");
                Environment.Exit(1);
            }

            GlobalConfiguration config = GlobalConfiguration.Load();
            Steam steam = new Steam(config.SteamCMD, config.SteamEXE, config.SteamPath, config.SteamUser);
            string serv = args[0];

            var servers = from s in config.Servers
                          where s.Hostname.Equals(serv, StringComparison.InvariantCultureIgnoreCase) || 
                                s.Name.Contains(serv, StringComparison.InvariantCultureIgnoreCase)
                          select s;
            
            if (servers == null || servers.Count() == 0) {
                Console.WriteLine("error: no such server: " + serv);
                Environment.Exit(3);
            }

            ConanServer server = servers.ToArray()[0];
            Console.WriteLine("Updating server information...");
            server.Update();
            if (server.ModList.Count() > 0) {
                Console.WriteLine("Updating mods...");
                steam.DownloadWorkShopItems(Steam.CONAN_EXILES_APP_ID, server.ModList.ToArray());
                Console.WriteLine("Writing updated modlist...");
                WriteModList(config, server.ModList.ToArray());
            }
            Console.WriteLine("Updating base game...");
            steam.UpdateGame(Steam.CONAN_EXILES_APP_ID);
            Console.WriteLine("Conan Exile's is now ready to be launched!");
        }

        static void Usage()
        {
            Console.WriteLine("conanlauncher:");
            Console.WriteLine("  add --host ip --port port --query queryport");
            Console.WriteLine("  steampath <path-to-conanfiles>");
            Console.WriteLine("  steam <path-to-steam.exe>");
            Console.WriteLine("  setup [server]");
            Console.WriteLine("  query [server]");
        }

        static void Main(string[] args)
        {
            try  {
                if (args.Length <= 0) {
                    Usage();
                    Environment.Exit(0);
                }

                string cmd = args[0].ToLower();
                args = args.Skip(1).ToArray();

                if (cmd == "add") {
                    AddServer(args);
                } else if (cmd == "query") {
                    QueryServer(args);
                } else if (cmd == "steampath") {
                    SetSteamPath(args);
                } else if (cmd == "steam") {
                    SetSteam(args);
                } else if (cmd == "setup") {
                    Setup(args);
                }
            } catch (Exception e) {
                Console.WriteLine("error: " + e.ToString());
            }
        }
    }
}
