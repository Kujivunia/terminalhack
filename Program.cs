using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearLib;
using Newtonsoft.Json;

namespace terminalhack
{


    class HackGameOld
    {
        private static readonly int OffsetInc = 12;
        private static readonly int TerminalHeight = 24;
        private static readonly int TerminalWidth = 64;
        private static readonly int TableHeight = 32;
        private static readonly int TableWidth = 12 + 1;
        private static readonly int OffsetMax = 65150;
        private static readonly string TrashChars = "!\"#$%&\'()*+/:;<=>?@[\\]^_{|}";
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
        private static int Tor(int a, int b, int c)
        {
            int result = 0;

            while (c < a)
            {
                c = (b - a + c + 1);
            }
            while (c > b)
            {
                c = (c + a - b - 1);
            }
            result = c;
            return result;
        }
        public HackGameOld(int TerminalLevel, int ScienceSkillLevel, int LuckyLevel = 5)
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
                this.WordsTable[i].Add("0x" + (this.OffsetStart + OffsetInc * i).ToString("X") + " ");

                this.WordsTable[i].Add(this.WordsDict[rnd.Next(this.WordsDict.Count)]);
                if (rnd.Next(100) < 60) this.WordsTable[i].Add(this.WordsDict[rnd.Next(this.WordsDict.Count)]);
                if (rnd.Next(100) < 40) this.WordsTable[i].Add(this.WordsDict[rnd.Next(this.WordsDict.Count)]);
                if (rnd.Next(100) < 20) this.WordsTable[i].Add(this.WordsDict[rnd.Next(this.WordsDict.Count)]);
            }
        }
        public void ShowWordsTable()
        {
            for (int y = 0; y < TableHeight; y++)
            {
                int x = 0;

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
                    int dx = 0;
                    for (int i = 0; i < x; i++)
                    {
                        dx += this.WordsTable[y][i].Length;
                    }

                    if (y < TableHeight / 2)
                        Terminal.Print(0 + dx, y + 4, item);
                    else
                        Terminal.Print(20 + dx, y + 4 - TableHeight / 2, item);


                    x++;
                }



            }
            Terminal.Refresh();
            Terminal.Set("window: title=" + "'" + this.Cursor.ToString() + "'");
        }
        public void MoveCursor(int dx, int dy)
        {
            this.Cursor.X += dx;
            this.Cursor.Y += dy;
            this.Cursor.Y = Tor(0, TableHeight - 1, this.Cursor.Y);
            if (this.Cursor.X < 1)
            {
                this.Cursor.Y -= 16;
                this.Cursor.Y = Tor(0, TableHeight - 1, this.Cursor.Y);
                this.Cursor.X = this.WordsTable[this.Cursor.Y].Count() - 1;
            }
            else
            if (this.Cursor.X >= this.WordsTable[this.Cursor.Y].Count())
            {
                this.Cursor.Y += 16;
                this.Cursor.X = 1;
            }
            this.Cursor.Y = Tor(0, TableHeight - 1, this.Cursor.Y);
            this.Cursor.X = Tor(1, this.WordsTable[this.Cursor.Y].Count(), this.Cursor.X);
        }
    }
    //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
    class HackGame
    {
        private Random rnd = new Random();

        private static readonly int DumpHeight = 32;
        private static readonly int DumpWidth = 12;

        private static readonly int OffsetInc = DumpWidth;
        private static readonly int OffsetMin = 4096;
        private static readonly int OffsetMax = 65150;
        private int OffsetStart = 0;

        private static readonly int TerminalHeight = 21;//25
        private static readonly int TerminalWidth = 64;//80



        private static readonly string TrashChars = "!\"#$%&\'()*+/:;<=>?@[\\]^_{|}";
        private static readonly string Brackets = "<>[]{}()";
        private System.Drawing.Point Cursor = new System.Drawing.Point(0, 0);
        private int CursorFlat = 0;
        private int CursorWordIndex = 0;

        private int PasswordLength = 0;
        private int BracketCount = 0;
        private int WordCount = 0;
        private List<string> WordsTable = new List<string>(DumpHeight * DumpWidth);
        private List<KeyValuePair<int, int>> WordsTableRanges = new List<KeyValuePair<int, int>>(DumpHeight * DumpWidth);
        private List<string> WordsDict = new List<string>();
        private List<string> HexAddresses = new List<string>();
        private List<string> IOLog = new List<string>();
        private string Password;
        private static int Tor(int a, int b, int c)
        {
            int result = 0;

            while (c < a)
            {
                c = (b - a + c + 1);
            }
            while (c > b)
            {
                c = (c + a - b - 1);
            }
            result = c;
            return result;
        }

        public HackGame(List<string> WordsList, int TerminalLevel = 2, int ScienceSkillLevel = 50, int LuckyLevel = 5)
        {
            this.OffsetStart = rnd.Next(OffsetMin, OffsetMax);
            this.PasswordLength = 4 + 2 * TerminalLevel;
            this.WordCount = this.PasswordLength - rnd.Next(0, (int)(LuckyLevel / 2)) - (ScienceSkillLevel / 10);
            this.WordCount = this.WordCount < 5 ? 5 : this.WordCount;
            this.BracketCount = 50 + LuckyLevel * 10;

            foreach (var item in WordsList.Where(item => item.Length == this.PasswordLength).ToList())
            {
                this.WordsDict.Add(item);
            }

            string options = "window: size =" + TerminalWidth.ToString() + "x" + TerminalHeight.ToString();
            Terminal.Open();
            Terminal.Set(options);
            Terminal.BkColor(System.Drawing.Color.DarkGreen);
            Terminal.Color(System.Drawing.Color.LightGreen);
            Terminal.Clear();
            Terminal.Refresh();
        }

        private void WordsTableRangesFill()
        {
            int i = 0;
            foreach (var Word in this.WordsTable)
            {
                this.WordsTableRanges.Add(new KeyValuePair<int, int>(i, Word.Length-1));
                i += Word.Length;
            }
        }
        private void CursorWordIndexMath()
        {
            int index = 0;
            for (int i = 0; i < this.WordsTableRanges.Count; i++)
            {
                if (this.CursorFlat >= this.WordsTableRanges[i].Key && this.CursorFlat <= this.WordsTableRanges[i].Value)
                {
                    index = i;
                }
            }
            this.CursorWordIndex = index;
        }
        public void GenerateWordsTable()
        {
            for (int i = 0; i < this.WordCount; i++)//заполняет таблицу паролями
            {
                var j = rnd.Next(this.WordsDict.Count);
                while (this.WordsTable.Contains(this.WordsDict[j]))
                    j = rnd.Next(this.WordsDict.Count);
                this.WordsTable.Add(this.WordsDict[j]);
            }
            this.Password = this.WordsTable[rnd.Next(this.WordCount)];

            while (this.WordsTable.Count() + ((this.PasswordLength - 1) * this.WordCount) < DumpHeight * DumpWidth)
            {
                this.WordsTable.Insert(rnd.Next(this.WordsTable.Count()+1), TrashChars[rnd.Next(TrashChars.Count())].ToString());//заполняет таблицу мусором
            }

            for (int i = 0; i < DumpHeight; i++)
            {
                this.HexAddresses.Add("0x" + (this.OffsetStart + OffsetInc * i).ToString("X"));
            }

            WordsTableRangesFill();

        }
        
        public Boolean CheckWord()
        {
            
            return false;
        }
        public void ShowFrame()
        {
            int i = 0;
            foreach (var Word in this.WordsTable)
            {
                foreach (var Char in Word)
                {
                    if ((int)(i / DumpWidth) < DumpHeight / 2)
                        Terminal.Print((int)(i % DumpWidth) + 7, (int)(i / DumpWidth) + (TerminalHeight-DumpHeight/2), Char.ToString() == "]" || Char.ToString() == "[" ? Char.ToString() + Char.ToString() : Char.ToString().ToUpper());
                    else
                        Terminal.Print((int)(i % DumpWidth) + 20 + 7, (int)(i / DumpWidth) + (TerminalHeight - DumpHeight / 2) - DumpHeight / 2, Char.ToString() == "]" || Char.ToString() == "[" ? Char.ToString() + Char.ToString() : Char.ToString().ToUpper());
                    i++;
                }
            }
            i = 0;
            foreach (var Address in HexAddresses)
            {
                if (i < DumpHeight / 2)
                    Terminal.Print(0, (int)(i) + (TerminalHeight - DumpHeight / 2), Address.ToString());
                else
                    Terminal.Print(20, (int)(i) + (TerminalHeight - DumpHeight / 2) - DumpHeight / 2, Address.ToString());
                i++;
            }
            Terminal.Print(0, 0, "robco industries (tm) termlink protocol\nвведите пароль\n\n4 попытки осталось: x x x x".ToUpper());
            Terminal.Print(40, (TerminalHeight - 1), ">"+this.Password.ToUpper());
            
            Terminal.Refresh();
        }
        private static int FlatteringCursor(System.Drawing.Point CursorPoint)
        {
            return (DumpWidth * CursorPoint.Y + CursorPoint.X);
        }
        public void MoveCursor(System.Drawing.Point MoveVector)
        {
            this.Cursor.X += MoveVector.X;
            this.Cursor.Y += MoveVector.Y;
            this.CursorFlat = FlatteringCursor(Cursor);
            CursorWordIndexMath();
        }
        public void MoveToCursor(System.Drawing.Point MovePoint)
        {
            this.Cursor.X = MovePoint.X;
            this.Cursor.Y = MovePoint.Y;
            this.CursorFlat = FlatteringCursor(Cursor);
            CursorWordIndexMath();
        }


    }
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    class Programm
    {
        static void Main(string[] args)
        {
            StreamReader sr = new StreamReader("common.json");
            string json = sr.ReadToEnd();
            List<string> WordsDictionary = JsonConvert.DeserializeObject<List<string>>(json).Where(item => item.Length >= 4 && item.Length <= 12).ToList();
            HackGame qwe = new HackGame(WordsDictionary);
            qwe.GenerateWordsTable();
            qwe.ShowFrame();
            Terminal.Read();

            string hexValue = 27.ToString("X");
            int decValue = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

            Random rnd = new Random();

            Terminal.Open();
            HackGameOld hg = new HackGameOld(4, 80);
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
                    //Terminal.Set("window: title=" + "'" + Terminal.Peek().ToString() + "'");
                    dx = TK == Terminal.TK_LEFT ? -1 : TK == Terminal.TK_RIGHT ? 1 : 0;
                    dy = TK == Terminal.TK_UP ? -1 : TK == Terminal.TK_DOWN ? 1 : 0;
                }
                hg.MoveCursor(dx, dy);
                hg.ShowWordsTable();
            }

            Terminal.Close();
        }
    }

}
