using YamlDotNet;
using YamlDotNet.Serialization;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace conanlauncher
{
    public class GlobalConfiguration
    {
        private string steampath;
        private string steam;
        private string steamuser;
        private List<ConanServer> servers = new List<ConanServer>();

        public GlobalConfiguration()
        {            
        }

        public static string ConfigFile()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/conanlauncher.yaml";
        }

        public static GlobalConfiguration Load()
        {
            Deserializer deserializer = new DeserializerBuilder().Build();
            GlobalConfiguration config = null;

          
            using (Stream stream = File.OpenRead(ConfigFile()))
            {
                using (TextReader reader = new StreamReader(stream))
                {
                    config = deserializer.Deserialize<GlobalConfiguration>(reader);
                }
            }

            return config;
        }

        public void Save()
        {
            Serializer serializer =  new SerializerBuilder().Build();

            using (Stream stream = File.OpenWrite(ConfigFile()))
            {
                using (TextWriter writer = new StreamWriter(stream))
                {
                    serializer.Serialize(writer, this);
                }
            }
        }

        public string SteamPath 
        {
            get { return steampath; }
            set { steampath = value; }
        }

        public string SteamUser 
        {
            get { 
                if (steamuser == null) {
                    return "anonymous";
                } else {
                    return steamuser;
                } 
            }
            set { steamuser = value; }
        }

        public string Steam 
        {
            get { return steam; }
            set { steam = value; }
        }

        public List<ConanServer> Servers
        {
            get { return servers; }
            set { servers = value; }
        }

        [YamlIgnore]
        public string SteamEXE
        {
            get { return Path.Combine(steam, "steam.exe"); }
        }

        [YamlIgnore]
        public string SteamCMD
        {
            get { return Path.Combine(steam, "steamcmd.exe"); }
        }

        [YamlIgnore]
        public string WorkshopPath
        {
            get { return Path.Combine(steampath, "steamapps\\workshop\\content\\440900"); }
        }

        [YamlIgnore]
        public string ModList
        {
            get { return Path.Combine(steampath, "steamapps\\common\\Conan Exiles\\ConanSandbox\\Mods\\modlist.txt"); }
        }

        [YamlIgnore]
        public string GamePath
        {
            get { return Path.Combine(steampath, "steamapps\\common\\Conan Exiles\\"); }
        }
    }
}