using DiscordRPC;
using DiscordRPC.Logging;

namespace Re_Hippocamp.Misc
{
    public static class Discord
    {
		public static DiscordRpcClient client = null;
		public static bool isEnabled = false;

		public static void Ini()
        {
			client = new DiscordRpcClient("913139809868976148");

			//Set the logger
			client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
			//Subscribe to events

			//Connect to the RPC
			client.Initialize();
			//Set the rich presence
			//Call this as many times as you want and anywhere in your code.
		}

        public static void updatePresence(HSong hSong, string now, string duration)
        {
			if (!isEnabled) return;
			if (client == null) Ini();

			client.SetPresence(new RichPresence()
			{
				Details = "Listening to " + hSong.Name + " by " + hSong.Artist,
				State = now + "/" + duration,
				Assets = new Assets()
				{
					LargeImageKey = "hippocamplogo",
					LargeImageText = "Hippocamp available on Steam !"
				}
			});
		}

		public static void Stop()
        {
			//if (!isEnabled) return;
			if (client != null)
			{
				client.ClearPresence();
				client.Deinitialize();
				client.Dispose();
				client = null;
			}
		}

    }
}
