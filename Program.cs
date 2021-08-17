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
            
            bool MouseClickAvailable = false;

            StreamReader sr = new StreamReader("common.json");
            string json = sr.ReadToEnd();

            StreamReader SettingsSr = new StreamReader("Settings.json");
            string SettingsJson = SettingsSr.ReadToEnd();

            List<string> WordsDictionary = JsonConvert.DeserializeObject<List<string>>(json).Where(item => item.Length >= 4 && item.Length <= 12).ToList();


            HackGame GameSession = new HackGame(WordsDictionary);

            Terminal.Set("input.filter = [keyboard, mouse]; window: title='RobCo Industries™ Termlink',icon='icon.ico'");
            GameSession.SwitchColor("f3");//amber
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
                        GameSession = new HackGame(WordsDictionary);
                        GameSession.GenerateWordsTable();
                        GameSession.SwitchColor("f3");//amber
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
