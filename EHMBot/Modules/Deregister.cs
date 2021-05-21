using Discord.Commands;
using Discord.WebSocket;
using EHMBot.Core;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EHMBot.Modules
{
    public class Deregister : ModuleBase<SocketCommandContext>
    {
        private EhmBotSettings ehmBotSettings;
        private readonly string settingsPath = Path.Combine(System.AppContext.BaseDirectory, "Utilities", EhmBotSettings.SETTINGS_FILE_NAME);
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Command("deregister", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> DeregisterGeneralManager([Remainder]string teamName)
        {
            logger.Debug($"Attemping to deregister GM from {teamName}");
            ehmBotSettings = Program.EhmBotSettings;
            await Task.Delay(0);
            var team = TeamList.GetTeam(teamName);
            if (team == null) return new BotResult(CommandError.Unsuccessful, "Unrecognized team") { Action = EAction.Deregister, ResultTime = DateTime.Now };
            var generalManager = ehmBotSettings.GetGmByTeam(team);
            SocketGuildUser waitingGeneralManager = null;
            if (!string.IsNullOrWhiteSpace(generalManager.WaitingDiscordId))
                waitingGeneralManager = Context.Guild.GetUser(Convert.ToUInt64(generalManager.WaitingDiscordId));
            DeregisterGeneralManager(generalManager);
            ehmBotSettings.Save(settingsPath);
            var nickName = Context.Guild.GetUser(Context.User.Id).Nickname;
            logger.Debug($"Deregistered GM from {teamName} {(waitingGeneralManager != null ? $" Replaced with {waitingGeneralManager.Nickname ?? waitingGeneralManager.Username}" : string.Empty)}");
            return new BotResult(null, $"Deregistered {nickName} from {team.FullName}" +
                                       $"{(waitingGeneralManager != null ? $" Replaced with {waitingGeneralManager.Nickname ?? waitingGeneralManager.Username}" : string.Empty)}")
            {
                Action = EAction.Deregister,
                ResultTime = DateTime.Now
            };
        }

        private static void DeregisterGeneralManager(GeneralManager generalManager)
        {
            generalManager.DiscordId = string.IsNullOrWhiteSpace(generalManager.WaitingDiscordId)
                ? null
                : generalManager.WaitingDiscordId;
        }
    }
}
