using Discord.Commands;
using EHMBot.Core;
using NLog;
using System;
using System.Threading.Tasks;

namespace EHMBot.Modules
{
    public class Sim : ModuleBase<SocketCommandContext>
    {
        private EhmBotSettings ehmBotSettings;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        [Command("sim", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> SimGames([Remainder]string length)
        {
            ehmBotSettings = Program.EhmBotSettings;
            await Task.Delay(0);
            /*var process = Process.Start("Utilities//Sim.exe");
            process?.WaitForExit();*/
            var fRes = true ? new BotResult(CommandError.Unsuccessful, "Can't sim yet") : new BotResult(null, "Save made");
            fRes.Action = EAction.Sim;
            fRes.ResultTime = DateTime.Now;
            return fRes;
        }
    }
}
