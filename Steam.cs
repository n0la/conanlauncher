using System;
using System.IO;
using System.Diagnostics;

namespace conanlauncher
{
    public class Steam
    {
        public static string CONAN_EXILES_APP_ID = "440900";

        private string steamcmd;
        private string steamexe;
        private string steampath;
        private string steamuser;

        public Steam(string steamcmd, string steamexe, string path, string user)
        {
            this.steamcmd = steamcmd;
            this.steamexe = steamexe;
            this.steampath = path;
            this.steamuser = user;
        }

        public void UpdateGame(string appid)
        {
            string args = String.Format("+login {0} +app_update {1} +quit", steamuser, appid);
            Console.WriteLine(String.Format("'{0}' {1}", steamcmd, args));

            Process p = new Process();

            p.StartInfo.FileName = steamcmd;
            p.StartInfo.Arguments = args;
            p.Start();
            p.WaitForExit();

            if (p.ExitCode != 0) {
                throw new ApplicationException("failed to update steam workshop mods");
            }
        }

        public void RunApp(string appid, string param)
        {
            string args = String.Format("-applaunch {0} {1}", appid, param);
            Console.WriteLine(String.Format("'{0}' {1}", steamexe, args));

            Process p = new Process();
            p.StartInfo.FileName = steamexe;
            p.StartInfo.Arguments = args;
            p.Start();
            p.WaitForExit();

            if (p.ExitCode != 0) {
                throw new ApplicationException("failed to update steam workshop mods");
            }
        }

        public void DownloadWorkShopItems(string appid, string[] workshopid)
        {
            string args = String.Format("+login {0} +force_install_dir {1} ", steamuser, steampath);
            
            foreach (string mod in workshopid) {
                args += String.Format("+workshop_download_item {0} {1} ", appid, mod);
            }

            args += "validate +logout +quit";
            Console.WriteLine(String.Format("'{0}' {1}", steamcmd, args));

            Process p = new Process();

            p.StartInfo.FileName = steamcmd;
            p.StartInfo.Arguments = args;
            p.Start();
            p.WaitForExit();

            if (p.ExitCode != 0) {
                throw new ApplicationException("failed to update steam workshop mods");
            }
        }
    }
}