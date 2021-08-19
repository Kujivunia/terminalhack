using System;
using System.Collections.Generic;
using System.Linq;
using BearLib;

namespace terminalhack
{
    //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
    class HackGame
    {
        private Random rnd = new Random();

        private static readonly int DumpHeight = 34;//32-34
        private static readonly int DumpWidth = 12;

        private static readonly int OffsetInc = DumpWidth;
        private static readonly int OffsetMin = 4096;
        private static readonly int OffsetMax = 65150;
        private int OffsetStart = 0;

        private static readonly int TerminalHeight = 22;//25-21
        private int TerminalWidth = 58;//80-64-56

        private int Attempts = 4;

        private static readonly string TrashChars = "!\"#$%&\'()*+/:;<=>?@[\\]^_{|}";
        //private static readonly string Brackets = "<>[]{}()";
        private static readonly string OpenBrackets = "<[{(";
        private static readonly string CloseBrackets = ">]})";
        private System.Drawing.Point Cursor = new System.Drawing.Point(0, 0);
        private int CursorFlat = 0;
        private int CursorWordIndex = 0;

        private int PasswordLength = 0;
        //private int BracketCount = 0;
        private int WordCount = 0;
        private List<string> WordsTable = new List<string>(DumpHeight * DumpWidth);
        private List<KeyValuePair<int, int>> WordsTableRanges = new List<KeyValuePair<int, int>>(DumpHeight * DumpWidth);
        private List<string> WordsDict = new List<string>();
        private List<string> HexAddresses = new List<string>();
        private List<string> IOLog = new List<string>();
        private List<int> UsedBracketsIndex = new List<int>();
        private string Password;
        private List<string> Duds = new List<string>();
        private bool bBlockScreen = false;
        private bool bUnlockScreen = false;
        private bool bUnlocked = false;
        private bool bLocked = false;
        private System.Drawing.Color Color = System.Drawing.Color.LightGreen;
        private System.Drawing.Color BkColor = System.Drawing.Color.DarkGreen;
        private Dictionary<string, Dictionary<string, string>> Strings = new Dictionary<string, Dictionary<string, string>>();
        private string Language = "ru";
        private bool bLaunching = true;
        private bool bAfterLaunchingScreen = true;
        bool bSlowMode = false;
        public void SwitchColor(string ColorTheme)
        {
            ColorTheme = ColorTheme.ToLower();
            switch (ColorTheme)
            {

                case "f3":
                    {
                        this.BkColor = System.Drawing.Color.DarkGreen;
                        this.Color = System.Drawing.Color.LightGreen;
                        break;
                    }
                case "fnv":
                    {
                        this.BkColor = System.Drawing.Color.DarkGreen;
                        this.Color = System.Drawing.Color.LightGreen;
                        break;
                    }
                case "f4":
                    {
                        this.BkColor = System.Drawing.Color.Black;
                        this.Color = System.Drawing.Color.FromArgb(51, 255, 51);//Apple ][
                        break;
                    }
                case "amber":
                    {
                        this.BkColor = System.Drawing.Color.FromArgb(40, 40, 40);//bg
                        this.Color = System.Drawing.Color.FromArgb(255, 176, 0);//Amber
                        break;
                    }
                default:
                    {
                        this.BkColor = System.Drawing.Color.DarkGreen;
                        this.Color = System.Drawing.Color.LightGreen;
                        break;
                    }

            }

        }
        private int DudsAndPasswordCount(int ScienceLvl, int TerminalLvl)
        {
            if (TerminalLvl == 100 && ScienceLvl == 100)
                return 13;
            else
                return ((int)System.Math.Round(15d / (100 - TerminalLvl) * (100 - ScienceLvl)) + 5) < 5 ? 5 : ((int)System.Math.Round(15d / (100 - TerminalLvl) * (100 - ScienceLvl)) + 5);
        }

        public void SetUserColor(System.Drawing.Color Color, System.Drawing.Color BkColor)
        {
            Terminal.BkColor(BkColor);
            Terminal.Color(this.Color);
        }
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
        private int WordBulls(string foo, string bar)
        {
            if (foo.Length != bar.Length) return 0;
            int Bulls = 0;
            for (int i = 0; i < foo.Length; i++)
            {
                Bulls += foo[i] == bar[i] ? 1 : 0;//готово
            }

            return Bulls;
        }
        private int UsedBracketsIndexFind(int WordIndex = -1, int BracketIndex = -1)
        {
            int result = 0;
            if (BracketIndex == -1)
            {
                for (int i = 0; i < WordIndex + 1; i++)
                {
                    if (OpenBrackets.Contains(this.WordsTable[i]))
                    {
                        result++;
                    }

                }
            }
            else if (WordIndex == -1)
            {
                for (int i = 0; i < WordIndex + 1; i++)
                {
                    if (OpenBrackets.Contains(this.WordsTable[i]))
                    {
                        result++;
                    }
                    if (result == BracketIndex)
                    {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        }
        public HackGame(List<string> WordsList, int TerminalLevel = 50, int ScienceLevel = 50, string Language = "ru", bool bSlowMode=false)
        {
            this.bSlowMode = bSlowMode;
            this.OffsetStart = rnd.Next(OffsetMin, OffsetMax);
            this.PasswordLength = 4 + 2 * (TerminalLevel / 25);
            this.WordCount = this.DudsAndPasswordCount(ScienceLevel, TerminalLevel);
            this.Language = Language;
            //this.BracketCount = 50 + LuckyLevel * 10;
            switch (this.Language)
            {
                case "ru":
                    this.TerminalWidth = 58;
                    break;
                case "en":
                    this.TerminalWidth = 54;
                    break;
                default:
                    this.TerminalWidth = 54;
                    break;
            }
            foreach (var item in WordsList.Where(item => item.Length == this.PasswordLength).ToList())
            {
                this.WordsDict.Add(item);
            }

            /* 
            string Stringsqwe = Newtonsoft.Json.JsonConvert.SerializeObject(Strings);
            System.IO.StreamWriter StringsSr = new System.IO.StreamWriter("Strings.json");
            StringsSr.Write(Stringsqwe);
            StringsSr.Close();
            */

            System.IO.StreamReader StringsSr = new System.IO.StreamReader("Strings.json");
            string StringsJson = StringsSr.ReadToEnd();
            StringsSr.Close();
            Strings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(StringsJson);

            int MaxLogWidth = 12;
            List<string> TempList = new List<string>();
            TempList.Add("EntryDenied");
            TempList.Add("Correct");
            TempList.Add("Allowance");
            TempList.Add("Replenished");
            TempList.Add("DudRemoved");
            TempList.Add("PleaseWait");
            TempList.Add("ExactMatch");
            TempList.Add("WhileSystem");
            TempList.Add("IsAccessed");
            TempList.Add("InitLockout");
            foreach (var item in TempList)
            {
                MaxLogWidth = Strings[Language][item].Length > MaxLogWidth ? Strings[Language][item].Length : MaxLogWidth;
            }
            this.TerminalWidth = 40 + MaxLogWidth;

            string options = "window: size =" + TerminalWidth.ToString() + "x" + TerminalHeight.ToString();
            Terminal.Open();
            Terminal.Set(options);
            Terminal.Color(this.BkColor);
            Terminal.Color(this.Color);
            Terminal.Clear();
            Terminal.Refresh();
        }
        private KeyValuePair<int, int> SearchSecretCombinations(int StartSearchWordIndex)
        {
            KeyValuePair<int, int> result = new KeyValuePair<int, int>(StartSearchWordIndex, StartSearchWordIndex);
            int UsedBracketIndex = UsedBracketsIndexFind(StartSearchWordIndex);
            foreach (var index in this.UsedBracketsIndex)
            {
                if (index == UsedBracketIndex)
                {
                    return result;
                }
            }

            int OpenBracketType = OpenBrackets.IndexOf(this.WordsTable[StartSearchWordIndex]);
            for (int i = StartSearchWordIndex; i < (((StartSearchWordIndex + DumpWidth) >= this.WordsTable.Count()) ? this.WordsTable.Count() : StartSearchWordIndex + DumpWidth); i++)
            {
                if (this.WordsTable[i].Length > 1)
                {
                    result = new KeyValuePair<int, int>(StartSearchWordIndex, StartSearchWordIndex);
                    break;
                }
                if (this.WordsTable[i].Equals(CloseBrackets[OpenBracketType].ToString()))
                {
                    result = new KeyValuePair<int, int>(StartSearchWordIndex, i);
                    break;
                }
            }

            if ((int)(this.WordsTableRanges[result.Key].Key / DumpWidth) != (int)(this.WordsTableRanges[result.Value].Key / DumpWidth))
            {
                result = new KeyValuePair<int, int>(StartSearchWordIndex, StartSearchWordIndex);
            }



            return result;
        }

        private void WordsTableRangesFill()
        {
            int i = 0;
            this.WordsTableRanges = new List<KeyValuePair<int, int>>();
            foreach (var Word in this.WordsTable)
            {
                this.WordsTableRanges.Add(new KeyValuePair<int, int>(i, i + Word.Length - 1));
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
            this.Password = this.WordsDict[rnd.Next(this.WordsDict.Count)];
            for (int i = 0; i < this.WordCount - 1; i++)//заполняет таблицу паролями
            {
                int j = rnd.Next(this.WordsDict.Count);
                do
                {
                    j = rnd.Next(this.WordsDict.Count);
                } while (this.WordsTable.Contains(this.WordsDict[j]) || WordBulls(this.Password, this.WordsDict[j]) < 1);

                this.WordsTable.Add(this.WordsDict[j]);
            }
            foreach (var Dud in this.WordsTable)
            {
                this.Duds.Add(Dud);
            }

            this.WordsTable.Insert(rnd.Next(this.WordsTable.Count + 1), this.Password);

            for (int i = this.WordCount - 1; i > 0; i--)
            {
                this.WordsTable.Insert(i, TrashChars[rnd.Next(TrashChars.Count())].ToString());
            }

            while (this.WordsTable.Count() + ((this.PasswordLength - 1) * this.WordCount) < DumpHeight * DumpWidth)
            {
                this.WordsTable.Insert(rnd.Next(this.WordsTable.Count() + 1), TrashChars[rnd.Next(TrashChars.Count())].ToString());//заполняет таблицу мусором
            }

            for (int i = 0; i < DumpHeight; i++)
            {
                this.HexAddresses.Add("0x" + (this.OffsetStart + OffsetInc * i).ToString("X"));
            }

            WordsTableRangesFill();
        }
        public int CheckWord()
        {
            KeyValuePair<int, int> CursorBlock = new KeyValuePair<int, int>(
                (int)(this.CursorFlat / DumpWidth) * DumpWidth,
                (int)(this.CursorFlat / DumpWidth) * DumpWidth + 1);
            this.Attempts--;

            if (OpenBrackets.Contains(this.WordsTable[CursorWordIndex]) && SearchSecretCombinations(CursorWordIndex).Key != SearchSecretCombinations(CursorWordIndex).Value)
            {
                string BracketCombination = "";

                for (int i = SearchSecretCombinations(CursorWordIndex).Key; i <= SearchSecretCombinations(CursorWordIndex).Value; i++)
                {
                    BracketCombination += this.WordsTable[i];
                    if (BracketCombination[BracketCombination.Length - 1].ToString() == "[" || BracketCombination[BracketCombination.Length - 1].ToString() == "]")
                    {
                        BracketCombination += BracketCombination[BracketCombination.Length - 1];
                    }
                }
                this.IOLog.Add(">" + BracketCombination);
                UsedBracketsIndex.Add(UsedBracketsIndexFind(CursorWordIndex));
                if (this.rnd.Next(100) < 50)
                {
                    this.Attempts = 4;

                    this.IOLog.Add(">" + this.Strings[this.Language]["Allowance"]);
                    this.IOLog.Add(">" + this.Strings[this.Language]["Replenished"]);
                    return -1;
                }
                else
                {
                    int DudToRemoveIndex = this.rnd.Next(this.Duds.Count);
                    int RemovedDudIndex = this.WordsTable.IndexOf(this.Duds[DudToRemoveIndex]);
                    this.Duds.RemoveAt(DudToRemoveIndex);
                    this.WordsTable[RemovedDudIndex] = ".";
                    for (int i = 0; i < PasswordLength - 1; i++)
                    {
                        this.WordsTable.Insert(RemovedDudIndex, ".");
                    }
                    this.WordsTableRangesFill();
                    this.CursorWordIndexMath();
                    this.Attempts++;
                    this.IOLog.Add(">" + this.Strings[this.Language]["DudRemoved"]);
                    return -2;
                }

            }
            else if (this.WordsTable[CursorWordIndex].Equals(this.Password))
            {
                this.IOLog.Add(">" + this.WordsTable[CursorWordIndex].ToUpper());
                this.IOLog.Add(">" + this.Strings[this.Language]["ExactMatch"]);
                this.IOLog.Add(">" + this.Strings[this.Language]["PleaseWait"]);
                this.IOLog.Add(">" + this.Strings[this.Language]["WhileSystem"]);
                this.IOLog.Add(">" + this.Strings[this.Language]["IsAccessed"]);
                this.bUnlocked = true;
                this.Attempts += 1;
                return this.PasswordLength;
            }
            else if (this.WordsTable[CursorWordIndex].Length == this.PasswordLength)
            {

                int Bulls = 0;
                for (int i = 0; i < this.Password.Length; i++)
                {
                    Bulls += this.Password[i] == this.WordsTable[CursorWordIndex][i] ? 1 : 0;//готово
                }
                this.IOLog.Add(">" + this.WordsTable[CursorWordIndex].ToUpper());
                this.IOLog.Add(">" + this.Strings[this.Language]["EntryDenied"]);
                this.IOLog.Add(">" + Bulls + "/" + this.PasswordLength + " " + this.Strings[this.Language]["Correct"]);
                if (this.Attempts < 1)
                    this.bLocked = true;
                return Bulls;
            }
            else
            {
                if (WordsTable[CursorWordIndex] == "]")
                {
                    this.IOLog.Add(">" + this.WordsTable[CursorWordIndex] + this.WordsTable[CursorWordIndex]);
                }
                else
                {
                    this.IOLog.Add(">" + this.WordsTable[CursorWordIndex]);
                }

                this.IOLog.Add(">" + this.Strings[this.Language]["EntryDenied"]);
                this.IOLog.Add(">" + this.WordBulls(this.Password, this.WordsTable[CursorWordIndex]) + "/" + this.PasswordLength + " " + this.Strings[this.Language]["Correct"]);
            }

            if (this.Attempts < 1)
            {
                this.IOLog.Add(">" + this.Strings[this.Language]["InitLockout"]);
            }
            if (this.Attempts < 1)
                this.bLocked = true;

            return 0;
        }
        //! (бонусная задача) Сделать вывод а-ля fallout, т.е. буквы выводятся по очереди (у меня весь кадр формируется целиком и выводится сразу). 
        private void UnlockScreen()
        {
            if (this.bUnlocked && !this.bUnlockScreen)
            {
                ShowGameField();
                Terminal.Delay(500);
                for (int frame = 0; frame < TerminalHeight; frame++)
                {
                    Terminal.Color(this.BkColor);
                    Terminal.Color(this.Color);
                    for (int y = 0; y < TerminalHeight; y++)
                    {
                        for (int x = 0; x < TerminalWidth; x++)
                        {
                            Terminal.Layer(0);
                            var foo = Terminal.Pick(x, y + 1);
                            Terminal.Layer(1);
                            Terminal.Put(x, y, foo);

                        }
                        Terminal.Delay(1);
                    }
                    Terminal.Layer(0);
                    Terminal.ClearArea(0, 0, TerminalWidth, TerminalHeight);
                    Terminal.Refresh();

                    for (int y = 0; y < TerminalHeight; y++)
                    {
                        for (int x = 0; x < TerminalWidth; x++)
                        {
                            Terminal.Layer(1);
                            var foo = Terminal.Pick(x, y);
                            Terminal.Layer(0);
                            Terminal.Put(x, y, foo);

                        }
                    }
                }
                Terminal.BkColor(this.BkColor);
                Terminal.Color(this.Color);
                Terminal.Clear();

                //Terminal.Print(0, 0, this.Strings[this.Language]["WelcomeToRobco"].ToUpper());
                RobCoPrintingString(0, 0, this.Strings[this.Language]["WelcomeToRobco"].ToUpper());
                Terminal.Refresh();
                Terminal.Print(0, 2, "> ");
                Terminal.Delay(150);
                Terminal.Refresh();
                //for (int x = 0; x < this.Strings[this.Language]["LogonAdmin"].Length; x++)
                //{
                UserPrintingString(2, 2, this.Strings[this.Language]["LogonAdmin"].ToUpper());
                //Terminal.Put(x, 2, this.Strings[this.Language]["LogonAdmin"].ToUpper()[x]);
                //Terminal.Refresh();
                //Terminal.Delay(rnd.Next(35, 88));
                //}
                Terminal.Print(0, 4, this.Strings[this.Language]["EnterPassword"].ToUpper());
                Terminal.Refresh();
                string pass = "";
                for (int x = 0; x < this.PasswordLength; x++) pass += "*";
                Terminal.Print(0, 6, "> ");
                Terminal.Refresh();
                Terminal.Delay(150);
                UserPrintingString(2, 6, pass.ToString());
                //for (int x = 0; x < pass.Length; x++)
                //{
                //    Terminal.Put(x, 6, pass[x]);
                //    Terminal.Refresh();
                //    Terminal.Delay(rnd.Next(35, 88));
                //}
                Terminal.Refresh();
                this.bUnlockScreen = true;
                return;
            }
            if (this.bUnlockScreen)
            {
                Terminal.Color(this.BkColor);
                Terminal.Color(this.Color);
                Terminal.Clear();
                Terminal.Print(0, 0, this.Strings[this.Language]["WelcomeToRobco"].ToUpper());
                Terminal.Print(0, 2, "> ");
                Terminal.Print(2, 2, this.Strings[this.Language]["LogonAdmin"].ToUpper());
                Terminal.Print(0, 4, this.Strings[this.Language]["EnterPassword"].ToUpper());
                string pass = "> "; for (int x = 0; x < this.PasswordLength; x++) pass += "*";
                Terminal.Print(0, 6, pass.ToUpper());
                Terminal.Refresh();
            }
        }
        private void LockScreen()
        {

            if (this.bLocked && !this.bBlockScreen)
            {
                ShowGameField();
                Terminal.Delay(500);
                for (int frame = 0; frame < TerminalHeight; frame++)
                {
                    Terminal.BkColor(this.BkColor);
                    Terminal.Color(this.Color);
                    for (int y = 0; y < TerminalHeight; y++)
                    {
                        for (int x = 0; x < TerminalWidth; x++)
                        {
                            Terminal.Layer(0);
                            var foo = Terminal.Pick(x, y + 1);
                            Terminal.Layer(1);
                            Terminal.Put(x, y, foo);

                        }
                        Terminal.Delay(1);
                    }
                    Terminal.Layer(0);
                    Terminal.ClearArea(0, 0, TerminalWidth, TerminalHeight);
                    Terminal.Refresh();

                    for (int y = 0; y < TerminalHeight; y++)
                    {
                        for (int x = 0; x < TerminalWidth; x++)
                        {
                            Terminal.Layer(1);
                            var foo = Terminal.Pick(x, y);
                            Terminal.Layer(0);
                            Terminal.Put(x, y, foo);

                        }
                    }
                }
                Terminal.BkColor(this.BkColor);
                Terminal.Color(this.Color);
                Terminal.Clear();
                RobCoPrintingString(TerminalWidth / 2 - this.Strings[this.Language]["TerminalLocked"].Length / 2, TerminalHeight / 2 - 1, this.Strings[this.Language]["TerminalLocked"].ToUpper());
                RobCoPrintingString(TerminalWidth / 2 - this.Strings[this.Language]["PleaseContactAnAdministrator"].Length / 2, TerminalHeight / 2 + 1, this.Strings[this.Language]["PleaseContactAnAdministrator"].ToUpper()); ;
                Terminal.Refresh();
                this.bBlockScreen = true;
                return;
            }
            if (this.bBlockScreen)
            {
                Terminal.BkColor(this.BkColor);
                Terminal.Color(this.Color);
                Terminal.Clear();
                Terminal.Print(TerminalWidth / 2 - this.Strings[this.Language]["TerminalLocked"].Length / 2, TerminalHeight / 2 - 1, this.Strings[this.Language]["TerminalLocked"].ToUpper());
                Terminal.Print(TerminalWidth / 2 - this.Strings[this.Language]["PleaseContactAnAdministrator"].Length / 2, TerminalHeight / 2 + 1, this.Strings[this.Language]["PleaseContactAnAdministrator"].ToUpper());
                Terminal.Refresh();
            }
        }
        private void ShowGameField()
        {

            Terminal.BkColor(this.BkColor);
            Terminal.Color(this.Color);
            Terminal.Clear();

            int CurrentWordIndex = 0;
            int i = 0;
            bool SecretCombinationsActive = false;
            KeyValuePair<int, int> SecretCombination = new KeyValuePair<int, int>();

            if (this.bAfterLaunchingScreen && bSlowMode)
            {

                string AttemptsCountSquares = "";
                for (int sq = 0; sq < this.Attempts; sq++)
                {
                    AttemptsCountSquares += " " + this.Strings[this.Language]["Square"];
                }
                this.RobCoPrintingString(0, 0, this.Strings[this.Language]["RobcoIndustries"].ToUpper());
                this.RobCoPrintingString(0, 1, this.Strings[this.Language]["EnterPassword"].ToUpper());
                this.RobCoPrintingString(0, 3, this.Attempts.ToString() + this.Strings[this.Language]["AttemptsLeft"].ToUpper().Substring(3) + AttemptsCountSquares);

                string[,] TempWordTable = new string[TerminalWidth, TerminalHeight];

                i = 0;
                foreach (var Address in HexAddresses)
                {
                    if (i < DumpHeight / 2)
                        Terminal.Print(0, (int)(i) + (TerminalHeight - DumpHeight / 2), Address.ToString());
                    else
                        Terminal.Print(20, (int)(i) + (TerminalHeight - DumpHeight / 2) - DumpHeight / 2, Address.ToString());

                        Terminal.Refresh();
                    i++;
                }
                i = 0;
                foreach (var Word in this.WordsTable)
                {
                    foreach (var Char in Word)
                    {
                        if ((int)(i / DumpWidth) < DumpHeight / 2)
                            Terminal.Print((int)(i % DumpWidth) + 7, (int)(i / DumpWidth) + (TerminalHeight - DumpHeight / 2), Char.ToString() == "]" || Char.ToString() == "[" ? Char.ToString() + Char.ToString() : Char.ToString().ToUpper());
                        else
                            Terminal.Print((int)(i % DumpWidth) + 20 + 7, (int)(i / DumpWidth) + (TerminalHeight - DumpHeight / 2) - DumpHeight / 2, Char.ToString() == "]" || Char.ToString() == "[" ? Char.ToString() + Char.ToString() : Char.ToString().ToUpper());
                        i++;
                    }
                    if (i%12==0)
                    {
                        Terminal.Refresh();
                    }
                    
                }
                i = 0;



                Terminal.Refresh();
                this.bAfterLaunchingScreen = false;
            }
            else
            {
                foreach (var Word in this.WordsTable)
                {
                    if (OpenBrackets.Contains(Word) && !(SecretCombinationsActive))
                    {
                        SecretCombination = SearchSecretCombinations(CurrentWordIndex);
                        SecretCombinationsActive = true;
                    }
                    if (this.CursorWordIndex == CurrentWordIndex || (this.CursorWordIndex == SecretCombination.Key && SecretCombination.Key != SecretCombination.Value && (CurrentWordIndex >= SecretCombination.Key && CurrentWordIndex <= SecretCombination.Value)))
                    {
                        Terminal.Color(this.BkColor);
                        Terminal.BkColor(this.Color);
                    }
                    else
                    {
                        SecretCombinationsActive = false;
                        Terminal.BkColor(this.BkColor);
                        Terminal.Color(this.Color);
                    }
                    foreach (var Char in Word)
                    {
                        if ((int)(i / DumpWidth) < DumpHeight / 2)
                            Terminal.Print((int)(i % DumpWidth) + 7, (int)(i / DumpWidth) + (TerminalHeight - DumpHeight / 2), Char.ToString() == "]" || Char.ToString() == "[" ? Char.ToString() + Char.ToString() : Char.ToString().ToUpper());
                        else
                            Terminal.Print((int)(i % DumpWidth) + 20 + 7, (int)(i / DumpWidth) + (TerminalHeight - DumpHeight / 2) - DumpHeight / 2, Char.ToString() == "]" || Char.ToString() == "[" ? Char.ToString() + Char.ToString() : Char.ToString().ToUpper());
                        i++;
                    }

                    CurrentWordIndex++;
                }
                i = 0;
                Terminal.BkColor(this.BkColor);
                Terminal.Color(this.Color);
                foreach (var Address in HexAddresses)
                {
                    if (i < DumpHeight / 2)
                        Terminal.Print(0, (int)(i) + (TerminalHeight - DumpHeight / 2), Address.ToString());
                    else
                        Terminal.Print(20, (int)(i) + (TerminalHeight - DumpHeight / 2) - DumpHeight / 2, Address.ToString());
                    i++;
                }

                string AttemptsCountSquares = "";
                for (int sq = 0; sq < this.Attempts; sq++)
                {
                    AttemptsCountSquares += " " + this.Strings[this.Language]["Square"];
                }


                if (this.Attempts > 1)
                {
                    Terminal.Print(0, 0, this.Strings[this.Language]["RobcoIndustries"].ToUpper());
                    Terminal.Print(0, 1, this.Strings[this.Language]["EnterPassword"].ToUpper());
                    Terminal.Print(0, 3, this.Strings[this.Language]["AttemptsLeft"].ToUpper() + AttemptsCountSquares, Attempts);
                }
                if (this.Attempts <= 1 && !this.bUnlocked)
                {
                    Terminal.Print(0, 0, this.Strings[this.Language]["RobcoIndustries"].ToUpper());
                    //if (System.DateTime.Now.Millisecond<500)
                    //{
                        Terminal.Print(0, 1, this.Strings[this.Language]["LockoutImminent"].ToUpper());
                    //}
                    Terminal.Print(0, 3, this.Strings[this.Language]["AttemptsLeft"].ToUpper() + AttemptsCountSquares, Attempts);

                }
                if (this.Attempts >= 1 && this.bUnlocked)
                {
                    Terminal.Print(0, 0, this.Strings[this.Language]["RobcoIndustries"].ToUpper());
                    Terminal.Print(0, 3, this.Strings[this.Language]["AttemptsLeft"].ToUpper() + AttemptsCountSquares, Attempts);
                }


                if (this.WordsTable[CursorWordIndex] == "]")
                {
                    Terminal.Print(DumpWidth * 2 + 12 + 4, (TerminalHeight - 1), ">]]");
                    //Terminal.Print(DumpWidth * 2 + 12 + 4, (TerminalHeight - 1), ">");
                    //Terminal.Refresh();
                    //this.UserPrintingString(DumpWidth * 2 + 12 + 4+1, (TerminalHeight - 1), "]]");
                }
                if (OpenBrackets.Contains(this.WordsTable[this.CursorWordIndex]))
                {
                    Terminal.Print(DumpWidth * 2 + 12 + 4, (TerminalHeight - 1), ">");
                    //string SecretCombinationStr = "";
                    for (int c = SearchSecretCombinations(this.CursorWordIndex).Key; c <= SearchSecretCombinations(this.CursorWordIndex).Value; c++)
                    {
                        //SecretCombinationStr += this.WordsTable[c][0].ToString() == "]" || this.WordsTable[c][0].ToString() == "[" ? this.WordsTable[c].ToString() + this.WordsTable[c].ToString() : this.WordsTable[c].ToString().ToUpper();
                        Terminal.Print(DumpWidth * 2 + 12 + 4 + 1 + c - SearchSecretCombinations(this.CursorWordIndex).Key, (TerminalHeight - 1), this.WordsTable[c][0].ToString() == "]" || this.WordsTable[c][0].ToString() == "[" ? this.WordsTable[c].ToString() + this.WordsTable[c].ToString() : this.WordsTable[c].ToString().ToUpper());//квадратные скобки выводить парами
                                                                                                                                                                                                                                                                                                                                                //Terminal.Delay(rnd.Next(35, 88));
                                                                                                                                                                                                                                                                                                                                                //Terminal.Refresh();
                                                                                                                                                                                                                                                                                                                                                //! Побуквенный вывод в "поле ввода" при выборе слова из кучи. 
                    }
                    //this.UserPrintingString(DumpWidth * 2 + 12 + 4 + 1 - SearchSecretCombinations(this.CursorWordIndex).Key, (TerminalHeight - 1), SecretCombinationStr);//квадратные скобки выводить парами


                }
                else
                {
                    Terminal.Print(DumpWidth * 2 + 12 + 4, (TerminalHeight - 1), ">" + this.WordsTable[this.CursorWordIndex].ToUpper());
                    //Terminal.Print(DumpWidth * 2 + 12 + 4, (TerminalHeight - 1), ">");
                    //Terminal.Refresh();
                    //this.UserPrintingString(DumpWidth * 2 + 12 + 4+1, (TerminalHeight - 1), this.WordsTable[this.CursorWordIndex].ToUpper());
                }


                for (int d = this.IOLog.Count() - DumpHeight / 2 + 2 < 0 ? 0 : this.IOLog.Count() - DumpHeight / 2 + 2; d < this.IOLog.Count(); d++)
                {
                    Terminal.Print(DumpWidth * 2 + 12 + 4, TerminalHeight - this.IOLog.Count() + d - 2, this.IOLog[d]);
                    //this.UserPrintingString(DumpWidth * 2 + 12 + 4, TerminalHeight - this.IOLog.Count() + d - 2, this.IOLog[d]);
                }

                Terminal.Refresh();
            }
        }
        private void PrintingRow(int x, int y, string str, int minWait, int maxWait)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i].ToString().Equals("\n"))
                {
                    y++;
                    x = -i - 1;
                    continue;
                }
                Terminal.Print(x + i, y, this.Strings[this.Language]["Cursor"]);
                Terminal.Refresh();
                Terminal.Delay(this.rnd.Next(minWait, maxWait));
                Terminal.Print(x + i, y, str[i].ToString());
                Terminal.Refresh();
            }
        }
        private void RobCoPrintingString(int x, int y, string str)
        {
            PrintingRow(x, y, str, 1, 3);
        }
        private void UserPrintingString(int x, int y, string str)
        {
            PrintingRow(x, y, str, 16, 32);
        }
        private void EnteringScreen()
        {
            if (this.bLaunching)
            {
                if (bSlowMode)
                {
                    RobCoPrintingString(0, 0, this.Strings[this.Language]["WelcomeToRobco"].ToUpper());
                    Terminal.Delay(150);
                    Terminal.Put(0, 2, '>'); Terminal.Refresh(); Terminal.Delay(150);
                    UserPrintingString(1, 2, this.Strings[this.Language]["SetTerminalInquire"].ToUpper());
                    RobCoPrintingString(0, 4, this.Strings[this.Language]["RITV300"].ToUpper());
                    Terminal.Put(0, 6, '>'); Terminal.Refresh(); Terminal.Delay(150);
                    UserPrintingString(1, 6, this.Strings[this.Language]["SetFileProtectionOwner"].ToUpper());
                    Terminal.Put(0, 7, '>'); Terminal.Refresh(); Terminal.Delay(150);
                    UserPrintingString(1, 7, this.Strings[this.Language]["SetHaltRestart"].ToUpper());
                    RobCoPrintingString(0, 9, this.Strings[this.Language]["BootAgentInfo"]);
                    Terminal.Put(0, 17, '>'); Terminal.Refresh(); Terminal.Delay(150);
                    UserPrintingString(1, 17, this.Strings[this.Language]["RunDebugAccounts"].ToUpper());

                    Terminal.BkColor(this.BkColor);
                    Terminal.Color(this.Color);
                    Terminal.Delay(1000);
                    Terminal.Clear();
                    Terminal.Refresh();
                }
                else
                {
                    Terminal.Print(0, 0, this.Strings[this.Language]["WelcomeToRobco"].ToUpper());
                    Terminal.Put(0, 2, '>');
                    Terminal.Print(1, 2, this.Strings[this.Language]["SetTerminalInquire"].ToUpper());
                    Terminal.Print(0, 4, this.Strings[this.Language]["RITV300"].ToUpper());
                    Terminal.Put(0, 6, '>');
                    Terminal.Print(1, 6, this.Strings[this.Language]["SetFileProtectionOwner"].ToUpper());
                    Terminal.Put(0, 7, '>');
                    Terminal.Print(1, 7, this.Strings[this.Language]["SetHaltRestart"].ToUpper());
                    Terminal.Print(0, 9, this.Strings[this.Language]["BootAgentInfo"]);
                    Terminal.Put(0, 17, '>');
                    Terminal.Print(1, 17, this.Strings[this.Language]["RunDebugAccounts"].ToUpper());


                    Terminal.Refresh();
                    Terminal.BkColor(this.BkColor);
                    Terminal.Color(this.Color);
                    Terminal.Delay(1000);
                    Terminal.Clear();
                }

            }
        }

        public void ShowFrame()
        {
            Terminal.BkColor(this.BkColor);
            Terminal.Color(this.Color);
            Terminal.Clear();
            if (this.bUnlocked)
            {
                this.UnlockScreen();
            }
            else if (this.bLocked)
            {
                this.LockScreen();
            }
            else if (this.bLaunching)
            {
                EnteringScreen();
                
                this.bLaunching = false;
            }
            else
            {
                ShowGameField();
            }

        }
        private static int FlatteringCursor(System.Drawing.Point CursorPoint)
        {
            return (DumpWidth * CursorPoint.Y + CursorPoint.X);
        }
        public void MoveCursor(System.Drawing.Point MoveVector)
        {
            if (MoveVector.X < 0)
            {
                this.Cursor.X += -1 * MoveVector.X * ((this.WordsTableRanges[this.CursorWordIndex].Key - 1) - this.CursorFlat);
            }
            if (MoveVector.X > 0)
            {
                this.Cursor.X += MoveVector.X * ((this.WordsTableRanges[this.CursorWordIndex].Value + 1) - this.CursorFlat);
            }

            if (MoveVector.Y > 0 && this.Cursor.Y == DumpHeight / 2 - 1)//down
            {

            }
            else if (MoveVector.Y < 0 && this.Cursor.Y == DumpHeight / 2)//up
            {

            }
            else
            {
                this.Cursor.Y += MoveVector.Y;
            }

            if (this.Cursor.X >= DumpWidth && this.Cursor.Y < DumpHeight / 2)
            {
                this.Cursor.Y = this.Cursor.Y + DumpHeight / 2;
                this.Cursor.X = 0;
            }
            else
            if (this.Cursor.X < 0 && this.Cursor.Y >= DumpHeight / 2)
            {
                this.Cursor.Y = this.Cursor.Y - DumpHeight / 2;
                this.Cursor.X = DumpHeight;
            }
            this.Cursor.Y = this.Cursor.Y < 0 ? 0 : this.Cursor.Y;
            this.Cursor.Y = this.Cursor.Y >= DumpHeight ? DumpHeight - 1 : this.Cursor.Y;

            this.Cursor.X = this.Cursor.X < 0 ? 0 : this.Cursor.X;
            this.Cursor.X = this.Cursor.X >= DumpWidth ? DumpWidth - 1 : this.Cursor.X;
            this.CursorFlat = FlatteringCursor(this.Cursor);
            CursorWordIndexMath();
        }
        public void MoveToCursor(System.Drawing.Point MovePoint)
        {
            if (MovePoint.X < 7 || MovePoint.X >= 39 || MovePoint.Y < (TerminalHeight - DumpHeight / 2)) return;
            if (MovePoint.X >= 7 && MovePoint.X < 19)
            {
                this.Cursor.X = MovePoint.X - 7;
                this.Cursor.Y = MovePoint.Y - (TerminalHeight - DumpHeight / 2);
            }
            if (MovePoint.X > 26 && MovePoint.X < 39)
            {
                this.Cursor.X = MovePoint.X - 27;
                this.Cursor.Y = MovePoint.Y - (TerminalHeight - DumpHeight / 2) + DumpHeight / 2;
            }

            this.CursorFlat = FlatteringCursor(Cursor);
            CursorWordIndexMath();
        }


    }

}
