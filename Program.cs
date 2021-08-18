using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearLib;
using Newtonsoft.Json;

namespace terminalhack
{
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    class Programm
    {
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

            HackGame GameSession = new HackGame(WordsDictionary,int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]),Settings["Language"].ToLower());
            
            
            GameSession.SwitchColor(Settings["ColorTheme"].ToLower());//amber
            

            if (int.Parse(Settings["ScienceLevel"]) < int.Parse(Settings["TerminalLevel"]))
            {
                Terminal.Clear();
                string temprus = "Вы не можете взломать терминал уровня {0} с наукой {1}".ToUpper();
                string tempen = "You can't hack lvl {0} terminal with science lvl {1}".ToUpper();
                if (Settings["Language"].ToLower().Equals("ru"))
                {
                    Terminal.Print(Terminal.State(Terminal.TK_WIDTH) / 2 - temprus.Length/2, Terminal.State(Terminal.TK_HEIGHT) / 2, temprus, int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]));
                }
                if (Settings["Language"].ToLower().Equals("en"))
                {
                    Terminal.Print(Terminal.State(Terminal.TK_WIDTH) / 2 - tempen.Length / 2, Terminal.State(Terminal.TK_HEIGHT) / 2, tempen, int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]));
                }
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
            //Terminal.Set("font: {0}, size={1}", Settings["Font"], Settings["FontSize"]);
            GameSession.GenerateWordsTable();
            GameSession.ShowFrame();
            while (Terminal.HasInput() ? Terminal.Read() != Terminal.TK_CLOSE : true)
            {
                int dx = 0;
                int dy = 0;
                int mx = -1;
                int my = -1;
                //if (Terminal.HasInput())
                //! (бонусная задача) Добавить звуки. 
                {//! Сделать нормальное приложение вокруг механики игры. А именно, починить управление (сейчас работает лишь частично). 
                    int TK = Terminal.Read();
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
                        GameSession = new HackGame(WordsDictionary, int.Parse(Settings["TerminalLevel"]), int.Parse(Settings["ScienceLevel"]),Settings["Language"].ToLower());
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
                GameSession.ShowFrame();
            }
            /*
            string hexValue = 27.ToString("X");
            int decValue = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

            string str = "";
            Terminal.ReadStr(1, 1, ref str, 80);
            Terminal.Print(1, 1, str.ToString());
            Terminal.Close();
            */
        }
    }

}
