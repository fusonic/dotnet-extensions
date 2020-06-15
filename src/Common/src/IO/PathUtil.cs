namespace Fusonic.Extensions.Common.IO
{
    public static class PathUtil
    {
        /// <summary>
        /// Removes invalid characters from a filename.
        /// Note: When using this method, you don't have to call RemoveInvalidPathChars() anymore, as all the chars removed by RemoveInvalidPathChars() are also removed here.
        /// 
        /// Path.GetInvalidFileNameChars() returns different values based on the OS. Linux for example only returns char 0 and 47 '/'. This uses the set returned on windows.
        /// (The windows set seems not to lack any chars, linux and mac os sets are a subset of this, so we should be fine everywhere)
        /// </summary>
        public static string RemoveInvalidFilenameChars(string filename)
        {
            var output = new char[filename.Length];
            int i = 0;
            
            //while checking against a HashSet<char> is the fastest way when using collections, that ugly switch statement is still more than twice as fast
            foreach (char c in filename)
            {
                switch (c)
                {
                    case '\"':
                    case '<':
                    case '>':
                    case '|':
                    case ':':
                    case '*':
                    case '?':
                    case '\\':
                    case '/':
                    case '\0':
                    case (char)1:
                    case (char)2:
                    case (char)3:
                    case (char)4:
                    case (char)5:
                    case (char)6:
                    case (char)7:
                    case (char)8:
                    case (char)9:
                    case (char)10:
                    case (char)11:
                    case (char)12:
                    case (char)13:
                    case (char)14:
                    case (char)15:
                    case (char)16:
                    case (char)17:
                    case (char)18:
                    case (char)19:
                    case (char)20:
                    case (char)21:
                    case (char)22:
                    case (char)23:
                    case (char)24:
                    case (char)25:
                    case (char)26:
                    case (char)27:
                    case (char)28:
                    case (char)29:
                    case (char)30:
                    case (char)31:
                        break;

                    default:
                        output[i++] = c;
                        break;
                }
            }

            return new string(output, 0, i);
        }

        /// <summary>
        ///    Removes invalid characters from a path.
        ///    
        ///    Path.GetInvalidPathChars() returns different values based on the OS. Linux for example only returns char 0. This uses the set returned on windows.
        ///    (The windows set seems not to lack any chars, linux and mac os sets are a subset of this, so we should be fine everywhere)
        /// </summary>
        public static string RemoveInvalidPathChars(string path)
        {
            var output = new char[path.Length];
            int i = 0;

            //while checking against a HashSet<char> is the fastest way when using collections, that ugly switch statement is still more than twice as fast
            foreach (char c in path)
            {
                switch (c)
                {
                    case '\"':
                    case '<':
                    case '>':
                    case '|':
                    case '*':
                    case '?':
                    case '\0':
                    case (char)1:
                    case (char)2:
                    case (char)3:
                    case (char)4:
                    case (char)5:
                    case (char)6:
                    case (char)7:
                    case (char)8:
                    case (char)9:
                    case (char)10:
                    case (char)11:
                    case (char)12:
                    case (char)13:
                    case (char)14:
                    case (char)15:
                    case (char)16:
                    case (char)17:
                    case (char)18:
                    case (char)19:
                    case (char)20:
                    case (char)21:
                    case (char)22:
                    case (char)23:
                    case (char)24:
                    case (char)25:
                    case (char)26:
                    case (char)27:
                    case (char)28:
                    case (char)29:
                    case (char)30:
                    case (char)31:
                        break;

                    default:
                        output[i++] = c;
                        break;
                }
            }

            return new string(output, 0, i);
        }
    }
}