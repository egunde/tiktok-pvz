using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using TikTokLiveSharp.Client;
using TikTokLiveSharp.Events;

namespace TikTokLiveSharpTestApplication
{
    internal class Program
    {
        static Dictionary<string, int> yoCount = new Dictionary<string, int>();
        static void Main(string[] args)
        {
            Console.WriteLine("Enter a username:");            
            var client = new TikTokLiveClient(Console.ReadLine(), "");
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;
            client.OnRoomUpdate += Client_OnViewerData;
            client.OnLiveEnded += Client_OnLiveEnded;
            client.OnJoin += Client_OnJoin;
            client.OnChatMessage += Client_OnComment;
            client.OnFollow += Client_OnFollow;
            client.OnShare += Client_OnShare;
            client.OnSubscribe += Client_OnSubscribe;
            client.OnLike += Client_OnLike;
            client.OnGiftMessage += Client_OnGiftMessage;
            client.OnEmoteChat += Client_OnEmote;
            //var prog = new Program();
            //prog.PrintGiftData(client);
            client.Run(new System.Threading.CancellationToken());
        }
        /* Used to retrieve gift data. Prints the cost and url
        private async void PrintGiftData(TikTokLiveClient client)
        {
            var gifts = await client.FetchAvailableGifts();
            foreach (KeyValuePair<int, TikTokLiveSharp.Models.HTTP.TikTokGift> kvp in gifts)
            {
                Console.WriteLine("Cost = {0}, Url = {1}", kvp.Value.diamond_count, kvp.Value.image.url_list.ElementAt(1));
            }
        }
        */

        // Sends data to the PVZ tools app
        private static void SendGiftData(string data)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "PIPE", PipeDirection.Out))
            {
                pipeClient.Connect();
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                pipeClient.Write(dataBytes, 0, dataBytes.Length);
            }
        }

        //Fired on gift. Sends data in the form: type cost uid name
        private static void Client_OnGiftMessage(TikTokLiveClient sender, GiftMessage e)
        {
            SetConsoleColor(ConsoleColor.Magenta);
            //Console.WriteLine($"{e.Sender.UniqueId} sent {e.Amount}x {e.Gift.Name}!");
            SendGiftData($"{e.Gift.Type} {e.Gift.DiamondCost} {e.User.UniqueId} {e.Gift.Name}");
            SetConsoleColor(ConsoleColor.White);
        }

        //Fired on like. Sends data in the form: type(always 0) count
        private static void Client_OnLike(TikTokLiveClient sender, Like e)
        {
            SetConsoleColor(ConsoleColor.Red);
            //Console.WriteLine($"{e.Sender.UniqueId} liked!");
            //Sending a 0 here to trigger like condition on switch statement in MainForm.cs
            SendGiftData($"0 {e.Count}");
            SetConsoleColor(ConsoleColor.White);
        }

        //Fired on follow. Sends data in the form: type(always -1) uid
        private static void Client_OnFollow(TikTokLiveClient sender, Follow e)
        {
            SetConsoleColor(ConsoleColor.DarkRed);
            //Console.WriteLine($"{e.NewFollower?.UniqueId} followed!");
            //Sending a -1 here to trigger follower condition on switch statement in MainForm.cs
            SendGiftData($"-1 {e.User?.UniqueId}");
            SetConsoleColor(ConsoleColor.White);
        }

        //Fired on share. Sends data in the form: type(always -2) uid
        private static void Client_OnShare(TikTokLiveClient sender, Share e)
        {
            SetConsoleColor(ConsoleColor.Blue);
            //Console.WriteLine($"{e.User?.UniqueId} shared!");
            //Sending a -2 here to trigger share condition on switch statement in MainForm.cs
            SendGiftData($"-2 {e.User?.UniqueId}");
            SetConsoleColor(ConsoleColor.White);
        }

        //When the users comment "!yo", this function prints their total count for the session
        private static void Client_OnComment(TikTokLiveClient sender, Chat e)
        {
            SetConsoleColor(ConsoleColor.Blue);
            //Console.WriteLine(e.Message);
            if (e.Message.Equals("/yo"))
            {
                int senderCount = 1;
                if (yoCount.ContainsKey(e.Sender.UniqueId))
                {
                    senderCount = yoCount[e.Sender.UniqueId] + 1;
                    yoCount[e.Sender.UniqueId] = senderCount;
                    Console.WriteLine($"YO! {e.Sender.UniqueId} yoCount:{senderCount}");
                }
                else
                {
                    yoCount.Add(e.Sender.UniqueId, senderCount);
                    Console.WriteLine($"YO! {e.Sender.UniqueId} yoCount:{senderCount}");
                }
            }
            SetConsoleColor(ConsoleColor.White);
        }
        private static void Client_OnJoin(TikTokLiveClient sender, Join e)
        {
            SetConsoleColor(ConsoleColor.Green);
            //Console.WriteLine($"{e.User.UniqueId} joined!");
            SendGiftData($"-3 {e.User.UniqueId}");
            SetConsoleColor(ConsoleColor.White);
        }

        private static void Client_OnConnected(TikTokLiveClient sender, bool e)
        {
            SetConsoleColor(ConsoleColor.White);
            //Console.WriteLine($"Connected to Room! [Connected:{e}]");
        }

        private static void Client_OnDisconnected(TikTokLiveClient sender, bool e)
        {
            SetConsoleColor(ConsoleColor.White);
            //Console.WriteLine($"Disconnected from Room! [Connected:{e}]");
        }

        private static void Client_OnViewerData(TikTokLiveClient sender, RoomUpdate e)
        {
            SetConsoleColor(ConsoleColor.Cyan);
            //Console.WriteLine($"Viewer count is: {e.NumberOfViewers}");
            SetConsoleColor(ConsoleColor.White);
        }

        private static void Client_OnLiveEnded(TikTokLiveClient sender, ControlMessage e)
        {
            SetConsoleColor(ConsoleColor.White);
            Console.WriteLine("Host ended Stream!");
        }

        private static void Client_OnSubscribe(TikTokLiveClient sender, Subscribe e)
        {
            SetConsoleColor(ConsoleColor.DarkCyan);
            //Console.WriteLine($"{e.User.UniqueId} subscribed!");
            SetConsoleColor(ConsoleColor.White);
        }

        private static void Client_OnEmote(TikTokLiveClient sender, EmoteChat e)
        {
            SetConsoleColor(ConsoleColor.DarkGreen);
            //Console.WriteLine($"{e.User.UniqueId} sent {e.Emotes?.First()?.Id}!");
            SetConsoleColor(ConsoleColor.White);
        }

        private static void SetConsoleColor(ConsoleColor color)
        {
            if (Console.ForegroundColor != color)
                Console.ForegroundColor = color;
        }
    }
}
