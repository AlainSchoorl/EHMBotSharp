using EHMBot.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Menu = EasyConsole.Menu;

namespace Setup
{
    internal class Program
    {
        private static EhmBotSettings oldSettings;
        private static EhmBotSettings ehmBotSettings;
        private static bool initialSetup;

        private static readonly Dictionary<string, string> pointList = new Dictionary<string, string>
        {
            { "Save game OK", "The confirmation button in the save game submenu." },
            { "Save game yes", "The confirmation to replace a save file." },
            { "Scroll to middle", "The middle of the scroll bar on the GM status page. Clicking this point makes the list jump to the middle." },
            { "Scroll to end", "The bottom of the scroll bar on the GM status page. Clicking this point makes the list jump to the end." },
            { "Scroll to top", "The top of the scroll bar on the GM status page. Clicking this point makes the list jump back to the start." },
            { "Status top left", "A bit up and left of the first GM status. (playing/watching/on vacation)" },
            { "Status bottom right", "A bit down and right of the first GM status. (leave enough room for longer statuses)" },
            { "Last status", "A bit up and left of the last visible GM status. (about as far up from this last status as you did for the first status)" },
            { "Status name", "Over a GM name in the GM status list" },
            { "GM name", "The GM name button, between the roster button and the NHL logo." },
            { "Go on vacation", "The go on vacation button." },
            { "Set days", "The set days option when going on vacation." },
            { "Load network game", "Save game load when restoring the game." },
            { "Load game OK", "The confirmation button in the load game submenu." },
            { "Restore scroll", "The part of the scroll bar that needs to be clicked to view the host GM. \n Set this only if you need to scroll down the GM list when restoring. Leave blank or as 0,0 otherwise." },
            { "Host", "The button to connect as the host GM." },
            { "Host log in yes", "The button to resume control of the host GM." },
            { "Finish", "The button to finish restoring the game." },
            { "Anywhere", "Any place on the game that's not a button but will stop the game from simming after restore." }
        };

        private static void Main(string[] args)
        {
            var fileExists = File.Exists(Path.Combine(System.AppContext.BaseDirectory, EhmBotSettings.SETTINGS_FILE_NAME));
            if (!fileExists) FirstSetup();
            else
            {
                using (var file = new StreamReader(Path.Combine(System.AppContext.BaseDirectory, EhmBotSettings.SETTINGS_FILE_NAME)))
                {
                    var json = file.ReadToEnd();
                    oldSettings = JsonConvert.DeserializeObject<EhmBotSettings>(json);
                    ehmBotSettings = JsonConvert.DeserializeObject<EhmBotSettings>(json);
                }
                StartingMenu();
            }
        }

        private static void FirstSetup()
        {
            oldSettings = new EhmBotSettings();
            ehmBotSettings = new EhmBotSettings();
            Console.WriteLine("Welcome to Alain's EHM Save Bot Version 2.0 :)");
            Console.WriteLine($"Looks like this is your first time starting the bot, if this is incorrect, please make sure the {EhmBotSettings.SETTINGS_FILE_NAME} file is present.");
            Console.WriteLine("Do you want to proceed to the initial setup? (y/n)");
            var yesNo = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(yesNo) && yesNo[0] == 'y')
            {
                initialSetup = true;
                Console.WriteLine("Please set F11 to open the General Manager Status page in EHM, and make sure F9 is 'Save Game' then press any key to continue.");
                Console.ReadKey();
                Console.WriteLine(string.Empty);
                SetBotToken();
            }
            else
            {
                StartingMenu();
            }
        }

        private static void StartingMenu()
        {
            var menu = new Menu()
                .Add("Set bot token", SetBotToken)
                .Add("Set General Managers order", SetGmOrder)
                .Add("Set VNC role name", SetVncRoleName)
                .Add("Set host position", SetHostPosition)
                .Add("Set whether save requests should be deleted", SetDeleteSaveRequest)
                .Add("Set how long a save takes", SetMillisecondsToSave)
                .Add("Set the save announce channel", SetSaveAnnounceChannel)
                .Add("Set bot request channel", SetSaveRequestChannel)
                .Add("Set save location", SetSaveLocation)
                .Add("Set game exe location", SetGameLocation)
                .Add("Set save prefix", SetSavePrefix)
                .Add("Set rolling save amount", SetRollingSaveAmount)
                .Add("Set default sim length", SetDefaultSimLength)
                .Add("Set click positions", SetClickPositions)
                .Add("View settings", View)
                .Add("Save settings", Save);
            menu.Display();

        }

        private static void SetBotToken()
        {
            Console.WriteLine("The bot token is used to connect to your unique discord bot.\n" +
                              "Go to https://discordapp.com/developers/applications to make a bot or find a bot's token");
            Console.WriteLine($"Current bot token is : {ehmBotSettings.BotToken}");
            Console.WriteLine("Please fill in the new bot token: (leave empty to keep the current token)");
            var botToken = Console.ReadLine()?.Trim();
            if (!(botToken == null || string.IsNullOrWhiteSpace(botToken))) ehmBotSettings.BotToken = botToken;
            if (initialSetup) SetGmOrder();
            else StartingMenu();
        }

        private static void SetGmOrder()
        {
            Console.WriteLine("Please fill in all team names, as ordered on the GM Status page.");
            var teamDictionary = ehmBotSettings.GeneralManagers ?? new Dictionary<int, GeneralManager>();
            for (var i = 1; i <= 30; i++)
            {
                Console.WriteLine($"Team {i}:");
                var teamName = Console.ReadLine()?.Trim();
                if (teamName == null)
                {
                    i--;
                    Console.WriteLine("Couldn't match a team, please try again.");
                    continue;
                }
                var team = TeamList.GetTeam(teamName);
                if (team == null)
                {
                    i--;
                    Console.WriteLine("Couldn't match a team, please try again.");
                    continue;
                }
                var gm = new GeneralManager { Team = team };
                teamDictionary[i] = gm;
            }
            ehmBotSettings.GeneralManagers = teamDictionary;
            if (initialSetup) SetVncRoleName();
            else StartingMenu();
        }

        private static void SetVncRoleName()
        {
            Console.WriteLine("What is the name of the VNC role? (leave blank to leave unchanged.)");
            Console.WriteLine($"Current vnc role name : {ehmBotSettings.VncRoleName}");
            var vncRoleName = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(vncRoleName))
                ehmBotSettings.VncRoleName = vncRoleName;
            if (initialSetup) SetHostPosition();
            else StartingMenu();
        }

        private static void SetHostPosition()
        {
            Console.WriteLine("Which position in the GM status list is the host? (leave blank to leave unchanged.)");
            Console.WriteLine($"Current host position is : {ehmBotSettings.HostPosition}");
            var hostPosition = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(hostPosition))
                ehmBotSettings.HostPosition = int.Parse(hostPosition);
            if (initialSetup) SetDeleteSaveRequest();
            else StartingMenu();
        }

        private static void SetDeleteSaveRequest()
        {
            Console.WriteLine("Should the bot delete save requests? (y/n)");
            Console.WriteLine($"Currently : {ehmBotSettings.DeleteSaveRequest}");
            var yesNo = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(yesNo) && yesNo[0] == 'y')
                ehmBotSettings.DeleteSaveRequest = true;
            else
                ehmBotSettings.DeleteSaveRequest = false;
            if (initialSetup) SetMillisecondsToSave();
            else StartingMenu();
        }

        private static void SetMillisecondsToSave()
        {
            Console.WriteLine("What is the maximum amount of time the game takes to save? (in milliseconds, leave blank to leave unchanged.)");
            Console.WriteLine($"Current milliseconds to save is : {ehmBotSettings.MillisecondsToSave}");
            var millisecondsToSave = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(millisecondsToSave))
                ehmBotSettings.MillisecondsToSave = int.Parse(millisecondsToSave);
            if (initialSetup) SetSaveAnnounceChannel();
            else StartingMenu();
        }

        private static void SetSaveAnnounceChannel()
        {
            Console.WriteLine("Specify the ID of the channel in which the bot should announce saves.\n" +
                              "(To get a channel's ID, right click a channel and 'Copy Id' while in developer mode)");
            Console.WriteLine($"Current channel ID: {ehmBotSettings.SaveAnnounceChannel}. Leave blank to leave unchanged.");
            var channel = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(channel)) ehmBotSettings.SaveAnnounceChannel = channel;
            if (initialSetup) SetSaveRequestChannel();
            else StartingMenu();
        }

        private static void SetSaveRequestChannel()
        {
            Console.WriteLine("Specify the ID of the channel in which saves and other commands may be requested.\n" +
                              "(To get a channel's ID, right click a channel and 'Copy Id' while in developer mode)");
            Console.WriteLine($"Current channel ID: {(string.IsNullOrWhiteSpace(ehmBotSettings.BotRequestChannel) ? "ALL" : ehmBotSettings.BotRequestChannel)}. " +
                              $"Leave blank to leave unchanged. Type in ALL to allow requests in all channels");
            var channel = Console.ReadLine()?.Trim();
            if (!(channel == null || string.IsNullOrWhiteSpace(channel)))
                ehmBotSettings.BotRequestChannel = channel.ToUpperInvariant() == "ALL" ? string.Empty : channel;
            if (initialSetup) SetSaveLocation();
            else StartingMenu();
        }

        private static void SetSaveLocation()
        {
            Console.WriteLine("Set the location in which the EHM saves are located.");
            Console.WriteLine($"Current save location: {ehmBotSettings.SaveLocation}. " +
                              $"Leave blank to leave unchanged. Type in DEFAULT to set it to C:/Users/Documents/Admin/Sports Interactive/EHM/games");
            var saveLocation = Console.ReadLine()?.Trim();
            if (!(saveLocation == null || string.IsNullOrWhiteSpace(saveLocation)))
                ehmBotSettings.SaveLocation = saveLocation.ToUpperInvariant() == "DEFAULT" ? @"C:\Users\Documents\Admin\Sports Interactive\EHM\games" : saveLocation;
            if (initialSetup) SetGameLocation();
            else StartingMenu();
        }

        private static void SetGameLocation()
        {
            Console.WriteLine("Set the location of the game exe.");
            Console.WriteLine($"Current save location: {ehmBotSettings.GameLocation}. " +
                              $"Leave blank to leave unchanged. Type in DEFAULT to set it to C:\\Program Files (x86)\\Steam\\steamapps\\common\\Eastside Hockey Manager");
            var gameLocation = Console.ReadLine()?.Trim();
            if (!(gameLocation == null || string.IsNullOrWhiteSpace(gameLocation)))
                ehmBotSettings.GameLocation = gameLocation.ToUpperInvariant() == "DEFAULT" ? "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Eastside Hockey Manager" : gameLocation;
            if (initialSetup) SetSavePrefix();
            else StartingMenu();
        }

        private static void SetSavePrefix()
        {
            Console.WriteLine("Specify the prefix for save file names.");
            Console.WriteLine($"Current save prefix: {ehmBotSettings.SavePrefix}. Leave blank to leave unchanged.");
            var savePrefix = Console.ReadLine()?.Trim();
            if (!(savePrefix == null || string.IsNullOrWhiteSpace(savePrefix)))
                ehmBotSettings.SavePrefix = savePrefix;
            if (initialSetup) SetRollingSaveAmount();
            else StartingMenu();
        }

        private static void SetRollingSaveAmount()
        {
            Console.WriteLine("Specify the amount of saves to roll.");
            Console.WriteLine($"Current rolling save amount: {ehmBotSettings.RollingSaveAmount}. Leave blank to leave unchanged.");
            var rollingSaveAmount = Console.ReadLine()?.Trim();
            if (!(rollingSaveAmount == null || string.IsNullOrWhiteSpace(rollingSaveAmount)))
                if (int.TryParse(rollingSaveAmount, out var rollingSaveAmountInt))
                    ehmBotSettings.RollingSaveAmount = rollingSaveAmountInt;
            if (initialSetup) SetDefaultSimLength();
            else StartingMenu();
        }

        private static void SetDefaultSimLength()
        {
            Console.WriteLine("Specify the default amount of days to sim.");
            Console.WriteLine($"Current default sim length: {ehmBotSettings.DefaultSimLength}. Leave blank to leave unchanged.");
            var defaultSimLength = Console.ReadLine()?.Trim();
            if (!(defaultSimLength == null || string.IsNullOrWhiteSpace(defaultSimLength)))
                if (int.TryParse(defaultSimLength, out var defaultSimLengthInt))
                    ehmBotSettings.DefaultSimLength = defaultSimLengthInt;
            if (initialSetup) SetClickPositions();
            else StartingMenu();
        }

        private static void SetClickPositions()
        {
            var clickPositions = ehmBotSettings.ClickPositions ?? new Dictionary<string, Coordinate>();
            var pointsToGoThrough = pointList;
            if (clickPositions.Count != 0)
            {
                Console.WriteLine("Would you like to only set unassigned click positions? (y/n)");
                var yesNo = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(yesNo) && yesNo[0] == 'y') pointsToGoThrough = pointsToGoThrough.Where(x => !clickPositions.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            }
            Console.WriteLine("This will run through each point the bot needs to click the perform its various actions.");
            Console.WriteLine("Fill in each point as two integers separated by a comma. Leave blank to leave unchanged.");
            foreach (var kvp in pointsToGoThrough)
            {
                Console.WriteLine($"{kvp.Key} : {kvp.Value}");
                clickPositions.TryGetValue(kvp.Key, out var currentPoint);
                Console.WriteLine($"Current point: {currentPoint?.X},{currentPoint?.Y}");

                while (!Console.KeyAvailable)
                {
                    var positionString = $"{Cursor.Position.X},{Cursor.Position.Y}";
                    Console.Write($"Current cursor position: {positionString.PadLeft(9)}\r");
                    Thread.Sleep(50);
                }
                Console.WriteLine("");
                var pointString = Console.ReadLine()?.Trim();
                if (pointString == null || string.IsNullOrWhiteSpace(pointString)) continue;
                var pointCoordinates = pointString.Split(',').ToList();
                if (pointCoordinates.Count != 2) continue;
                if (int.TryParse(pointCoordinates[0], out var x) && int.TryParse(pointCoordinates[1], out var y))
                    clickPositions[kvp.Key] = new Coordinate(x, y);
                else Console.WriteLine($"Error parsing. Please come back and redo this point later: {kvp.Key}");
            }


            ehmBotSettings.ClickPositions = clickPositions;
            if (initialSetup) Save();
            else StartingMenu();
        }


        private static void View()
        {
            Console.WriteLine("Current settings:");
            Console.WriteLine(ehmBotSettings.ToString());
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            Console.WriteLine(string.Empty);
            StartingMenu();
        }

        private static void Save()
        {
            if (!initialSetup)
            {
                Console.WriteLine("Changes:");
                Console.WriteLine(ehmBotSettings.GetDifferences(oldSettings));
            }
            else
            {
                Console.WriteLine("Setup done. You can always reopen this program to edit any individual settings");
            }

            Console.WriteLine("Are you sure you want to save? (y/n)");
            var yesNo = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(yesNo) && yesNo[0] == 'y') DefinitelySave();
            else StartingMenu();
        }

        private static void DefinitelySave()
        {
            ehmBotSettings.Save(Path.Combine(Path.Combine(System.AppContext.BaseDirectory, EhmBotSettings.SETTINGS_FILE_NAME)));
            StartingMenu();
        }
    }
}
