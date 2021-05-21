using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EHMBot;
using EHMBot.Core;
using EHMBot.Utilities;
using Newtonsoft.Json;
using NLog;

namespace Restore
{
    class Program
    {
        private static EhmBotSettings ehmBotSettings;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        private static async Task Main(string[] args)
        {
            try
            {
                using (var file = new StreamReader(Path.Combine(System.AppContext.BaseDirectory, EhmBotSettings.SETTINGS_FILE_NAME)))
                {
                    var json = file.ReadToEnd();
                    ehmBotSettings = JsonConvert.DeserializeObject<EhmBotSettings>(json);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error loading settings");
                throw;
            }

            await Utilities.ActivateApp();
            await Utilities.Click(ehmBotSettings.ClickPositions["Finish"]);
            await Task.Delay(300);
            await Utilities.Click(ehmBotSettings.ClickPositions["Anywhere"]);
            await Task.Delay(300);
            await Utilities.Click(ehmBotSettings.ClickPositions["Anywhere"]);
            await Task.Delay(300);
            await Utilities.Click(ehmBotSettings.ClickPositions["Anywhere"]);
            await Task.Delay(300);
            await Utilities.Click(ehmBotSettings.ClickPositions["Anywhere"]);
            await Task.Delay(300);
            await Utilities.Click(ehmBotSettings.ClickPositions["Anywhere"]);

            Console.WriteLine("Done");
        }
    }
}
