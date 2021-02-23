// FuXin.Crypto.DESCryptoService
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Demo.Core.JM
{
	public class Crypto
    {
        public static char[] Key = CharArrayType.FromString("`1234567890-=~!@#$%^&*()_+qwertyuiop[]\\QWERTYUIOP{}|asdfghjkl;'ASDFGHJKL:zxcvbnm,./ZXCVBNM<>? \"");

        public static string Decrypt(string str)
        {
            if (StringType.StrCmp(str.Trim(), "", false) == 0)
            {
                return "";
            }
            StringBuilder builder = new StringBuilder();
            try
            {
                short num3 = (short)(str.Length - 1);
                for (short i = 0; i <= num3; i = (short)(i + 1))
                {
                    short charPos = GetCharPos(CharType.FromString(str.Substring(i, 1)));
                    if (charPos == 0)
                    {
                        charPos = (short)(Key.Length + 1);
                    }
                    builder.Append(GetChar((short)(((short)(charPos - i)) - 1)));
                }
            }
            catch (Exception exception1)
            {
                ProjectData.SetProjectError(exception1);
                Exception exception = exception1;
                ProjectData.ClearProjectError();
            }
            return builder.ToString();
        }

        public static string Encrypt(string str)
        {
            if (StringType.StrCmp(str.Trim(), "", false) == 0)
            {
                return "";
            }
            StringBuilder builder = new StringBuilder();
            try
            {
                short num3 = (short)(str.Length - 1);
                for (short i = 0; i <= num3; i = (short)(i + 1))
                {
                    short charPos = GetCharPos(CharType.FromString(str.Substring(i, 1)));
                    if (charPos == Key.Length)
                    {
                        charPos = -1;
                    }
                    builder.Append(GetChar((short)(((short)(charPos + i)) + 1)));
                }
            }
            catch (Exception exception1)
            {
                ProjectData.SetProjectError(exception1);
                Exception exception = exception1;
                ProjectData.ClearProjectError();
            }
            return builder.ToString();
        }

        private static char GetChar(short Pos)
        {
            while (Pos < 0)
            {
                if (Pos < 0)
                {
                    Pos = (short)(0x60 + Pos);
                }
            }
            while (Pos >= 0x60)
            {
                if (Pos >= 0x60)
                {
                    Pos = (short)(Pos - 0x60);
                }
            }
            return Key[Pos];
        }

        private static short GetCharPos(char ch)
        {
            short num = -1;
            short num3 = (short)(Key.Length - 1);
            for (short i = 0; i <= num3; i = (short)(i + 1))
            {
                if (ch == Key[i])
                {
                    return i;
                }
            }
            return num;
        }
	}
}
