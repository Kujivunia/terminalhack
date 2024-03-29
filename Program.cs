﻿using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearLib;
using Newtonsoft.Json;
using System.Threading;

namespace terminalhack
{

    class Programm
    {
        //public static HackGame GameSession;
        //public bool bCanShowing;
        private static System.Collections.Generic.Dictionary<string, string> Menu(HackGame qwe)
        {
            System.Collections.Generic.Dictionary<string, string> Settings = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(Read());
            string Read()
            {
                StreamReader SettingsSr = new StreamReader("Settings.json");
                string SettingsJson = SettingsSr.ReadToEnd();
                SettingsSr.Close();
                return SettingsJson;
            }

            void Write(string input)
            {
                StreamWriter SettingsWr = new StreamWriter("Settings.json");
                SettingsWr.Write(input);
                SettingsWr.Close();
            }

            void ShowMenu()
            {
                string Square = "";
                Terminal.BkColor(System.Drawing.Color.Blue);
                Terminal.Color(System.Drawing.Color.Yellow);
                Terminal.ClearArea(0, 0, (Terminal.TK_WIDTH / 2), 6);
                Terminal.Print(0, 0, "ScienceLvl: {0}", Settings["ScienceLevel"]);
                Terminal.Print(0, 1, "TerminalLvl: {0}", Settings["TerminalLevel"]);
                Terminal.Print(0, 2, "ColorTheme: |f3| |f4| |amber|");
                Terminal.Print(0, 3, "Language: |ru| |en|");
                Terminal.Print(0, 4, "FontSize: |12| |18| |24|");
                Terminal.Print(0, 5, "SlowMode: |ON| |OFF|");
                
                Terminal.ClearArea(17, 0, 21, 1);
                Terminal.Print(18, 0, "┣┷┷┷┷╋┷┷┷┷╋┷┷┷┷╋┷┷┷┷┫");
                Terminal.Print(18, 1, "┣┷┷┷┷╋┷┷┷┷╋┷┷┷┷╋┷┷┷┷┫");
                //Terminal.Layer(1);
                Terminal.Print(18 + int.Parse(Settings["ScienceLevel"])/5, 0, Square);
                Terminal.Print(18 + int.Parse(Settings["TerminalLevel"]) / 5, 1, Square);
                //Terminal.Layer(0);
                Terminal.Color(System.Drawing.Color.Blue);
                Terminal.BkColor(System.Drawing.Color.Yellow);
                if (Settings["ColorTheme"].Equals("f3"))
                {
                    Terminal.ClearArea(12, 2, 4, 1);
                    Terminal.Print(12, 2, "|f3|");
                }
                if (Settings["ColorTheme"].Equals("f4"))
                {
                    Terminal.ClearArea(17, 2, 4, 1);
                    Terminal.Print(17, 2, "|f4|");
                }
                if (Settings["ColorTheme"].Equals("amber"))
                {
                    Terminal.ClearArea(22, 2, 7, 1);
                    Terminal.Print(22, 2, "|amber|");
                }
                if (Settings["Language"].Equals("ru"))
                {
                    Terminal.ClearArea(10, 3, 4, 1);
                    Terminal.Print(10, 3, "|ru|");
                }
                if (Settings["Language"].Equals("en"))
                {
                    Terminal.ClearArea(15, 3, 4, 1);
                    Terminal.Print(15, 3, "|en|");
                }
                if (Settings["FontSize"].Equals("12"))
                {
                    Terminal.ClearArea(10, 4, 4, 1);
                    Terminal.Print(10, 4, "|12|");
                }
                if (Settings["FontSize"].Equals("18"))
                {
                    Terminal.ClearArea(15, 4, 4, 1);
                    Terminal.Print(15, 4, "|18|");
                }
                if (Settings["FontSize"].Equals("24"))
                {
                    Terminal.ClearArea(20, 4, 4, 1);
                    Terminal.Print(20, 4, "|24|");
                }
                if (Settings["SlowMode"].Equals("True"))
                {
                    Terminal.ClearArea(10, 5, 4, 1);
                    Terminal.Print(10, 5, "|ON|");
                }
                if (Settings["SlowMode"].Equals("False"))
                {
                    Terminal.ClearArea(15, 5, 5, 1);
                    Terminal.Print(15, 5, "|OFF|");
                }
                Terminal.Refresh();
            }
            ShowMenu();

            bool MouseClickAvailable = false;

            {
                int mx = -1;
                int my = -1;
                int TK = 0;
                StringBuilder TerminalLvlStr = new StringBuilder("");
                StringBuilder ScienceLvlStr = new StringBuilder("");
                while (Terminal.HasInput() ? Terminal.Read() != Terminal.TK_CLOSE : true)
                {
                    ShowMenu();
                    while (Terminal.HasInput())
                    {
                        TK = Terminal.Read();
                    }
                    TK = Terminal.Read();
                    if (TK == Terminal.TK_CLOSE)
                    {
                        Terminal.Close();
                    }
                    if (TK == Terminal.TK_ESCAPE)
                    {
                        Write(JsonConvert.SerializeObject(Settings));
                        return Settings;
                    }
                    if (TK == Terminal.TK_MOUSE_MOVE)
                    {
                        mx = Terminal.State(Terminal.TK_MOUSE_X);
                        my = Terminal.State(Terminal.TK_MOUSE_Y);
                        MouseClickAvailable = true;

                    }

                    if (TK == Terminal.TK_MOUSE_LEFT)
                    {
                        switch (my)
                        {
                            case 0:
                                {
                                    if (mx<16)
                                    {
                                        Terminal.ClearArea(12, 0, 4, 1);
                                        Terminal.ReadStr(12, 0, ScienceLvlStr, 3);
                                        if (ScienceLvlStr.ToString().Length < 1) ScienceLvlStr.Append("50");
                                        Settings["ScienceLevel"] = (ScienceLvlStr.ToString());
                                        if (int.Parse(Settings["ScienceLevel"]) < 0)
                                        {
                                            Settings["ScienceLevel"] = (0).ToString();
                                        }
                                        if (int.Parse(Settings["ScienceLevel"]) > 100)
                                        {
                                            Settings["ScienceLevel"] = (100).ToString();
                                        }
                                        if (int.Parse(Settings["ScienceLevel"]) < int.Parse(Settings["TerminalLevel"]))
                                        {
                                            Settings["TerminalLevel"] = Settings["ScienceLevel"];
                                        }

                                    }
                                    else if(mx<(18+21))
                                    {
                                        if (ScienceLvlStr.ToString().Length < 1) ScienceLvlStr.Append("50");
                                        Settings["ScienceLevel"] = ((mx - 18) * 5).ToString();
                                        if (int.Parse(Settings["ScienceLevel"]) < 0)
                                        {
                                            Settings["ScienceLevel"] = (0).ToString();
                                        }
                                        if (int.Parse(Settings["ScienceLevel"]) > 100)
                                        {
                                            Settings["ScienceLevel"] = (100).ToString();
                                        }
                                        if (int.Parse(Settings["ScienceLevel"]) < int.Parse(Settings["TerminalLevel"]))
                                        {
                                            Settings["TerminalLevel"] = Settings["ScienceLevel"];
                                        }
                                    }
                                    
                                    break;
                                }
                            case 1:
                                {
                                    if (mx < 17)
                                    {
                                        Terminal.ClearArea(13, 1, 4, 1);
                                        Terminal.ReadStr(13, 1, TerminalLvlStr, 3);
                                        if (TerminalLvlStr.ToString().Length < 1) TerminalLvlStr.Append("50");
                                        Settings["TerminalLevel"] = (TerminalLvlStr.ToString());
                                        if (int.Parse(Settings["TerminalLevel"]) < 0)
                                        {
                                            Settings["TerminalLevel"] = (0).ToString();
                                        }
                                        if (int.Parse(Settings["TerminalLevel"]) > 100)
                                        {
                                            Settings["TerminalLevel"] = (100).ToString();
                                        }
                                        if (int.Parse(Settings["ScienceLevel"]) < int.Parse(Settings["TerminalLevel"]))
                                        {
                                            Settings["ScienceLevel"] = Settings["TerminalLevel"];
                                        }
                                    }
                                    else if (mx < (18 + 21))
                                    {
                                        if (TerminalLvlStr.ToString().Length < 1) TerminalLvlStr.Append("50");
                                        Settings["TerminalLevel"] = ((mx - 18) * 5).ToString();
                                        if (int.Parse(Settings["TerminalLevel"]) < 0)
                                        {
                                            Settings["TerminalLevel"] = (0).ToString();
                                        }
                                        if (int.Parse(Settings["TerminalLevel"]) > 100)
                                        {
                                            Settings["TerminalLevel"] = (100).ToString();
                                        }
                                        if (int.Parse(Settings["ScienceLevel"]) < int.Parse(Settings["TerminalLevel"]))
                                        {
                                            Settings["ScienceLevel"] = Settings["TerminalLevel"];
                                        }
                                    }
                                        break;
                                }
                            case 2:
                                {
                                    if (mx > 11 && mx < 16) Settings["ColorTheme"] = "f3";
                                    if (mx > 16 && mx < 21) Settings["ColorTheme"] = "f4";
                                    if (mx > 21 && mx < 28) Settings["ColorTheme"] = "amber";
                                    break;
                                }
                            case 3:
                                {
                                    if (mx > 9 && mx < 14) Settings["Language"] = "ru";
                                    if (mx > 14 && mx < 19) Settings["Language"] = "en";
                                    break;
                                }
                            case 4:
                                {
                                    if (mx > 9 && mx < 14) Settings["FontSize"] = "12";
                                    if (mx > 14 && mx < 19) Settings["FontSize"] = "18";
                                    if (mx > 19 && mx < 24) Settings["FontSize"] = "24";
                                    break;
                                }
                            case 5:
                                {
                                    if (mx > 9 && mx < 14) Settings["SlowMode"] = "True";
                                    if (mx > 14 && mx < 19) Settings["SlowMode"] = "False";
                                    break;
                                }
                            default:
                                break;
                        }
                    }

                }


            }

            Write(JsonConvert.SerializeObject(Settings));
            return Settings;
        }
        static void Main(string[] args)
        {

            System.Collections.Generic.Dictionary<string, string> Settings = new Dictionary<string, string>();
            Settings.Add("TerminalLevel", "50");
            Settings.Add("ScienceLevel", "50");
            Settings.Add("ColorTheme", "f3");
            Settings.Add("Language", "ru");
            Settings.Add("FontSize", "18");
            Settings.Add("Font", "FixedsysRus.ttf");

            bool MouseClickAvailable = false;

            StreamReader sr = new StreamReader("common.json");
            string json = sr.ReadToEnd();
            sr.Close();
            StreamReader SettingsSr = new StreamReader("Settings.json");
            string SettingsJson = SettingsSr.ReadToEnd();
            SettingsSr.Close();
            List<string> WordsDictionary = JsonConvert.DeserializeObject<List<string>>(json).Where(item => item.Length >= 4 && item.Length <= 12).ToList();
            Settings = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(SettingsJson);

            HackGame GameSession = new HackGame(WordsDictionary, int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]), Settings["Language"].ToLower(), bool.Parse(Settings["SlowMode"]));

            System.IO.StreamReader StringsSr = new System.IO.StreamReader("Strings.json");
            string StringsJson = StringsSr.ReadToEnd();
            StringsSr.Close();
            Dictionary<string, Dictionary<string, string>> Strings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(StringsJson);

            GameSession.SwitchColor(Settings["ColorTheme"].ToLower());//amber

                if (int.Parse(Settings["ScienceLevel"]) < int.Parse(Settings["TerminalLevel"]))
                {
                    Terminal.Clear();
                    Terminal.Print(Terminal.State(Terminal.TK_WIDTH) / 2 - Strings[Settings["Language"].ToLower()]["TooHard"].Length / 2, Terminal.State(Terminal.TK_HEIGHT) / 2, Strings[Settings["Language"].ToLower()]["TooHard"], int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]));
                    Terminal.Refresh();
                    //Settings = Menu(GameSession);
                    Terminal.Read();
                    return;
                }


            Terminal.Set("input.filter = [keyboard, mouse]; window: title='RobCo Industries™ Termlink',icon='icon.ico';");
            try
            {
                Terminal.Set("font: {0}, size={1}", Settings["Font"], Settings["FontSize"]);
            }
            catch (System.Exception)
            {
                Terminal.Set("font:");
            }

            //Terminal.Set("font: {0}, size={1}", Settings["Font"], Settings["FontSize"]);
            GameSession.GenerateWordsTable();
            GameSession.ShowFrame();
            GameSession.ShowFrame();
            int TK = 0;
            while (Terminal.HasInput() ? Terminal.Read() != Terminal.TK_CLOSE : true)
            //while (TK != Terminal.TK_CLOSE)
            {
                int dx = 0;
                int dy = 0;
                int mx = -1;
                int my = -1;

                //if (Terminal.HasInput())
                //! (бонусная задача) Добавить звуки. 
                {//! Сделать нормальное приложение вокруг механики игры. А именно, починить управление (сейчас работает лишь частично). 
                    GameSession.ShowFrame();
                    while (Terminal.HasInput())
                    {
                        TK = Terminal.Read();
                    }
                    TK = Terminal.Read();
                    if (TK == Terminal.TK_LEFT || TK == Terminal.TK_RIGHT || TK == Terminal.TK_DOWN || TK == Terminal.TK_UP)
                    {
                        dx = TK == Terminal.TK_LEFT ? -1 : TK == Terminal.TK_RIGHT ? 1 : 0;
                        dy = TK == Terminal.TK_UP ? -1 : TK == Terminal.TK_DOWN ? 1 : 0;
                    }
                    if (TK == Terminal.TK_A || TK == Terminal.TK_W || TK == Terminal.TK_D || TK == Terminal.TK_S)
                    {
                        dx = TK == Terminal.TK_A ? -1 : TK == Terminal.TK_D ? 1 : 0;
                        dy = TK == Terminal.TK_W ? -1 : TK == Terminal.TK_S ? 1 : 0;
                    }
                    GameSession.MoveCursor(new System.Drawing.Point(dx, dy));
                    if (TK == Terminal.TK_BACKSPACE)
                    {
                        GameSession = new HackGame(WordsDictionary, int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]), Settings["Language"].ToLower(), bool.Parse(Settings["SlowMode"]));
                        GameSession.GenerateWordsTable();
                        GameSession.SwitchColor(Settings["ColorTheme"].ToLower());//amber
                        GameSession.ShowFrame();
                        GameSession.ShowFrame();
                    }
                    if (TK == Terminal.TK_CLOSE)
                    {
                        Terminal.Close();
                    }
                    if (TK == Terminal.TK_ESCAPE)
                    {
                        Settings = Menu(GameSession);
                        GameSession.SwitchColor(Settings["ColorTheme"].ToLower());
                        Terminal.Set("input.filter = [keyboard, mouse]; window: title='RobCo Industries™ Termlink',icon='icon.ico';");
                        try
                        {
                            Terminal.Set("font: {0}, size={1}", Settings["Font"], Settings["FontSize"]);
                        }
                        catch (System.Exception)
                        {
                            Terminal.Set("font:");
                        }
                        GameSession.ShowFrame();
                    }
                    if (TK == Terminal.TK_MOUSE_MOVE)
                    {
                        mx = Terminal.State(Terminal.TK_MOUSE_X);
                        my = Terminal.State(Terminal.TK_MOUSE_Y);
                        MouseClickAvailable = false;
                        if (mx >= 0 && my >= 0) GameSession.MoveToCursor(new System.Drawing.Point(mx, my));
                        if (((mx >= 7 && mx < 19) || (mx > 26 && mx < 39)) && my >= (21 - 32 / 2))
                        {
                            MouseClickAvailable = true;
                        }
                    }
                    if (TK == Terminal.TK_ENTER || TK == Terminal.TK_SPACE || TK == Terminal.TK_E || TK == Terminal.TK_MOUSE_LEFT)
                    {
                        if (TK != Terminal.TK_MOUSE_LEFT)
                        {
                            int Bulls = GameSession.CheckWord();
                        }
                        if (TK == Terminal.TK_MOUSE_LEFT && MouseClickAvailable)
                        {
                            int Bulls = GameSession.CheckWord();
                        }
                    }
                }

                //! очищать очередь команд здесь.
            }
            /*
            Thread ControlThread = new Thread(new ThreadStart(Control));
            Thread ShowThread = new Thread(new ThreadStart(Show));
            ControlThread.Start();
            ShowThread.Start();
            */
            /*
            string hexValue = 27.ToString("X");
            int decValue = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

            string str = "";
            Terminal.ReadStr(1, 1, ref str, 80);
            Terminal.Print(1, 1, str.ToString());
            Terminal.Close();
            */
        }
        /*
        public static void Control()
        {
            System.Collections.Generic.Dictionary<string, string> Settings = new Dictionary<string, string>();
            Settings.Add("TerminalLevel", "50");
            Settings.Add("ScienceLevel", "50");
            Settings.Add("ColorTheme", "f3");
            Settings.Add("Language", "ru");
            Settings.Add("FontSize", "18");
            Settings.Add("Font", "FixedsysRus.ttf");

            bool MouseClickAvailable = false;

            StreamReader sr = new StreamReader("common.json");
            string json = sr.ReadToEnd();
            sr.Close();
            StreamReader SettingsSr = new StreamReader("Settings.json");
            string SettingsJson = SettingsSr.ReadToEnd();
            SettingsSr.Close();
            List<string> WordsDictionary = JsonConvert.DeserializeObject<List<string>>(json).Where(item => item.Length >= 4 && item.Length <= 12).ToList();
            Settings = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(SettingsJson);

            GameSession = new HackGame(WordsDictionary, int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]), Settings["Language"].ToLower(), bool.Parse(Settings["SlowMode"]));

            System.IO.StreamReader StringsSr = new System.IO.StreamReader("Strings.json");
            string StringsJson = StringsSr.ReadToEnd();
            StringsSr.Close();
            Dictionary<string, Dictionary<string, string>> Strings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(StringsJson);

            GameSession.SwitchColor(Settings["ColorTheme"].ToLower());//amber


            if (int.Parse(Settings["ScienceLevel"]) < int.Parse(Settings["TerminalLevel"]))
            {
                Terminal.Clear();
                Terminal.Print(Terminal.State(Terminal.TK_WIDTH) / 2 - Strings[Settings["Language"].ToLower()]["TooHard"].Length / 2, Terminal.State(Terminal.TK_HEIGHT) / 2, Strings[Settings["Language"].ToLower()]["TooHard"], int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]));
                Terminal.Refresh();
                Terminal.Read();
                return;
            }

            Terminal.Set("input.filter = [keyboard, mouse]; window: title='RobCo Industries™ Termlink',icon='icon.ico';");
            try
            {
                Terminal.Set("font: {0}, size={1}", Settings["Font"], Settings["FontSize"]);
            }
            catch (System.Exception)
            {
                Terminal.Set("font:");
            }
            int TK = 0;
            while (Terminal.HasInput() ? Terminal.Read() != Terminal.TK_CLOSE : true)
            {
                int dx = 0;
                int dy = 0;
                int mx = -1;
                int my = -1;

                //if (Terminal.HasInput())
                //! (бонусная задача) Добавить звуки. 
                {//! Сделать нормальное приложение вокруг механики игры. А именно, починить управление (сейчас работает лишь частично). 
                    TK = Terminal.Read();
                    if (TK == Terminal.TK_LEFT || TK == Terminal.TK_RIGHT || TK == Terminal.TK_DOWN || TK == Terminal.TK_UP)
                    {
                        dx = TK == Terminal.TK_LEFT ? -1 : TK == Terminal.TK_RIGHT ? 1 : 0;
                        dy = TK == Terminal.TK_UP ? -1 : TK == Terminal.TK_DOWN ? 1 : 0;
                    }
                    if (TK == Terminal.TK_A || TK == Terminal.TK_W || TK == Terminal.TK_D || TK == Terminal.TK_S)
                    {
                        dx = TK == Terminal.TK_A ? -1 : TK == Terminal.TK_D ? 1 : 0;
                        dy = TK == Terminal.TK_W ? -1 : TK == Terminal.TK_S ? 1 : 0;
                    }
                    GameSession.MoveCursor(new System.Drawing.Point(dx, dy));
                    if (TK == Terminal.TK_BACKSPACE)
                    {
                        GameSession = new HackGame(WordsDictionary, int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]), Settings["Language"].ToLower(), bool.Parse(Settings["SlowMode"]));
                        GameSession.GenerateWordsTable();
                        GameSession.SwitchColor(Settings["ColorTheme"].ToLower());//amber
                    }
                    if (TK == Terminal.TK_ESCAPE || TK == Terminal.TK_CLOSE)
                    {
                        Terminal.Close();
                    }
                    if (TK == Terminal.TK_MOUSE_MOVE)
                    {
                        mx = Terminal.State(Terminal.TK_MOUSE_X);
                        my = Terminal.State(Terminal.TK_MOUSE_Y);
                        MouseClickAvailable = false;
                        if (mx >= 0 && my >= 0) GameSession.MoveToCursor(new System.Drawing.Point(mx, my));
                        if (((mx >= 7 && mx < 19) || (mx > 26 && mx < 39)) && my >= (21 - 32 / 2))
                        {
                            MouseClickAvailable = true;
                        }
                    }
                    if (TK == Terminal.TK_ENTER || TK == Terminal.TK_SPACE || TK == Terminal.TK_E || TK == Terminal.TK_MOUSE_LEFT)
                    {
                        if (TK != Terminal.TK_MOUSE_LEFT)
                        {
                            int Bulls = GameSession.CheckWord();
                        }
                        if (TK == Terminal.TK_MOUSE_LEFT && MouseClickAvailable)
                        {
                            int Bulls = GameSession.CheckWord();
                        }
                    }
                }
            }
        }*/
        /*
        public static void Show()
        {
            while (true)
            {
                GameSession.ShowFrame();
            }
        }
        */
    }

}
