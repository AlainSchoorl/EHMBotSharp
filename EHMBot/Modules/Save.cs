using Discord.Commands;
using EHMBot.Core;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EHMBot.Modules
{
    public class Save : ModuleBase<SocketCommandContext>
    {
        private EhmBotSettings ehmBotSettings;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Command("save", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> SaveAsync([Remainder]string freeText = "")
        {
            logger.Debug("Starting save task");
            ehmBotSettings = Program.EhmBotSettings;
            Process.Start("Utilities//Save.exe");
            await Task.Delay(ehmBotSettings.MillisecondsToSave);

            var crashed = false;
            var processes = Process.GetProcessesByName("ehm");
            if (!processes.Any())
            {
                logger.Debug("Process crashed");
                crashed = true;
            }

            var success = false;
            BotResult fRes;
            try
            {
                logger.Debug("Getting last saved file");
                var savePath = new DirectoryInfo(ehmBotSettings.SaveLocation);
                var lastSavedFile = savePath.GetFiles("*.sav")
                    .Where(x => x.Name.StartsWith(ehmBotSettings.SavePrefix, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.LastWriteTime).FirstOrDefault();

                if (lastSavedFile == null) throw new ArgumentNullException($"No save files with prefix {ehmBotSettings.SavePrefix} were found");
                if (lastSavedFile.LastWriteTime > DateTime.Now.AddMilliseconds(ehmBotSettings.MillisecondsToSave).AddSeconds(-5))
                {
                    logger.Debug("File was saved in the last 20 seconds");
                    success = true;
                }

                if (!crashed)
                    fRes = !success ? new BotResult(CommandError.Unsuccessful, $"Save Failed! {Context.User.Mention}") : new BotResult(null, $"Saved {Context.User.Mention}");
                else
                    fRes = !success ? new BotResult(CommandError.Unsuccessful, $"Game crashed, save didn't stick. {Context.User.Mention}.") : new BotResult(null, $"Saved {Context.User.Mention}, but game crashed.");
                fRes.Action = EAction.Save;
                fRes.ResultTime = DateTime.Now;

            }
            catch (Exception e)
            {
                logger.Error(e, "Error saving game");
                throw;
            }
            return fRes;
        }

        [Command("saved", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> SaveAnnounce()
        {
            logger.Debug("Announcing save");
            ehmBotSettings = Program.EhmBotSettings;
            await Task.Delay(0);
            var savePath = new DirectoryInfo(ehmBotSettings.SaveLocation);
            var lastSavedFile = savePath.GetFiles("*.sav")
                .Where(x => x.Name.Contains($"{ehmBotSettings.SavePrefix} ")
                            && int.TryParse(x.Name.Split(' ')[1].Substring(0, x.Name.Split(' ')[1].Length - 4), out var saveNumber))
                .OrderByDescending(x => x.LastWriteTime).First();
            return new BotResult(null, $"Last save at {lastSavedFile.LastWriteTime}") { Action = EAction.Save, ResultTime = DateTime.Now };
        }
    }
}
