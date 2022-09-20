// Copyright (c) Fusonic GmbH. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Runtime.CompilerServices;

namespace Fusonic.Extensions.Common.IO;

public static class PathUtil
{
    /// <summary> Gets if a character is an invalid character for file names. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInvalidFilenameChar(char character) => character switch
    {
        //while checking against a HashSet<char> is the fastest way when using collections, that ugly switch statement is still more than twice as fast
        '\"' => true,
        '<' => true,
        '>' => true,
        '|' => true,
        '\0' => true,
        ':' => true,
        '*' => true,
        '?' => true,
        '\\' => true,
        '/' => true,
        (char)1 => true,
        (char)2 => true,
        (char)3 => true,
        (char)4 => true,
        (char)5 => true,
        (char)6 => true,
        (char)7 => true,
        (char)8 => true,
        (char)9 => true,
        (char)10 => true,
        (char)11 => true,
        (char)12 => true,
        (char)13 => true,
        (char)14 => true,
        (char)15 => true,
        (char)16 => true,
        (char)17 => true,
        (char)18 => true,
        (char)19 => true,
        (char)20 => true,
        (char)21 => true,
        (char)22 => true,
        (char)23 => true,
        (char)24 => true,
        (char)25 => true,
        (char)26 => true,
        (char)27 => true,
        (char)28 => true,
        (char)29 => true,
        (char)30 => true,
        (char)31 => true,
        _ => false
    };

    /// <summary> Gets if a character is an invalid character for paths. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInvalidPathChar(char character) => character switch
    {
        //while checking against a HashSet<char> is the fastest way when using collections, that ugly switch statement is still more than twice as fast
        '|' => true,
        '\0' => true,
        '\"' => true,
        '<' => true,
        '>' => true,
        ':' => true,
        '*' => true,
        '?' => true,
        (char)1 => true,
        (char)2 => true,
        (char)3 => true,
        (char)4 => true,
        (char)5 => true,
        (char)6 => true,
        (char)7 => true,
        (char)8 => true,
        (char)9 => true,
        (char)10 => true,
        (char)11 => true,
        (char)12 => true,
        (char)13 => true,
        (char)14 => true,
        (char)15 => true,
        (char)16 => true,
        (char)17 => true,
        (char)18 => true,
        (char)19 => true,
        (char)20 => true,
        (char)21 => true,
        (char)22 => true,
        (char)23 => true,
        (char)24 => true,
        (char)25 => true,
        (char)26 => true,
        (char)27 => true,
        (char)28 => true,
        (char)29 => true,
        (char)30 => true,
        (char)31 => true,
        _ => false
    };

    /// <summary> Gets if a filename contains an invalid character. </summary>
    public static bool HasInvalidFilenameChar(string filename)
    {
        foreach (char c in filename)
        {
            if (IsInvalidFilenameChar(c))
                return true;
        }

        return false;
    }

    /// <summary> Gets if a path contains an invalid character. </summary>
    public static bool HasInvalidPathChar(string path)
    {
        foreach (char c in path)
        {
            if (IsInvalidPathChar(c))
                return true;
        }

        return false;
    }

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
        var i = 0;

        foreach (char c in filename)
        {
            if (!IsInvalidFilenameChar(c))
                output[i++] = c;
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

        foreach (char c in path)
        {
            if (!IsInvalidPathChar(c))
                output[i++] = c;
        }

        return new string(output, 0, i);
    }
}
