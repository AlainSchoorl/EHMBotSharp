using EHMBot.Core;
using EHMBot.Utilities;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using Tesseract;

namespace CheckOnline
{
    internal class Program
    {
        private static EhmBotSettings ehmBotSettings;
        private static List<int> onlineList = new List<int>();
        private static List<int> responseList = new List<int>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static async Task Main(string[] args)
        {
            logger.Debug("Checking online players");
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
            SendKeys.SendWait("{F11}");
            await Task.Delay(1200);
            ReadOnline(1, 0);
            await Task.Delay(1800);
            await Utilities.Click(ehmBotSettings.ClickPositions["Scroll to middle"]);
            await Task.Delay(1800);
            ReadOnline(1, 11);
            await Task.Delay(1800);
            await Utilities.Click(ehmBotSettings.ClickPositions["Scroll to end"]);
            await Task.Delay(1800);
            ReadOnline(4, 19);
            await Task.Delay(1800);
            await CheckResponse();
            responseList.Remove(ehmBotSettings.HostPosition);
            Console.WriteLine(string.Join(",", responseList));
        }

        private static void ReadOnline(int startPosition, int playerAdd)
        {
            logger.Debug($"Reading online starting from player {playerAdd + 1}");
            var dimensionX = ehmBotSettings.ClickPositions["Status bottom right"].X - ehmBotSettings.ClickPositions["Status top left"].X;
            var dimensionY = ehmBotSettings.ClickPositions["Status bottom right"].Y - ehmBotSettings.ClickPositions["Status top left"].Y;
            var distance = (ehmBotSettings.ClickPositions["Last status"].Y - ehmBotSettings.ClickPositions["Status top left"].Y) / 10;
            var location = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            using (var engine = new TesseractEngine(location + "\\TESSDATA\\", "eng"))
            {
                engine.SetVariable("tessedit_char_whitelist", "Playing");
                engine.SetVariable("tessedit_pageseg_mode", "7");
                for (var counter = startPosition; counter <= 11; counter++)
                {
                    var bitmap = new Bitmap(dimensionX, dimensionY);
                    var g = Graphics.FromImage(bitmap);
                    g.CopyFromScreen(ehmBotSettings.ClickPositions["Status top left"].X, ehmBotSettings.ClickPositions["Status top left"].Y + ((counter - 1) * distance), 0, 0, new System.Drawing.Size(dimensionX, dimensionY));
                    bitmap = Utilities.ResizeImage(bitmap, dimensionX * 3, dimensionY * 3);
                    bitmap.Save($"{location}\\CheckOnline\\player{counter + playerAdd}.png");
                    var src = new Mat($"{location}\\CheckOnline\\player{counter + playerAdd}.png", ImreadModes.AnyColor);
                    var dst = new Mat();
                    Cv2.CvtColor(src, dst, ColorConversionCodes.BGR2GRAY);
                    Cv2.Erode(dst, dst, new Mat());
                    Cv2.Dilate(dst, dst, new Mat());
                    bitmap = Utilities.MatToBitmap(dst);
                    bitmap.Save($"{location}\\CheckOnline\\player{counter + playerAdd}.png");
                    using (bitmap)
                    using (var page = engine.Process(bitmap))
                    using (var iterator = page.GetIterator())
                    {
                        iterator.Begin();
                        do
                        {
                            var text = iterator.GetText(PageIteratorLevel.Word);
                            if (text == "Playing")
                            {
                                onlineList.Add(counter + playerAdd);
                            }
                        }
                        while (iterator.Next(PageIteratorLevel.Word));
                    }
                }
            }
        }

        private static async Task CheckResponse()
        {
            logger.Debug("Checking response screen GMs");

            var section1 = onlineList.Where(x => x <= 11);
            var section2 = onlineList.Where(x => x > 11 && x <= 22);
            var section3 = onlineList.Where(x => x > 22 && x <= 30);

            var xPos = ehmBotSettings.ClickPositions["Status name"].X;
            var yPosBase = (ehmBotSettings.ClickPositions["Status bottom right"].Y + ehmBotSettings.ClickPositions["Status top left"].Y) / 2;
            var distance = (ehmBotSettings.ClickPositions["Last status"].Y - ehmBotSettings.ClickPositions["Status top left"].Y) / 10;

            await Utilities.Click(ehmBotSettings.ClickPositions["Scroll to top"]);
            await Task.Delay(950);
            foreach (var player in section1)
            {
                await Utilities.RightClick(xPos, yPosBase + distance * (player - 1));
                await Task.Delay(950);
                var color = Utilities.GetColorAt(xPos + 5, yPosBase + distance * (player - 1) + 2);
                if (!(color.R == 10 && color.G == 41 && color.B == 92)) responseList.Add(player);
                await Utilities.Click(ehmBotSettings.ClickPositions["Anywhere"]);
                await Task.Delay(950);
            }

            await Utilities.Click(ehmBotSettings.ClickPositions["Scroll to middle"]);
            await Task.Delay(950);
            foreach (var player in section2)
            {
                await Utilities.RightClick(xPos, yPosBase + distance * (player - 12));
                await Task.Delay(950);
                var color = Utilities.GetColorAt(xPos + 5, yPosBase + distance * (player - 12) + 2);
                if (!(color.R == 10 && color.G == 41 && color.B == 92)) responseList.Add(player);
                await Utilities.Click(ehmBotSettings.ClickPositions["Anywhere"]);
                await Task.Delay(950);
            }

            await Utilities.Click(ehmBotSettings.ClickPositions["Scroll to end"]);
            await Task.Delay(950);
            foreach (var player in section3)
            {
                await Utilities.RightClick(xPos, yPosBase + distance * (player - 20));
                await Task.Delay(950);
                var color = Utilities.GetColorAt(xPos + 5, yPosBase + distance * (player - 20) + 2);
                if (!(color.R == 10 && color.G == 41 && color.B == 92)) responseList.Add(player);
                await Utilities.Click(ehmBotSettings.ClickPositions["Anywhere"]);
                await Task.Delay(950);
            }
        }
    }
}
