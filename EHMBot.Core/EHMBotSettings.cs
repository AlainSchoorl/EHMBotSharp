using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace EHMBot.Core
{
    public class EhmBotSettings
    {
        public const string SETTINGS_FILE_NAME = "derulo.json";
        public string BotToken { get; set; }
        public Dictionary<int, GeneralManager> GeneralManagers { get; set; }
        public string VncRoleName { get; set; }
        public int HostPosition { get; set; }
        public bool DeleteSaveRequest { get; set; }
        public int MillisecondsToSave { get; set; }
        public string SaveAnnounceChannel { get; set; }
        public string BotRequestChannel { get; set; }
        public string SaveLocation { get; set; }
        public string GameLocation { get; set; }
        public string SavePrefix { get; set; }
        public int RollingSaveAmount { get; set; }
        public int DefaultSimLength { get; set; }
        public Dictionary<string, Coordinate> ClickPositions { get; set; }

        public void Save(string path)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public GeneralManager GetGmById(string discordId)
        {
            foreach (var kvp in GeneralManagers)
            {
                if (kvp.Value.DiscordId == discordId) return kvp.Value;
            }

            return null;
        }

        public GeneralManager GetGmByTeam(Team team)
        {
            foreach (var kvp in GeneralManagers)
            {
                if (kvp.Value.Team.FullName == team.FullName) return kvp.Value;
            }

            return null;
        }

        public GeneralManager GetGmByPosition(int position)
        {
            return GeneralManagers[position];
        }

        public override string ToString()
        {
            var settingsString = new StringBuilder();
            settingsString.AppendLine($"Bot Token: {BotToken}");
            settingsString.AppendLine($"Delete Save Request: {DeleteSaveRequest}");
            settingsString.AppendLine($"Milliseconds to save: {MillisecondsToSave}");
            settingsString.AppendLine($"Save Announce Channel ID: {SaveAnnounceChannel}");
            settingsString.AppendLine($"Bot Request Channel ID: {(string.IsNullOrWhiteSpace(BotRequestChannel) ? "ALL" : BotRequestChannel)}");
            settingsString.AppendLine($"Save game location: {SaveLocation}");
            settingsString.AppendLine($"Game exe location: {GameLocation}");
            settingsString.AppendLine($"Save game prefix: {SavePrefix}");
            settingsString.AppendLine($"Rolling save amount: {RollingSaveAmount}");
            settingsString.AppendLine($"Default sim length: {DefaultSimLength}");
            if (GeneralManagers != null)
            {
                foreach (var kvp in GeneralManagers)
                    settingsString.AppendLine($"{kvp.Key}: {kvp.Value.Team.FullName}");
            }
            if (ClickPositions != null)
            {
                foreach (var kvp in ClickPositions)
                    settingsString.AppendLine($"{kvp.Key}: {kvp.Value.X},{kvp.Value.Y}");
            }
            return settingsString.ToString();
        }

        public string GetDifferences(EhmBotSettings oldSettings)
        {
            var differences = new StringBuilder();
            if (oldSettings.BotToken != BotToken) differences.AppendLine($"Bot Token: {oldSettings.BotToken} => {BotToken}");
            if (oldSettings.DeleteSaveRequest != DeleteSaveRequest) differences.AppendLine($"Delete Save Request: {oldSettings.DeleteSaveRequest} => {DeleteSaveRequest}");
            if (oldSettings.MillisecondsToSave != MillisecondsToSave) differences.AppendLine($"Milliseconds to save: {oldSettings.MillisecondsToSave} => {MillisecondsToSave}");
            if (oldSettings.SaveAnnounceChannel != SaveAnnounceChannel) differences.AppendLine($"Save Announce Channel ID: {oldSettings.SaveAnnounceChannel} => {SaveAnnounceChannel}");
            if (oldSettings.BotRequestChannel != BotRequestChannel)
                differences.AppendLine($"Bot Request Channel ID: " +
                                       $"{(string.IsNullOrWhiteSpace(oldSettings.BotRequestChannel) ? "ALL" : oldSettings.BotRequestChannel)} " +
                                       $"=> {(string.IsNullOrWhiteSpace(BotRequestChannel) ? "ALL" : BotRequestChannel)}");
            if (oldSettings.SaveLocation != SaveLocation) differences.AppendLine($"Save game location: {oldSettings.SaveLocation} => {SaveLocation}");
            if (oldSettings.GameLocation != GameLocation) differences.AppendLine($"Game exe location: {oldSettings.GameLocation} => {GameLocation}");
            if (oldSettings.SavePrefix != SavePrefix) differences.AppendLine($"Save game prefix: {oldSettings.SavePrefix} => {SavePrefix}");
            if (oldSettings.RollingSaveAmount != RollingSaveAmount) differences.AppendLine($"Rolling save amount: {oldSettings.RollingSaveAmount} => {RollingSaveAmount}");
            if (oldSettings.DefaultSimLength != DefaultSimLength) differences.AppendLine($"Default sim length: {oldSettings.DefaultSimLength} => {DefaultSimLength}");
            if (GeneralManagers != null)
            {
                foreach (var kvp in GeneralManagers)
                {
                    var oldValue = oldSettings.GeneralManagers?[kvp.Key];
                    if (oldValue?.Team.FullName != kvp.Value.Team.FullName) differences.AppendLine($"GM {kvp.Key}: {oldValue?.Team.FullName} => {kvp.Value.Team.FullName}");
                }
            }

            if (ClickPositions != null)
            {
                foreach (var kvp in ClickPositions)
                {
                    if (oldSettings.ClickPositions.TryGetValue(kvp.Key, out var oldValue))
                    {
                        if (oldValue != kvp.Value)
                            differences.AppendLine($"{kvp.Key}: {oldValue?.X}{oldValue?.Y} => {kvp.Value.X},{kvp.Value.Y}");
                    }
                    else differences.AppendLine($"{kvp.Key}: X,Y => {kvp.Value.X},{kvp.Value.Y}");
                }
            }

            return differences.ToString();
        }

    }

    public class GeneralManager
    {
        public Team Team { get; set; }
        public string DiscordId { get; set; }
        public string WaitingDiscordId { get; set; }
    }

    public class Team
    {
        public string ShortName { get; }
        public string FullName { get; }
        public string CityName { get; }
        public string TeamName { get; }
        public string AlternativeName { get; }
        public string AlternativeName2 { get; }
        public string AlternativeName3 { get; }

        public Team(string shortName, string fullName, string cityName, string teamName, string alternativeName, string alternativeName2, string alternativeName3)
        {
            ShortName = shortName;
            FullName = fullName;
            CityName = cityName;
            TeamName = teamName;
            AlternativeName = alternativeName;
            AlternativeName2 = alternativeName2;
            AlternativeName3 = alternativeName3;
        }

        public bool Equals(string teamName) => (string.Equals(teamName, ShortName, StringComparison.CurrentCultureIgnoreCase) || 
                                                string.Equals(teamName, FullName, StringComparison.CurrentCultureIgnoreCase) ||
                                                string.Equals(teamName, CityName, StringComparison.CurrentCultureIgnoreCase) ||
                                                string.Equals(teamName, TeamName, StringComparison.CurrentCultureIgnoreCase) ||
                                                string.Equals(teamName, AlternativeName, StringComparison.CurrentCultureIgnoreCase) ||
                                                string.Equals(teamName, AlternativeName2, StringComparison.CurrentCultureIgnoreCase) ||
                                                string.Equals(teamName, AlternativeName3, StringComparison.CurrentCultureIgnoreCase));
    }

    public class Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Coordinate a, Coordinate b) => (a?.X == b?.X && a?.Y == b?.Y);

        public static bool operator !=(Coordinate a, Coordinate b) => !(a?.X == b?.X && a?.Y == b?.Y);

        public override bool Equals(object obj)
        {
            var coordinate = obj as Coordinate;
            return coordinate != null &&
                   X == coordinate.X &&
                   Y == coordinate.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    }

    public static class TeamList
    {
        private static readonly List<Team> teamList = new List<Team>
        {
            new Team("ANA", "Anaheim Ducks", "Anaheim", "Ducks", "Mighty Ducks of Anaheim", "Mighty Ducks", null),
            new Team("ARI", "Arizona Coyotes", "Arizona", "Coyotes", "Phoenix Coyotes", "Yotes", null),
            new Team("BOS", "Boston Bruins", "Boston", "Bruins", "Bears", "Big Bad Bruins", null),
            new Team("BUF", "Buffalo Sabres", "Buffalo", "Sabres", "Blades", null, null),
            new Team("CAR", "Carolina Hurricanes", "Carolina", "Hurricanes", "Canes", "Hartford Whalers", "Whalers"),
            new Team("CGY", "Calgary Flames", "Calgary", "Flames", "Scorch", null, null),
            new Team("CHI", "Chicago Blackhawks", "Chicago", "Blackhawks", "Hawks", null, null),
            new Team("CBJ", "Columbus Blue Jackets", "Columbus", "Blue Jackets", "Jackets", null, null),
            new Team("COL", "Colorado Avalanche", "Colorado", "Avalanche", "Avs", "Nordiques", null),
            new Team("DAL", "Dallas Stars", "Dallas", "Stars", "Minnesota North Stars", "North Stars", null),
            new Team("DET", "Detroit Red Wings", "Detroit", "Red Wings", "Wings", "Cougars", "Falcons"),
            new Team("EDM", "Edmonton Oilers", "Edmonton", "Oilers", "Oil", "Oils", null),
            new Team("FLA", "Florida Panthers", "Florida", "Panthers", "Cats", null, null),
            new Team("LAK", "Los Angeles Kings", "Los Angeles", "Kings", null, null, null),
            new Team("MIN", "Minnesota Wild", "Minnesota", "Wild", "Mild", null, null),
            new Team("MTL", "Montreal Canadiens", "Montreal", "Canadiens", "Habs", "Habitants", "Canadians"),
            new Team("NSH", "Nashville Predators", "Nashville", "Predators", "Preds", "Smashville", null),
            new Team("NJD", "New Jersey Devils", "New Jersey", "Devils", "Jersey", null, null),
            new Team("NYI", "New York Islanders", "NY Islanders", "Islanders", "Isles", null, null),
            new Team("NYR", "New York Rangers", "NY Rangers", "Rangers", "Rags", null, null),
            new Team("OTT", "Ottawa Senators", "Ottawa", "Senators", "Sens", null, null),
            new Team("PHI", "Philadelphia Flyers", "Philadelphia", "Flyers", "Broad Street Bullies", "Bullies", "Gritty"),
            new Team("PIT", "Pittsburgh Penguins", "Pittsburgh", "Penguins", "Pens", null, null),
            new Team("SJS", "San Jose Sharks", "San Jose", "Sharks", null, null, null),
            new Team("STL", "St. Louis Blues", "St. Louis", "Blues", "St Louis Blues", "St Louis", null),
            new Team("TBL", "Tampa Bay Lightning", "Tampa Bay", "Lightning", "Bolts", "Tampa", null),
            new Team("TOR", "Toronto Maple Leafs", "Toronto", "Maple Leafs", "Leafs", null, null),
            new Team("VAN", "Vancouver Canucks", "Vancouver", "Canucks", "Nucks", null, null),
            new Team("WPG", "Winnipeg Jets", "Winnipeg", "Jets", null, null, null),
            new Team("WSH", "Washington Capitals", "Washington", "Capitals", "Caps", null, null)
        };

        public static Team GetTeam(string teamName)
        {
            foreach (var team in teamList)
                if (team.Equals(teamName))
                    return team;
            return null;
        }
    }
}
