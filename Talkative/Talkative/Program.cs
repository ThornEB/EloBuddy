using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Utils;

namespace Talkative
{
    class Program
    {
        public static Menu RootMenu;
        public static Menu WalkerMenu;
        public static Menu CensorMenu;
        public static AIHeroClient Player = ObjectManager.Player;
        public static float lastsent = 0;
        public static float lastsent2 = 0;
        public static int Strikes = 0;
        public static string[] CensoredWords = { "fucktard", "dick", "pussy", "vagina", "penis", "cunt", "fuck", "shit", "nigger", "nigga", "ez", "motherfucker", "mfer", "stfu", "noob", "nob", "retard", "retarded", "asshole", "bitch", "autist", "cancer", "tumor" };

        static void Main(string[] args)
        {
            Bootstrap.Init(null);
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            RootMenu = MainMenu.AddMenu("Talkative", "rootmenu");
            RootMenu.AddLabel("Made by Thorn @ EloBuddy");
            WalkerMenu = RootMenu.AddSubMenu("Walker", "walker");
            WalkerMenu.Add("won", new CheckBox("Follow mouse while chatting", false));
            /*CensorMenu = RootMenu.AddSubMenu("Ban Avoider", "banavoider");
            CensorMenu.Add("con", new CheckBox("Censor bad words", false));
            CensorMenu.Add("strikes", new CheckBox("Use strikes system", false));*/


            Game.OnUpdate += Game_OnUpdate;
            //Chat.OnMessage += Chat_OnInput;
            
        }

        private static void Chat_OnInput(AIHeroClient sender, ChatMessageEventArgs args)
        {
            bool alreadywarned = false;
            if (CensorMenu["con"].Cast<CheckBox>().CurrentValue && sender.IsMe)
            {
                if (Strikes > 3 && CensorMenu["strikes"].Cast<CheckBox>().CurrentValue)
                { args.Process = false; ChatWarning(); }
                else
                {
                    foreach (string word in CensoredWords)
                    {
                        if (args.Message.ToLower().Contains(word) && !alreadywarned && CensorMenu["strikes"].Cast<CheckBox>().CurrentValue)
                            args.Process = false; AddStrike(); ChatWarning(); alreadywarned = true;
                    }
                }
            }
        }

        public static void AddStrike()
        {
            if (Environment.TickCount - lastsent > 250)
            {
                lastsent = Environment.TickCount;
                if (CensorMenu["strikes"].Cast<CheckBox>().CurrentValue)
                    Strikes++;
            }
        }

        public static void ChatWarning()
        {
            if (Environment.TickCount - lastsent2 > 250)
            {
                lastsent2 = Environment.TickCount;
                if (CensorMenu["strikes"].Cast<CheckBox>().CurrentValue)
                {
                    if (Strikes <= 3)
                        Chat.Print(Strikes + "/3 strikes. Stop cursing!");
                    if (Strikes > 3)
                        Chat.Print("Your chat has been disabled due to an excessive amount of offenses.");
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (WalkerMenu["won"].Cast<CheckBox>().CurrentValue && Chat.IsOpen)
                FollowMouse();
        }

        public static void FollowMouse()
        {
            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
        }
    }
}
