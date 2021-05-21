using Discord.Commands;
using EHMBot.Core;
using NLog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EHMBot.Modules
{
    public class GmList : ModuleBase<SocketCommandContext>
    {
        private EhmBotSettings ehmBotSettings;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Command("gmlist", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> ViewTeamsAsync([Remainder]string freeText = "")
        {
            logger.Debug("Posting GM list");
            ehmBotSettings = Program.EhmBotSettings;
            var fRes = new StringBuilder();
            foreach (var kvp in ehmBotSettings.GeneralManagers)
            {
                fRes.AppendLine(
                    $"{kvp.Key} : {kvp.Value.Team.FullName} - " +
                    $"{(string.IsNullOrWhiteSpace(kvp.Value.DiscordId) ? "No GM" : (Context.Guild.GetUser(Convert.ToUInt64(kvp.Value.DiscordId)).Nickname ?? Context.Guild.GetUser(Convert.ToUInt64(kvp.Value.DiscordId)).Username))}");
            }

            await Context.Channel.SendMessageAsync(fRes.ToString());
            return new BotResult(null, "Printed GM List") { Action = EAction.ViewTeams, ResultTime = DateTime.Now };
        }
    }
}
