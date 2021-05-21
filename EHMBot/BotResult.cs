using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;

namespace EHMBot
{
    public class BotResult : RuntimeResult, IResult
    {
        public DateTime ResultTime { get; set; }
        public EAction Action { get; set; }

        public BotResult(CommandError? error, string reason) : base(error, reason)
        {
        }
    }

    public enum EAction
    {
        Save = 0,
        Sim = 1,
        Register = 2,
        Deregister = 3,
        ViewTeams = 4,
        Restore = 5,
        Help = 6,
        ReloadSettings = 7
    }
}
