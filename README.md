# conanlauncher

ConanLauncher doesn't actually launch Conan: Exiles, but it helps you set up mods
so that you can play on different servers more easily.

## Requirements

* Steam (running)
* ```steamcmd.exe``` (which can be found [here](https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip))
  in the same folder as your regular ```steam.exe``` (just download it, and drop it in there).
* Conan: Exiles (duh)

## Setup

Download and install conanlauncher. Make sure ```steamcmd.exe``` is where ```steam.exe``` is.
Then tell conanlauncher where where Conan is installed. Point it towards the base steam path:

```bash
$ conanlauncher steampath Z:\steam
```

If this is not also the path where you have your ```steamcmd.exe``` and ```steam.exe```, then
provide that also:

```bash
$ conanlauncher steam D:\steam
```

Now you can add servers by IP to conanlauncher:

```bash
$ conanlauncher add --host 160.202.167.18 --port 7787
```

This will add the Conan server to the list and you are then ready to use:

```bash
$ conanlauncher query RP-Server
Server: 1800-RP-Server
Port: 7787
Query port: 26015
Modlist:
  - 880454836
  - 1206493209
  - 1369802940
```

This shows you the server information, and also the mods that are used on that server. You can now prepare your
Conan: Exiles game for that server by doing:

```
$ conanlauncher setup RP-Server
```

This will download or update the needed mods and write a modlist.txt. Afterwards just launch Conan: Exiles and
join the server.

## Configuration File

The conanlauncher configuration file can be found in ```$UserDir\Documents\conanlauncher.yaml```.