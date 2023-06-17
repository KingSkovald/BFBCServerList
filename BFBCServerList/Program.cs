using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Spectre.Console;

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
                    BUType = (BUType)Enum.Parse(typeof(BUType), serverData[42], ignoreCase: true),
                    IsStable = new[] { "iad", "sjc", "gva" }
                        .Contains(serverData[6], StringComparer.InvariantCultureIgnoreCase)
                            && serverData[16] == "O"
                });
            }

            var rankedServers = GetStableServers(servers, BUType.Ranked);
            var unrankedServers = GetStableServers(servers, BUType.Unranked);

            AnsiConsole.MarkupLine($"[green1]Stable[/] ranked servers: {rankedServers.Count(x => x.IsStable)}");
            AnsiConsole.MarkupLine($"[red1]Unstable[/] ranked servers: {rankedServers.Count(x => !x.IsStable)}");

            if (rankedServers.Any())
            {
                RenderTable(
                    new TableTitle("RANKED SERVERS", new Style(decoration: Decoration.Bold)),
                    rankedServers);

                Console.WriteLine();
            }

            if (unrankedServers.Any())
            {
                RenderTable(
                    new TableTitle("UNRANKED SERVERS", new Style(decoration: Decoration.Bold)),
                    unrankedServers);

                Console.WriteLine();
            }

            return false;
        }

        private static void RenderTable(TableTitle title, IReadOnlyList<ServerInfo> servers)
        {
            var table = new Table
            {
                Border = TableBorder.Square,
                Title = title
            };

            AddTableColumns(table);
            AddTableRows(table, servers);

            AnsiConsole.Write(table);
        }

        private static void AddTableColumns(Table table) => table
            .AddColumns(
                new TableColumn(nameof(ServerInfo.Location)).Centered(),
                new TableColumn(nameof(ServerInfo.Name)).Centered(),
                new TableColumn(nameof(ServerInfo.Level)).Centered(),
                new TableColumn(nameof(ServerInfo.Mode)).Centered(),
                new TableColumn(nameof(ServerInfo.PlayerCount)).Centered(),
                new TableColumn(nameof(ServerInfo.Playgroup)).Centered(),
                new TableColumn(nameof(ServerInfo.Balance)).Centered());

        private static void AddTableRows(Table table, IReadOnlyList<ServerInfo> servers)
        {
            for (var i = 0; i < servers.Count; i++)
            {
                table.AddRow(
                    new Markup(servers[i].Location, new Style(foreground: servers[i].IsStable ? Color.Green1 : Color.Red1)),
                    new Markup(servers[i].Name),
                    new Markup(servers[i].Level),
                    new Markup(servers[i].Mode, new Style(foreground: servers[i].Mode == "GOLDRUSH" ? Color.Gold1 : Color.Blue)),
                    new Markup(servers[i].PlayerCount.ToString()),
                    new Markup(servers[i].Playgroup),
                    new Markup(servers[i].Balance));
            }
        }

        private static IReadOnlyList<ServerInfo> GetStableServers(IEnumerable<ServerInfo> servers, BUType buType) => servers
            .Where(x => x.Type == "O" && x.BUType == buType)
            .OrderBy(x => x.BUType)
            .OrderByDescending(x => x.IsStable)
            .ToList();
    }
}
