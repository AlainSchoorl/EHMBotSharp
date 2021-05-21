using Discord.Commands;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EHMBot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {

        private static readonly Dictionary<string, string> commandList = new Dictionary<string, string>
        {
            { "Help", "Displays a list of commands." },
            { "Save", "Saves the game." },
            { "Saved", "Announces the last known save time (in case a GM blocked the initial save)." },
            { "Register", "Registers a GM to a team (for purposes of notifying you to roster for a save)." },
            { "Deregister", "Removes a GM from a team." },
            { "GMList", "Displays which GM is registered to each team." },
            { "Restore", "Force quits and restarts the game (VNC only)." },
            { "Ready", "Notifies the bot that it can finish restoring the game." },
            { "Sim x", "Sims the game x days (WIP)" },
            { "ReloadSettings", "Reloads changes in the bot settings without having to restart the bot." }
        };
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        [Command("help", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> HelpAsync([Remainder]string freeText = "")
        {
            logger.Debug("Posting command list");
            var fRes = new StringBuilder();
            foreach (var kvp in commandList)
            {
                fRes.AppendLine($"{kvp.Key} : {kvp.Value}");
            }
            await Context.Channel.SendMessageAsync(fRes.ToString());
            return new BotResult(null, "Printed Help") { Action = EAction.Help, ResultTime = DateTime.Now };
        }
    }
}
