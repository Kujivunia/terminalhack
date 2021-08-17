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

        private static readonly int DumpHeight = 32;
        private static readonly int DumpWidth = 12;

        private static readonly int OffsetInc = DumpWidth;
        private static readonly int OffsetMin = 4096;
        private static readonly int OffsetMax = 65150;
        private int OffsetStart = 0;

        private static readonly int TerminalHeight = 21;//25-21
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
        private bool BlockScreen = false;
        private bool UnlockScreen = false;
        private bool Unlock = false;
        private System.Drawing.Color Color = System.Drawing.Color.LightGreen;
        private System.Drawing.Color BkColor = System.Drawing.Color.DarkGreen;
        private Dictionary<string, Dictionary<string, string>> Strings = new Dictionary<string, Dictionary<string, string>>();
        private string Language = "ru";
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
                return (int)System.Math.Round(15d / (100 - TerminalLvl) * (100 - ScienceLvl)) + 5;
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
        public HackGame(List<string> WordsList, int TerminalLevel = 50, int ScienceLevel = 50,string Language = "ru")
        {
            this.OffsetStart = rnd.Next(OffsetMin, OffsetMax);
            this.PasswordLength = 4 + 2 * (TerminalLevel / 25);
            this.WordCount = this.DudsAndPasswordCount(ScienceLevel, TerminalLevel);
            this.Language = Language;
            //this.BracketCount = 50 + LuckyLevel * 10;
            switch (this.Language)
            {
                case "ru": this.TerminalWidth = 58;
                    break;
                case "en":
                    this.TerminalWidth = 54;
                    break;
                default: this.TerminalWidth = 54;
                    break;
            }
            foreach (var item in WordsList.Where(item => item.Length == this.PasswordLength).ToList())
            {
                this.WordsDict.Add(item);
            }

            string options = "window: size =" + TerminalWidth.ToString() + "x" + TerminalHeight.ToString();

            Strings.Add("ru", new Dictionary<string, string>());
            Strings.Add("en", new Dictionary<string, string>());

            Strings["ru"].Add("EnterPassword", "введите пароль");
            Strings["en"].Add("EnterPassword", "enter password now");

            Strings["ru"].Add("WelcomeToRobco", "welcome to robco industries (tm) termlink");
            Strings["en"].Add("WelcomeToRobco", "welcome to robco industries (tm) termlink");

            Strings["ru"].Add("LogonAdmin", "> logon admin");
            Strings["en"].Add("LogonAdmin", "> logon admin");

            Strings["ru"].Add("EntryDenied", "Отказ в доступе.");
            Strings["en"].Add("EntryDenied", "Entry denied.");

            Strings["ru"].Add("Correct", "правильно.");
            Strings["en"].Add("Correct", "correct.");

            Strings["ru"].Add("Allowance", "Пользование");
            Strings["en"].Add("Allowance", "Allowance");

            Strings["ru"].Add("Replenished", "вновь разрешено.");
            Strings["en"].Add("Replenished", "replenished.");

            Strings["ru"].Add("DudRemoved", "Заглушка удалена.");
            Strings["en"].Add("DudRemoved", "Dud removed.");

            Strings["ru"].Add("PleaseWait", "Пожалуйста,");
            Strings["en"].Add("PleaseWait", "Please wait");

            Strings["ru"].Add("ExactMatch", "Точно!");
            Strings["en"].Add("ExactMatch", "Exact match!");

            Strings["ru"].Add("WhileSystem", "подождите");
            Strings["en"].Add("WhileSystem", "while system");

            Strings["ru"].Add("IsAccessed", "входа в систему.");
            Strings["en"].Add("IsAccessed", "is accessed.");

            Strings["ru"].Add("InitLockout", "Блокировка начата.");
            Strings["en"].Add("InitLockout", "Init lockout.");

            Strings["ru"].Add("TerminalLocked", "терминал заблокирован");
            Strings["en"].Add("TerminalLocked", "terminal locked");

            Strings["ru"].Add("PleaseContactAnAdministrator", "пожалуйста, свяжитесь с администратором");
            Strings["en"].Add("PleaseContactAnAdministrator", "please, contact an administrator");


            Strings["ru"].Add("AttemptsLeft", "robco industries (tm) termlink protocol\nвведите пароль\n\n{0} попытки осталось:");
            Strings["en"].Add("AttemptsLeft", "robco industries (tm) termlink protocol\nenter password now\n\n{0} attempt(s) left:");

            Strings["ru"].Add("LockoutImminent", "robco industries (tm) termlink protocol\n!!! предупреждение: терминал может быть заблокирован !!!\n\n{0} попытки осталось:");
            Strings["en"].Add("LockoutImminent", "robco industries (tm) termlink protocol\n!!! warning: lockout imminent !!!\n\n{0} attempt(s) left:");



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
        public bool TerminalBlocked()
        {
            if (this.Attempts < 1)
                return true;
            else
                return false;
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
                this.Unlock = true;
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

            return 0;
        }
        //! (бонусная задача) Сделать вывод а-ля fallout, т.е. буквы выводятся по очереди (у меня весь кадр формируется целиком и выводится сразу). 

        public void ShowFrame()
        {
            //////////////////////////////////////////////////////////////
            if (this.Unlock && !this.UnlockScreen)
            {
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

                Terminal.Print(0, 0, this.Strings[this.Language]["WelcomeToRobco"].ToUpper());
                Terminal.Refresh();
                for (int x = 0; x < this.Strings[this.Language]["LogonAdmin"].Length; x++)
                {
                    Terminal.Put(x, 2, this.Strings[this.Language]["LogonAdmin"].ToUpper()[x]);
                    Terminal.Refresh();
                    Terminal.Delay(rnd.Next(35, 88));
                }
                Terminal.Print(0, 4, this.Strings[this.Language]["EnterPassword"].ToUpper());
                Terminal.Refresh();
                string pass = "> "; for (int x = 0; x < this.PasswordLength; x++) pass += "*";
                for (int x = 0; x < pass.Length; x++)
                {
                    Terminal.Put(x, 6, pass[x]);
                    Terminal.Refresh();
                    Terminal.Delay(rnd.Next(35, 88));
                }
                Terminal.Refresh();
                this.UnlockScreen = true;
                return;
            }
            if (this.UnlockScreen)
            {
                Terminal.Color(this.BkColor);
                Terminal.Color(this.Color);
                Terminal.Clear();
                Terminal.Print(0, 0, this.Strings[this.Language]["WelcomeToRobco"].ToUpper());
                Terminal.Print(0, 2, this.Strings[this.Language]["LogonAdmin"].ToUpper());
                Terminal.Print(0, 4, this.Strings[this.Language]["EnterPassword"].ToUpper());
                string pass = "> "; for (int x = 0; x < this.PasswordLength; x++) pass += "*";
                Terminal.Print(0, 6, pass.ToUpper());
                Terminal.Refresh();
                return;
            }
            ////////////////////////////////////////////////////////
            if (TerminalBlocked() && !this.BlockScreen)
            {
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
                Terminal.Print(TerminalWidth / 2 - this.Strings[this.Language]["TerminalLocked"].Length / 2, TerminalHeight / 2 - 1, this.Strings[this.Language]["TerminalLocked"].ToUpper());
                Terminal.Print(TerminalWidth / 2 - this.Strings[this.Language]["PleaseContactAnAdministrator"].Length / 2, TerminalHeight / 2 + 1, this.Strings[this.Language]["PleaseContactAnAdministrator"].ToUpper());
                Terminal.Refresh();
                this.BlockScreen = true;
                return;
            }
            if (this.BlockScreen)
            {
                Terminal.BkColor(this.BkColor);
                Terminal.Color(this.Color);
                Terminal.Clear();
                Terminal.Print(TerminalWidth / 2 - this.Strings[this.Language]["TerminalLocked"].Length / 2, TerminalHeight / 2 - 1, this.Strings[this.Language]["TerminalLocked"].ToUpper());
                Terminal.Print(TerminalWidth / 2 - this.Strings[this.Language]["PleaseContactAnAdministrator"].Length / 2, TerminalHeight / 2 + 1, this.Strings[this.Language]["PleaseContactAnAdministrator"].ToUpper());
                Terminal.Refresh();
                return;
            }
            /////////////////////////////////////////////
            Terminal.BkColor(this.BkColor);
            Terminal.Color(this.Color);
            Terminal.Clear();
            int i = 0;
            int CurrentWordIndex = 0;
            bool SecretCombinationsActive = false;
            KeyValuePair<int, int> SecretCombination = new KeyValuePair<int, int>();
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
                AttemptsCountSquares += " ▚[+]▞";
            }

            if (this.Attempts == 1) 
                Terminal.Print(0, 0, this.Strings[this.Language]["LockoutImminent"].ToUpper() + AttemptsCountSquares, Attempts);
            else
                Terminal.Print(0, 0, this.Strings[this.Language]["AttemptsLeft"].ToUpper() + AttemptsCountSquares, Attempts);

            if (this.WordsTable[CursorWordIndex] == "]")
            {
                Terminal.Print(DumpWidth * 2 + 12 + 4, (TerminalHeight - 1), ">]]");
            }
            if (OpenBrackets.Contains(this.WordsTable[this.CursorWordIndex]))
            {
                Terminal.Print(DumpWidth * 2 + 12 + 4, (TerminalHeight - 1), ">");
                for (int c = SearchSecretCombinations(this.CursorWordIndex).Key; c <= SearchSecretCombinations(this.CursorWordIndex).Value; c++)
                {
                    Terminal.Print(DumpWidth * 2 + 12 + 4 + 1 + c - SearchSecretCombinations(this.CursorWordIndex).Key, (TerminalHeight - 1), this.WordsTable[c][0].ToString() == "]" || this.WordsTable[c][0].ToString() == "[" ? this.WordsTable[c].ToString() + this.WordsTable[c].ToString() : this.WordsTable[c].ToString().ToUpper());//квадратные скобки выводить парами
                    //Terminal.Delay(rnd.Next(35, 88));
                    //Terminal.Refresh();
                    //! Побуквенный вывод в "поле ввода" при выборе слова из кучи. 
                }

            }
            else
            {
                Terminal.Print(DumpWidth * 2 + 12 + 4, (TerminalHeight - 1), ">" + this.WordsTable[this.CursorWordIndex].ToUpper());
            }


            for (int d = this.IOLog.Count() - DumpHeight / 2 + 2 < 0 ? 0 : this.IOLog.Count() - DumpHeight / 2 + 2; d < this.IOLog.Count(); d++)
            {
                Terminal.Print(DumpWidth * 2 + 12 + 4, TerminalHeight - this.IOLog.Count() + d - 2, this.IOLog[d]);
            }

            Terminal.Refresh();


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

            if (MoveVector.Y > 0 && this.Cursor.Y == DumpHeight / 2-1)//down
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
            //this.Cursor.X = Tor(0, DumpWidth - 1, this.Cursor.X);
            this.Cursor.Y = this.Cursor.Y < 0 ? 0 : this.Cursor.Y;
            this.Cursor.Y = this.Cursor.Y >= DumpHeight ? DumpHeight - 1 : this.Cursor.Y;

            this.Cursor.X = this.Cursor.X < 0 ? 0 : this.Cursor.X;
            this.Cursor.X = this.Cursor.X >= DumpWidth ? DumpWidth - 1 : this.Cursor.X;
            /*
            //this.Cursor.X += MoveVector.X * this.WordsTable[this.CursorWordIndex].Length;
            if (MoveVector.X > 0)
            {
                //this.Cursor.X = this.WordsTableRanges[this.CursorWordIndex].Value % DumpWidth + 1;
                if ((this.WordsTableRanges[this.CursorWordIndex].Value + 1) / DumpWidth > this.CursorWordIndex / DumpWidth)
                {
                    this.Cursor.X = this.CursorWordIndex / DumpWidth + 1;
                }
                else
                {
                    this.Cursor.X = this.WordsTableRanges[this.CursorWordIndex].Value % DumpWidth + 1;
                }

                //this.Cursor.Y = (this.WordsTableRanges[this.CursorWordIndex].Value+1) / DumpWidth;
            }
            if (MoveVector.X < 0)
            {
                if ((this.WordsTableRanges[this.CursorWordIndex].Key - 1) / DumpWidth < this.CursorWordIndex / DumpWidth)
                {
                    this.Cursor.X = this.CursorWordIndex / DumpWidth - 1;
                }
                else
                {
                    this.Cursor.X = this.WordsTableRanges[this.CursorWordIndex].Value % DumpWidth - 1;
                }
                //this.Cursor.X = this.WordsTableRanges[this.CursorWordIndex].Key % 12 - 1;
                //this.Cursor.Y = (this.WordsTableRanges[this.CursorWordIndex].Key - 1) / DumpWidth;
            }

            this.Cursor.Y += MoveVector.Y;//ДОДЕЛАТЬ СИСТЕМУ КУРСОРА ГОВНА

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
            //this.Cursor.X = Tor(0, DumpWidth - 1, this.Cursor.X);
            this.Cursor.Y = this.Cursor.Y < 0 ? 0 : this.Cursor.Y;
            this.Cursor.Y = this.Cursor.Y >= DumpHeight ? DumpHeight - 1 : this.Cursor.Y;

            this.Cursor.X = this.Cursor.X < 0 ? 0 : this.Cursor.X;
            this.Cursor.X = this.Cursor.X >= DumpWidth ? DumpWidth - 1 : this.Cursor.X;
            */
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
