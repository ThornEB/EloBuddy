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

namespace SkinChanger
{
    class Program
    {
        public static string Model = ObjectManager.Player.BaseSkinName;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Chat.Print("Skin Changer by Thorn loaded!");
            Chat.OnInput += Chat_OnInput;
        }

        public static bool IsValidModel(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && RiotAsset.Exists(name);
        }

        private static void Chat_OnInput(ChatInputEventArgs args)
        {
            if (args.Input.ToLower().StartsWith("/model "))
            {
                string[] splits = args.Input.Split(' ');
                if (IsValidModel(splits[1]))
                { Player.SetModel(splits[1]); Model = splits[1]; }
                else Chat.Print("ERROR: Unknown model.");
                args.Input = " ";
            }
            if (args.Input.ToLower().StartsWith("/skin "))
            {
                string[] splits = args.Input.Split(' ');
                try
                {
                    var id = Convert.ToInt32(splits[1]);
                    Player.SetSkin(Model, id);
                    args.Input = " ";
                }
                catch { }
            }
        }
    }
}
