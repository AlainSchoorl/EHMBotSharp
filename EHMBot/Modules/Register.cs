using Discord.Commands;
using EHMBot.Core;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EHMBot.Modules
{
    public class Register : ModuleBase<SocketCommandContext>
    {
        private EhmBotSettings ehmBotSettings;
        private readonly string settingsPath = Path.Combine(System.AppContext.BaseDirectory, "Utilities", EhmBotSettings.SETTINGS_FILE_NAME);
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        public Register()
        {
            using (var file = new StreamReader(settingsPath))
            {
                var json = file.ReadToEnd();
                ehmBotSettings = JsonConvert.DeserializeObject<EhmBotSettings>(json);
            }
        }

        [Command("register", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> RegisterGeneralManager([Remainder]string teamName)
        {
            logger.Debug($"Attemping to register GM to {teamName}");
            ehmBotSettings = Program.EhmBotSettings;
            await Task.Delay(0);
            var team = TeamList.GetTeam(teamName);
            if (team == null) return new BotResult(CommandError.Unsuccessful, "Unrecognized team") { Action = EAction.Register, ResultTime = DateTime.Now };
            var generalManager = ehmBotSettings.GetGmByTeam(team);
            var currentGeneralManager = ehmBotSettings.GetGmById(Context.User.Id.ToString());
            var nickName = Context.Guild.GetUser(Context.User.Id).Nickname;
            if (string.IsNullOrWhiteSpace(generalManager.DiscordId) || generalManager.DiscordId == Context.User.Id.ToString())
            {
                generalManager.DiscordId = Context.User.Id.ToString();
                if (currentGeneralManager != null) DeregisterGeneralManager(currentGeneralManager);
                ehmBotSettings.Save(settingsPath);
                logger.Debug($"Registered GM {nickName} to {teamName}");
                return new BotResult(null, $"Registered {nickName} as {team.FullName}") { Action = EAction.Register, ResultTime = DateTime.Now };
            }
            var currentUserOnTeam = Context.Guild.GetUser(Convert.ToUInt64(generalManager.DiscordId));
            if (currentUserOnTeam == null)
            {
                generalManager.DiscordId = Context.User.Id.ToString();
                if (currentGeneralManager != null) DeregisterGeneralManager(currentGeneralManager);
                ehmBotSettings.Save(settingsPath);
                logger.Debug($"Registered GM {nickName} to {teamName}");
                return new BotResult(null, $"Registered {nickName} as {team.FullName}") { Action = EAction.Register, ResultTime = DateTime.Now };
            }
            generalManager.WaitingDiscordId = Context.User.Id.ToString();
            ehmBotSettings.Save(settingsPath);
            logger.Debug($"Set GM {nickName} in waiting list for {teamName}");
            return new BotResult(null, $"{nickName} will be registered as {team.FullName} when {currentUserOnTeam.Nickname ?? currentUserOnTeam.Nickname} registers as another team or deregisters.")
            {
                Action = EAction.Register,
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
