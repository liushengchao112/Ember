using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Utils
{
    public class InputUtil
    {
        #region Error String
        private const string sameUseNameError = "密码不能与用户名相同";
        private const string lengthError = "密码长度为8-16";
        private const string lettersNumbersError = "密码必须是字母数字组合";
        private const string sameNumError = "密码不能有连续4个及以上相同数字";
        private const string continuousNumError = "不能包含4个及以上连续数字";
        #endregion

        static Regex emailRegex = new Regex( @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$" );
        static Regex phoneRegex = new Regex( @"^[0-9]*$" );

        //TODO: badword list shared with server, also, assetbundle
        static List<string> badWords = new List<string> { "fuck", "shit", "pussy" };

        static string allowedChars = " _";

        public static bool IsValidInput( string userid )
        {
            if ( string.IsNullOrEmpty( userid ) )
                return false;

            int count = userid.Length;

            if ( userid[0] == ' ' || userid[userid.Length - 1] == ' ' )
                return false;

            for ( int i = 0; i < userid.Length; i++ )
            {
                if ( allowedChars.Contains( userid[i].ToString() ) )
                    continue;
                UnicodeCategory cat = char.GetUnicodeCategory( userid[i] );
                switch ( cat )
                {
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                        continue;
                    case UnicodeCategory.OtherLetter:
                        count++;
                        continue;
                    default:
                        return false;
                }
            }

            if ( count < 4 || count > 12 )
                return false;

            return !ContainsBadWord( userid );
        }

        private static bool ContainsBadWord( string userid )
        {
            userid = userid.ToLower();
            foreach ( string bad in badWords )
                if ( userid.Contains( bad ) )
                    return true;
            return false;
        }

        public static bool IsValidEmail( string email )
        {
            Match match = emailRegex.Match( email );
            return match.Success;
        }

        public static bool IsValidPhoneNumber( string phoneNr )
        {
            Match match = phoneRegex.Match( phoneNr );
            return match.Success;
        }

        public static bool IsValidPassword( string password, string useName, out string error )
        {
            //TODO:Easy to test, password not limited, need to be removed in the future -- Victor
            error = "";
            return password.Length > 5 && password.Length < 13;

            if ( password == useName )
            {
                error = sameUseNameError;
                return false;
            }

            //The password length is 8-16
            string pattern0 = @"(^\w{8,16}$)";
            if ( !Regex.IsMatch( password, pattern0 ) )
            {
                error = lengthError;
                return false;
            }

            //Passwords can only be letters and numbers.
            string pattern1 = @"[^a-zA-Z\d]";
            if ( Regex.IsMatch( password, pattern1 ) )
            {
                error = lettersNumbersError;
                return false;
            }

            //Passwords must have letters and numbers at the same time
            string pattern2 = @"(\d[a-zA-z]|[a-zA-Z]\d)";
            if ( !Regex.IsMatch( password, pattern2 ) )
            {
                error = lettersNumbersError;
                return false;
            }

            //Passwords cannot have 4 consecutive letters or numbers
            string pattern3 = @"(\d|[a-zA-Z])\1{3,}";
            if ( Regex.IsMatch( password, pattern3 ) )
            {
                error = sameNumError;
                return false;
            }

            //the continuous number in password cannot be Arithmetic sequence
            //这个翻译不太懂，就是密码中连续4+的数字不能是差值为1或-1的等差数列（如：1234，98765这种）
            string pattern4 = @"(\d){4,}";
            MatchCollection matchs = Regex.Matches( password, pattern4 );

            for ( int i = 0; i < matchs.Count; i++ )
            {
                int addTime = 1, lessTime = 1;
                for ( int r = 1; r < matchs[i].Value.Length; r++ )
                {
                    int num = int.Parse( matchs[i].Value[r].ToString() );
                    int last = int.Parse( matchs[i].Value[r - 1].ToString() );

                    addTime = ( num == ( last + 1 ) ) ? ++addTime : addTime;
                    lessTime = ( num == ( last - 1 ) ) ? ++lessTime : lessTime;

                    if ( addTime >= 4 && lessTime == 1 )
                    {
                        error = continuousNumError;
                        return false;
                    }
                    if ( lessTime >= 4 && addTime == 1 )
                    {
                        error = continuousNumError;
                        return false;
                    }
                    if ( num != ( last + 1 ) && num != ( last - 1 ) )
                        addTime = lessTime = 1;
                }
            }
            error = "";
            return true;
        }
    }
}

