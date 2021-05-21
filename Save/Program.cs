using System;
using System.Threading.Tasks;
using EHMBot.Utilities;
using System.Windows.Forms;
using NLog;

namespace Save
{
    internal class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static async Task Main(string[] args)
        {
            try
            {
                await Utilities.ActivateApp();
                SendKeys.SendWait("^(s)");
            }
            catch (Exception e)
            {
                logger.Error(e, "Error running save.exe");
                throw;
            }
        }
    }
}
