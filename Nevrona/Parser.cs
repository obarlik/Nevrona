using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Nevrona
{
    public abstract class Parser<T>
    {
        public Parser()
        {
        }


        public int Index { get; protected set; }


        protected string Buffer;


        protected char? Next()
        {
            return Current = 
                (Index < Buffer.Length - 1) ? 
                    (char?)Buffer[++Index] :
                    null;
        }


        public char? Current { get; protected set; }

        public bool EOF { get; protected set; }


        public string ReadWhile(Func<Char, bool> criteria)
        {
            var s = "";
            
            while (Current != null
                && criteria(Current.Value))
            {
                s += Current;
                Next();
            }

            return s;
        }


        public string ReadChars(string charset)
        {
            return ReadWhile(c => charset.IndexOf(c) >= 0);
        }


        public string ReadAlpha()
        {
            return ReadWhile(c => char.IsLetter(c));
        }


        public string ReadDigit()
        {
            return ReadWhile(c => char.IsDigit(c));
        }


        public string ReadAlphaNum()
        {
            return ReadWhile(c => char.IsLetterOrDigit(c));
        }


        public virtual T Parse(string s)
        {
            Index = -1;
            Buffer = s;
            Next();
            return default(T);
        }

    }
}
