using Discord;

namespace Vigilant.Extensions {
    static class DiscordExtensions {

        public static string GetAnyName(this User user) => 
            user == null 
            ? "N/A" 
            : user.Nickname ?? user.Name;

        public static User ToUser(this string userId, Server s)
            => userId == "-1"
            ? null
            : s.GetUser(userId.ToUlong());
    }
}
