using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EHMBot.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EHMBot
{
    internal class Program
    {
        public static EhmBotSettings EhmBotSettings { get; set; }
        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        private readonly string settingsPath = Path.Combine(System.AppContext.BaseDirectory, "Utilities", EhmBotSettings.SETTINGS_FILE_NAME);
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        private static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            using (var file = new StreamReader(settingsPath))
            {
                var json = file.ReadToEnd();
                EhmBotSettings = JsonConvert.DeserializeObject<EhmBotSettings>(json);
            }

            client = new DiscordSocketClient();
            commands = new CommandService();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .BuildServiceProvider();

            string botToken = EhmBotSettings.BotToken;

            client.Log += LogMessage;

            await RegisterCommandsAsync();

            await client.LoginAsync(TokenType.Bot, botToken);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        private Task LogMessage(LogMessage arg)
        {
            Console.WriteLine(arg);
            logger.Debug(arg.Message);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

            commands.CommandExecuted += HandlePostCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            if (message is null) return;

            if (message.Channel.Id.ToString() != EhmBotSettings.BotRequestChannel &&
                message.Channel.Id.ToString() != EhmBotSettings.SaveAnnounceChannel &&
                !string.IsNullOrWhiteSpace(EhmBotSettings.BotRequestChannel)
                && message.Content.StartsWith("!save")) return;

            var argumentPosition = 0;

            if (message.HasCharPrefix('!', ref argumentPosition))
            {
                logger.Debug($"Handling command: {message.Content}");
                var context = new SocketCommandContext(client, message);
                await commands.ExecuteAsync(context, argumentPosition, services);
            }
        }

        private async Task HandlePostCommandAsync(Optional<CommandInfo> command, ICommandContext context, IResult originalResult)
        {
            logger.Debug($"Handling post command. Error: {originalResult.Error} - {originalResult.ErrorReason}");
            var result = originalResult as BotResult;
            if (result?.Action == EAction.Save && EhmBotSettings.DeleteSaveRequest) await context.Message.DeleteAsync();


            if (result != null && !result.IsSuccess)
            {
                if (new List<EAction> { EAction.Register, EAction.Deregister, EAction.ReloadSettings }.Contains(result.Action)) ReloadSettings();
                switch (result.Action)
                {
                    case EAction.Save:
                        //await CheckOnline(context);
                        break;
                    default:
                        logger.Error($"{result.ResultTime} - {result.Action} - {result.Reason}");
                        break;
                }
            }
            if (result != null)
            {
                if (new List<EAction> { EAction.Register, EAction.Deregister, EAction.ReloadSettings }.Contains(result.Action)) ReloadSettings();
                switch (result.Action)
                {
                    case EAction.Save:
                        await Post(result.Reason);
                        break;
                    case EAction.Help:
                        break;
                    default:
                        await Post(result.Reason, client.GetChannel(context.Channel.Id) as SocketTextChannel);
                        break;
                }
            }
            else if (!originalResult.IsSuccess) logger.Error($"{originalResult.Error} - {originalResult.ErrorReason}");
        }

        private void ReloadSettings()
        {
            using (var file = new StreamReader(settingsPath))
            {
                var json = file.ReadToEnd();
                EhmBotSettings = JsonConvert.DeserializeObject<EhmBotSettings>(json);
            }
        }

        private async Task Post(string message, SocketTextChannel channel = null)
        {
            logger.Debug($"Posting '{message}' to '{channel?.Name}'");
            try
            {
                if (channel == null) channel = client.GetChannel(Convert.ToUInt64(EhmBotSettings.SaveAnnounceChannel)) as SocketTextChannel;
                if (channel != null) await channel.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Error posting following message in {channel?.Name} : {message}");
                throw;
            }
        }

        private async Task CheckOnline(ICommandContext context)
        {
            string output = string.Empty;
            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "Utilities//CheckOnline.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    output = proc.StandardOutput.ReadLine();
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error getting output from CheckOnline task");
                throw;
            }

            logger.Debug($"Getting online GMs from CheckOnline output: {output}");

            try
            {

                var onlineGmIntegers = output?.Split(',');
                var onlineGms = new List<GeneralManager>();
                if (onlineGmIntegers != null && onlineGmIntegers.Length > 0 && !string.IsNullOrWhiteSpace(onlineGmIntegers[0]))
                {
                    try
                    {
                        onlineGms.AddRange(onlineGmIntegers.Select(gm => EhmBotSettings.GetGmByPosition(int.Parse(gm))));
                    }
                    catch
                    {
                        await Post("Failed to save, no online GMs found. Host is possibly blocking save?", context.Channel as SocketTextChannel);
                        throw;
                    }

                    logger.Debug($"Notifying {onlineGms.Count} online GMs");

                    var notifyMessage = new StringBuilder();
                    foreach (var gm in onlineGms)
                    {
                        notifyMessage.Append(!string.IsNullOrEmpty(gm.DiscordId)
                            ? $"{client.GetUser(Convert.ToUInt64(gm.DiscordId)).Mention} "
                            : $"{gm.Team.TeamName} ");
                    }

                    notifyMessage.Append("Roster for a save please! Use the !saved command to let me know a save has happened so I can post it in announcements.");
                    await Post(notifyMessage.ToString(), context.Channel as SocketTextChannel);
                }
                else await Post("Failed to save, no online GMs found. Host is possibly blocking save?", context.Channel as SocketTextChannel);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error getting online GMs from CheckOnline output");
                throw;
            }
        }
    }
}
