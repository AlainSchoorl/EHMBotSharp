using EHMBot.Core;
using EHMBot.Utilities;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace Restart
{
    internal class Program
    {
        private static EhmBotSettings ehmBotSettings;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        private static async Task Main(string[] args)
        {
            try
            {
                using (var file = new StreamReader(Path.Combine(System.AppContext.BaseDirectory, EhmBotSettings.SETTINGS_FILE_NAME)))
                {
                    var json = file.ReadToEnd();
                    ehmBotSettings = JsonConvert.DeserializeObject<EhmBotSettings>(json);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error loading settings file");
                throw;
            }

            try
            {
                if (!await Utilities.KillAppIfRunning())
                {
                    if (!await Utilities.KillAppIfRunning())
                    {
                        logger.Warn("Error killing app");
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error killing app");
                throw;
            }

            logger.Debug("Starting game");
            Process.Start(Path.Combine(ehmBotSettings.GameLocation, "ehm.exe"));
            await Task.Delay(8500);
            await Utilities.ActivateApp();
            await Utilities.Click(ehmBotSettings.ClickPositions["Load network game"]);
            await Task.Delay(500);
            logger.Debug("Getting last saved game");
            var savePath = new DirectoryInfo(ehmBotSettings.SaveLocation);
            try
            {
                var mostRecentSaveFile = savePath.GetFiles("*.sav").Where(x => x.Name.Contains(ehmBotSettings.SavePrefix) && int.TryParse(x.Name.Split(' ')[1].Substring(0, x.Name.Split(' ')[1].Length - 4), out var saveNumber)).OrderByDescending(x => x.LastWriteTime).First();
                foreach (var character in mostRecentSaveFile.Name.Substring(0, mostRecentSaveFile.Name.Length - 4))
                {
                    SendKeys.SendWait(character.ToString());
                    await Task.Delay(50);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error finding most recent save");
                throw;
            }

            await Utilities.Click(ehmBotSettings.ClickPositions["Load game OK"]);
            await Task.Delay(8500);
            if (ehmBotSettings.ClickPositions.TryGetValue("Restore scroll", out var restoreScroll) && restoreScroll.X > 0)
            {
                await Utilities.Click(restoreScroll);
                await Task.Delay(950);
            }
            await Utilities.Click(ehmBotSettings.ClickPositions["Host"]);
            await Task.Delay(950);
            await Utilities.Click(ehmBotSettings.ClickPositions["Host log in yes"]);

            Console.WriteLine("Done");
        }
    }
}
