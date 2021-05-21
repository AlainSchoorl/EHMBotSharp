using Discord.Commands;
using EHMBot.Core;
using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EHMBot.Modules
{
    public class Restore : ModuleBase<SocketCommandContext>
    {
        private EhmBotSettings ehmBotSettings;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Command("restore", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> RestoreAsync([Remainder]string freeText = "")
        {
            logger.Debug("Starting restore");
            ehmBotSettings = Program.EhmBotSettings;
            if (Context.Guild.GetUser(Context.User.Id).Roles.FirstOrDefault(x => string.Equals(x.Name, ehmBotSettings.VncRoleName, StringComparison.OrdinalIgnoreCase)) == null)
                return new BotResult(CommandError.UnmetPrecondition, "Insufficient permissions") { Action = EAction.Restore, ResultTime = DateTime.Now };
            var process = Process.Start("Utilities//Restart.exe");
            process?.WaitForExit();

            await Context.Channel.SendMessageAsync("Ready for login as all");
            return new BotResult(null, "Ready for login as all") { Action = EAction.Restore, ResultTime = DateTime.Now };
        }

        [Command("ready", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> FinishRestoreAsync()
        {
            logger.Debug("Finishing restore");
            ehmBotSettings = Program.EhmBotSettings;
            var process = Process.Start("Utilities//Restore.exe");
            process?.WaitForExit();

            await Context.Channel.SendMessageAsync("");
            return new BotResult(null, "Restored") { Action = EAction.Restore, ResultTime = DateTime.Now };
        }
    }
}
