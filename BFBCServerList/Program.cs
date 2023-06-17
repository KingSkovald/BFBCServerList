using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace BFBCServerList
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("----------------------------BATTLEFIELD: BAD COMPANY PS3 SERVER MONITOR----------------------------");
            Console.WriteLine();
            Console.WriteLine("Simple wrapper for ealist.exe. Contact KingSkovald (PSN) in case you have any questions.");
            Console.WriteLine();
            Console.WriteLine("ealist.exe reference: https://aluigi.altervista.org/papers.htm#ealist.");
            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------------------------------------------------------");
            Console.WriteLine();

            var shouldRetry = true;
            var isFirstTry = true;

            while (shouldRetry)
            {
                shouldRetry = RunEAList(isFirstTry);
                isFirstTry = false;
            }

            Console.WriteLine();
            Console.WriteLine("Press \"Esc\" to exit");

            while (Console.ReadKey(intercept: true).Key != ConsoleKey.Escape)
            {
            }
        }

        private static bool RunEAList(bool isFirstTry)
        {
            Process eaList = null;
            string eaListResponse = null;

            try
            {
                eaList = new Process
                {
                    StartInfo =
                    {
                        FileName = "ealist.exe",
                        Arguments = "-n bfbc-ps3 -a liferkiller666 liferkiller666 bfbc-ps3 -X none",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                eaList.Start();

                if (isFirstTry)
                {
                    Console.WriteLine("Waiting for script to execute...");
                    Console.WriteLine();
                }

                eaListResponse = eaList.StandardOutput.ReadToEnd();

                eaList.WaitForExit();
            }
            catch (Win32Exception e)
            {
                if (e.Message.Contains("file"))
                {
                    Console.WriteLine("Failed to find ealist.exe.");
                    Console.WriteLine("Please ensure that ealist.exe is in the same folder as BFBCServerList.exe and re-run BFBCServerList.exe.");

                    return false;
                }

                throw;
            }
            finally
            {
                eaList.Close();
            }

            var output = eaListResponse.Split('\n');
            var servers = new List<ServerInfo>();

            if (output.Length < 10)
            {
                return true;
            }

            foreach (var line in output)
            {
                var serverData = line.Split('\\');

                if (serverData.Length < 10)
                {
                    continue;
                }

                servers.Add(new ServerInfo
                {
                    Location = serverData[6],
                    Level = serverData[24] == "GOLDRUSH"
                        ? LevelMaps.GoldRushMaps[serverData[10]]
                        : LevelMaps.ConquestMaps[serverData[10]],
                    Name = serverData[12],
                    IP = serverData[14],
                    Port = serverData[26],
                    Mode = serverData[24],
                    Playgroup = serverData[44],
                    PlayerCount = int.Parse(serverData[58]),
                    Balance = serverData[28],
                    Type = serverData[16],
                    BUType = serverData[42],
                    IsStable = new[] { "iad", "sjc", "gva" }
                        .Contains(serverData[6], StringComparer.InvariantCultureIgnoreCase)
                            && serverData[16] == "O"
                });
            }

            var openedServers = servers
                .Where(x => x.Type == "O")
                .OrderByDescending(x => x.IsStable)
                .ToList();

            Console.WriteLine($"Stable servers: {openedServers.Count(x => x.IsStable)}");
            Console.WriteLine($"Unstable servers: {openedServers.Count(x => !x.IsStable)}");
            Console.WriteLine();

            for (var i = 0; i < openedServers.Count; i++)
            {
                Console.WriteLine($"-------Server #{i + 1} ({(openedServers[i].IsStable ? "stable" : "unstable")})-------");
                Console.WriteLine($"{nameof(ServerInfo.Location)}: {openedServers[i].Location}");
                Console.WriteLine($"{nameof(ServerInfo.Level)}: {openedServers[i].Level}");
                Console.WriteLine($"{nameof(ServerInfo.Name)}: {openedServers[i].Name}");
                Console.WriteLine($"{nameof(ServerInfo.IP)}: {openedServers[i].IP}");
                Console.WriteLine($"{nameof(ServerInfo.Port)}: {openedServers[i].Port}");
                Console.WriteLine($"{nameof(ServerInfo.Mode)}: {openedServers[i].Mode}");
                Console.WriteLine($"{nameof(ServerInfo.Playgroup)}: {openedServers[i].Playgroup}");
                Console.WriteLine($"{nameof(ServerInfo.PlayerCount)}: {openedServers[i].PlayerCount}");
                Console.WriteLine($"{nameof(ServerInfo.Balance)}: {openedServers[i].Balance}");
                Console.WriteLine();
            }

            return false;
        }
    }
}
