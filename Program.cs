using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearLib;

namespace terminalhack
{
    class HackGame
    {
        private static readonly int OffsetInc = 12;
        private static readonly int TerminalHeight = 24;
        private static readonly int TerminalWidth = 64;
        private static readonly int TableHeight = 32;
        private static readonly int TableWidth = 12;
        private static readonly int OffsetMax = 65150;
        private System.Drawing.Point Cursor = new System.Drawing.Point(1, 0);
        private int OffsetStart = 0;
        private Random rnd = new Random();
        private List<int> PasswordLengthTable = new List<int>(5);
        private int PasswordLength = 0;
        private int BracketCount = 0;
        private int WordCount = 0;
        private List<List<string>> WordsTable = new List<List<string>>(TableHeight);
        private List<string> WordsDict = new List<string>();
        private string Password;
        public HackGame(int TerminalLevel, int ScienceSkillLevel, int LuckyLevel = 5)
        {
            this.OffsetStart = rnd.Next(4096, OffsetMax);
            for (int i = 0; i < TableHeight; i++)
            {
                this.WordsTable.Add(new List<string>(TableWidth));
            }
            for (int i = 0; i < 5; i++)
            {
                this.PasswordLengthTable.Add(4 + 2 * i);
            }
            this.PasswordLength = this.PasswordLengthTable[TerminalLevel];
            this.WordCount = this.PasswordLength - rnd.Next(0, (int)(LuckyLevel / 2)) - (ScienceSkillLevel / 10);
            this.WordCount = this.WordCount < 5 ? 5 : this.WordCount;
            this.BracketCount = 50 + LuckyLevel * 10;
            string options = "window: size =" + TerminalWidth.ToString() + "x" + TerminalHeight.ToString();
            Terminal.Set(options);
        }
        public void SetDict(List<string> Words)
        {
            foreach (var word in Words)
            {
                this.WordsDict.Add(word);
            }
        }
        public void GenerateWordsTable()
        {
            List<string> WordPool = new List<string>();
            for (int i = 0; i < WordCount; i++)
            {
                var j = rnd.Next(WordsDict.Count);
                while (WordPool.Contains(WordsDict[j])) ;
                WordPool.Add(WordsDict[j]);
            }
            this.Password = WordPool[rnd.Next(WordCount)];
            for (int i = 0; i < TableHeight; i++)
            {
                this.WordsTable[i].Add("0x" + (this.OffsetStart + OffsetInc * i).ToString("X")+" ");

                this.WordsTable[i].Add(this.WordsDict[rnd.Next(this.WordsDict.Count)]);
                //if (rnd.Next(100)<60) this.WordsTable[i].Add(this.WordsDict[rnd.Next(this.WordsDict.Count)]);
            }
        }
        public void ShowWordsTable()
        {
            for (int y = 0; y < TableHeight; y++)
            {
                int x = 0;
                int dx = 0;
                foreach (var item in this.WordsTable[y])
                {
                    if (x == this.Cursor.X && y == this.Cursor.Y)
                    {
                        Terminal.Color(System.Drawing.Color.DarkGreen);
                        Terminal.BkColor(System.Drawing.Color.LightGreen);
                    }
                    else
                    {
                        Terminal.BkColor(System.Drawing.Color.DarkGreen);
                        Terminal.Color(System.Drawing.Color.LightGreen);
                    }
                    
                    for (int i = 0; i < x; i++)
                    {
                        dx += this.WordsTable[y][i].Length;
                    }

                    if (y < TableHeight / 2)
                        Terminal.Print(0+dx, y + 4, item);
                    else
                        Terminal.Print(20+dx, y + 4 - TableHeight / 2, item);


                    x++;
                }



            }
            Terminal.Refresh();
            
        }
        public void MoveCursor(int dx, int dy)
        {
            this.Cursor.X += dx;
            this.Cursor.Y += dy;
            this.Cursor.Y = this.Cursor.Y % TableHeight;
            if (this.Cursor.X < 1)
            {
                this.Cursor.Y-=16;
                this.Cursor.Y = this.Cursor.Y % TableHeight;
                this.Cursor.X = this.Cursor.X % this.WordsTable[this.Cursor.Y].Count();
            } else 
            if (this.Cursor.X >= this.WordsTable[this.Cursor.Y].Count())
            {
                this.Cursor.Y += 16;
                this.Cursor.Y = this.Cursor.Y % TableHeight;
                this.Cursor.X = this.Cursor.X % this.WordsTable[this.Cursor.Y].Count();
            }
        }

    }
    class Programm
    {
        static void Main(string[] args)
        {
            string hexValue = 27.ToString("X");
            int decValue = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

            Random rnd = new Random();

            Terminal.Open();
            HackGame hg = new HackGame(4, 80);
            List<string> WordsDict = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                WordsDict.Add(rnd.Next(1000, 99999).ToString());
            }
            Terminal.BkColor(System.Drawing.Color.DarkGreen);
            Terminal.Color(System.Drawing.Color.LightGreen);
            Terminal.Clear();

            hg.SetDict(WordsDict);
            hg.GenerateWordsTable();

            string str = "";
            //Terminal.ReadStr(1, 1, ref str, 80);
            //Terminal.Print(1, 1, str.ToString());

            // Ждем, пока пользователь не закроет окно
            Terminal.Refresh();
            while (Terminal.HasInput() ? Terminal.Read() != Terminal.TK_CLOSE : true)
            {
                int dx = 0;
                int dy = 0;
                //if (Terminal.HasInput())
                {
                    int TK = Terminal.Read();
                    Terminal.Set("window: title="+"'"+Terminal.Peek().ToString()+"'");
                    dx = TK == Terminal.TK_LEFT ? -1 : TK == Terminal.TK_RIGHT?1:0;
                    dy = TK == Terminal.TK_UP ? -1 : TK == Terminal.TK_DOWN ? 1 : 0;
                }
                hg.MoveCursor(dx, dy);
                hg.ShowWordsTable();
            }

            Terminal.Close();
        }
    }

}
