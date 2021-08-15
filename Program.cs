﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BearLib;
using Newtonsoft.Json;

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

        private static readonly int TerminalHeight = 21;//25
        private static readonly int TerminalWidth = 64;//80

        private int Attempts = 4;

        private static readonly string TrashChars = "!\"#$%&\'()*+/:;<=>?@[\\]^_{|}";
        private static readonly string Brackets = "<>[]{}()";
        private static readonly string OpenBrackets = "<[{(";
        private static readonly string CloseBrackets = ">]})";
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
        private List<int> UsedBracketsIndex = new List<int>();
        private string Password;
        private List<string> Duds = new List<string>();
        private bool BlockScreen = false;
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
            if (foo.Length != bar.Length) return -1;
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
        //! Доработать систему генерации игрового поля. (сейчас есть ошибки генерации, и параметры генерации вроде слишком не такие, как в играх беседки). 
        public void GenerateWordsTable()
        {
            this.Password = this.WordsDict[rnd.Next(this.WordsDict.Count)];
            for (int i = 0; i < this.WordCount; i++)//заполняет таблицу паролями
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

            for (int i = 0; i < this.WordCount - 1; i++)
            {
                this.WordsTable.Insert(rnd.Next(this.WordsTable.Count() + 1), TrashChars[rnd.Next(TrashChars.Count())].ToString());
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
        //! Проверки парола. Нужно прикрутить определение числа совпадающих букв, это просто. 
        //! Удаление скобочных комбинаций. 
        //! Активация скобочных комбинаций. Удаление заглушек
        //! Вывода истории ввода -- пока не делал, но уже примерно знаю, как это сделать. 
        public int CheckWord()
        {
            KeyValuePair<int, int> CursorBlock = new KeyValuePair<int, int>(
                (int)(this.CursorFlat / DumpWidth) * DumpWidth,
                (int)(this.CursorFlat / DumpWidth) * DumpWidth + 1);
            this.Attempts--;
            if (OpenBrackets.Contains(this.WordsTable[CursorWordIndex]) && SearchSecretCombinations(CursorWordIndex).Key != SearchSecretCombinations(CursorWordIndex).Value)
            {
                UsedBracketsIndex.Add(UsedBracketsIndexFind(CursorWordIndex));
                if (this.rnd.Next(100) < 50)
                {
                    this.Attempts = 4;
                    return -1;
                }
                else
                {
                    
                    int DudToRemoveIndex = this.rnd.Next(this.Duds.Count);
                    int RemovedDudIndex = this.WordsTable.IndexOf(this.Duds[DudToRemoveIndex]);
                    this.Duds.RemoveAt(DudToRemoveIndex);
                    this.WordsTable[RemovedDudIndex] = ".";
                    for (int i = 0; i < PasswordLength-1; i++)
                    {
                        this.WordsTable.Insert(RemovedDudIndex,".");
                    }
                    this.WordsTableRangesFill();
                    this.Attempts++;
                    return -2;
                    //удалить заглушку
                }

            }
            else
            {
                if (this.WordsTable[CursorWordIndex].Equals(this.Password))
                {
                    return this.PasswordLength;
                }
                else if (this.WordsTable[CursorWordIndex].Length == this.PasswordLength)
                {

                    int Bulls = 0;
                    for (int i = 0; i < this.Password.Length; i++)
                    {
                        Bulls += this.Password[i] == this.WordsTable[CursorWordIndex][i] ? 1 : 0;//готово
                    }

                    return Bulls;
                }
            }

            return 0;
        }
        //! (бонусная задача) Сделать вывод а-ля fallout, т.е. буквы выводятся по очереди (у меня весь кадр формируется целиком и выводится сразу). 

        public void ShowFrame()
        {


            if (TerminalBlocked() && !this.BlockScreen)
            {
                for (int frame = 0; frame < TerminalHeight; frame++)
                {
                    Terminal.BkColor(System.Drawing.Color.DarkGreen);
                    Terminal.Color(System.Drawing.Color.LightGreen);
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
                Terminal.BkColor(System.Drawing.Color.DarkGreen);
                Terminal.Color(System.Drawing.Color.LightGreen);
                Terminal.Clear();
                Terminal.Print(TerminalWidth / 2 - "терминал заблокирован".Length / 2, TerminalHeight / 2 - 1, "терминал заблокирован".ToUpper());
                Terminal.Print(TerminalWidth / 2 - "пожалуйста, свяжитесь с администратором".Length / 2, TerminalHeight / 2 + 1, "пожалуйста, свяжитесь с администратором".ToUpper());
                Terminal.Refresh();
                this.BlockScreen = true;
                return;
            }
            if (this.BlockScreen)
            {
                Terminal.BkColor(System.Drawing.Color.DarkGreen);
                Terminal.Color(System.Drawing.Color.LightGreen);
                Terminal.Clear();
                Terminal.Print(TerminalWidth / 2 - "терминал заблокирован".Length / 2, TerminalHeight / 2 - 1, "терминал заблокирован".ToUpper());
                Terminal.Print(TerminalWidth / 2 - "пожалуйста, свяжитесь с администратором".Length / 2, TerminalHeight / 2 + 1, "пожалуйста, свяжитесь с администратором".ToUpper());
                Terminal.Refresh();
                return;
            }
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
                    Terminal.Color(System.Drawing.Color.DarkGreen);
                    Terminal.BkColor(System.Drawing.Color.LightGreen);
                }
                else
                {
                    SecretCombinationsActive = false;
                    Terminal.BkColor(System.Drawing.Color.DarkGreen);
                    Terminal.Color(System.Drawing.Color.LightGreen);
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
            Terminal.BkColor(System.Drawing.Color.DarkGreen);
            Terminal.Color(System.Drawing.Color.LightGreen);
            foreach (var Address in HexAddresses)
            {
                if (i < DumpHeight / 2)
                    Terminal.Print(0, (int)(i) + (TerminalHeight - DumpHeight / 2), Address.ToString());
                else
                    Terminal.Print(20, (int)(i) + (TerminalHeight - DumpHeight / 2) - DumpHeight / 2, Address.ToString());
                i++;
            }
            if (this.Attempts == 4) Terminal.Print(0, 0, "robco industries (tm) termlink protocol\nвведите пароль\n\n4 попытки осталось: x x x x".ToUpper());
            if (this.Attempts == 3) Terminal.Print(0, 0, "robco industries (tm) termlink protocol\nвведите пароль\n\n3 попытки осталось: x x x".ToUpper());
            if (this.Attempts == 2) Terminal.Print(0, 0, "robco industries (tm) termlink protocol\nвведите пароль\n\n2 попытки осталось: x x".ToUpper());
            if (this.Attempts == 1) Terminal.Print(0, 0, "robco industries (tm) termlink protocol\nвведите пароль\n\n1 попытка осталась: x".ToUpper());
            if (this.Attempts < 1) Terminal.Print(0, 0, "robco industries (tm) termlink protocol\nвведите пароль\n\n0 попыток осталось:".ToUpper());
            Terminal.Print(40, (TerminalHeight - 1), ">" + this.Password.ToUpper());

            Terminal.Refresh();


        }
        private static int FlatteringCursor(System.Drawing.Point CursorPoint)
        {
            return (DumpWidth * CursorPoint.Y + CursorPoint.X);
        }
        public void MoveCursor(System.Drawing.Point MoveVector)
        {
            this.Cursor.X += MoveVector.X * this.WordsTable[this.CursorWordIndex].Length;
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
            this.CursorFlat = FlatteringCursor(this.Cursor);
            CursorWordIndexMath();
        }
        public void MoveToCursor(System.Drawing.Point MovePoint)
        {
            this.Cursor.X = MovePoint.X;
            this.Cursor.Y = MovePoint.Y - (TerminalHeight - DumpHeight);//! Перевод координат окна в координаты Dump
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
            //Terminal.Read();
            Terminal.Set("input.filter = [keyboard, mouse]");
            while (Terminal.HasInput() ? Terminal.Read() != Terminal.TK_CLOSE : true)
            {
                int dx = 0;
                int dy = 0;
                int mx = -1;
                int my = -1;
                //if (Terminal.HasInput())
                //! (бонусная задача) Добавить звуки. 
                {//! Сделать нормальное приложение вокруг механики игры. А именно, починить управление (сейчас работает лишь частично). 
                    int TK = Terminal.Read(); //! Управление мышкой.  
                    //Terminal.Set("window: title=" + "'" + Terminal.Peek().ToString() + "'");
                    dx = TK == Terminal.TK_LEFT ? -1 : TK == Terminal.TK_RIGHT ? 1 : 0;
                    dy = TK == Terminal.TK_UP ? -1 : TK == Terminal.TK_DOWN ? 1 : 0;
                    if (TK == Terminal.TK_ENTER || TK == Terminal.TK_SPACE || TK == Terminal.TK_E)
                    {
                        int Bulls = qwe.CheckWord();
                        //Terminal.Set("window: title=" + "'" + "Парола: " + ((Bulls == 8) ? "верная" : "неверная") + " " + Bulls + "'");
                    }
                    if (TK == Terminal.TK_BACKSPACE)
                    {
                        qwe = new HackGame(WordsDictionary);
                        qwe.GenerateWordsTable();
                    }
                    if (TK == Terminal.TK_ESCAPE)
                    {
                        Terminal.Close();
                    }
                    if (TK == Terminal.TK_MOUSE_CLICKS || TK == Terminal.TK_MOUSE_LEFT)
                    {
                        mx = Terminal.State(Terminal.TK_MOUSE_X);
                        my = Terminal.State(Terminal.TK_MOUSE_Y);
                        //Terminal.Set("window: title=" + "'" + "mx: " + mx + " my: " + my + "'");
                    }

                }
                qwe.MoveCursor(new System.Drawing.Point(dx, dy));
                if (mx >= 0 && my >= 0) qwe.MoveToCursor(new System.Drawing.Point(mx, my));
                qwe.ShowFrame();
            }
            /*
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
            */
            Terminal.Close();
        }
    }

}
