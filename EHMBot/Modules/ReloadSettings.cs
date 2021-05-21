using Discord.Commands;
using NLog;
using System;
using System.Threading.Tasks;

namespace EHMBot.Modules
{
    public class ReloadSettings : ModuleBase<SocketCommandContext>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        [Command("reloadsettings", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> ReloadSettingsAsync([Remainder]string freeText = "")
        {
            await Task.Delay(0);
            logger.Debug("Reloading settings");
            return new BotResult(null, "Reloading settings") { Action = EAction.ReloadSettings, ResultTime = DateTime.Now };
        }
    }
}
