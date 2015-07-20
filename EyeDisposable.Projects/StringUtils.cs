using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EyeDisposable.Projects
{
    public static class StringUtils
    {
        public static string MakeRelativePath(this string absPathFull, string relTo)
        {
            string absPath = System.IO.Path.GetDirectoryName(absPathFull) + System.IO.Path.DirectorySeparatorChar;
            string[] absDirs = absPath.Split('\\');
            string[] relDirs = relTo.Split('\\'); // Get the shortest of the two paths
            int len = absDirs.Length < relDirs.Length ? absDirs.Length : relDirs.Length;
            // Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index; // Find common root
            for (index = 0; index < len; index++)
            { if (absDirs[index] == relDirs[index]) lastCommonRoot = index; else break; }
            // If we didn't find a common prefix then throw

            if (lastCommonRoot == -1) { throw new ArgumentException("Paths do not have a common base"); }
            // Build up the relative path
            var relativePath = new StringBuilder();
            // Add on the ..
            for (index = lastCommonRoot + 1; index < absDirs.Length; index++) { if (absDirs[index].Length > 0) relativePath.Append("..\\"); }
            // Add on the folders
            for (index = lastCommonRoot + 1; index < relDirs.Length - 1; index++) { relativePath.Append(relDirs[index] + "\\"); }
            relativePath.Append(relDirs[relDirs.Length - 1]);

            if (!String.IsNullOrEmpty(System.IO.Path.GetFileName(absPathFull)))
                relativePath.Append(System.IO.Path.GetFileName(absPathFull));
            return relativePath.ToString();
        }

        public static string Unquote(this string input)
        {
            return input.Trim('\'', '"');
        }

        public static string[] SplitArguments(this string input)
        {
            var result = new List<string>();
            int lastPos = 0;
            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '"':
                    case '\'':
                        char end = input[i];
                        i++;
                        lastPos = i;
                        while (i < input.Length && input[i] != end)
                            i++;
                        var str = input.Substring(lastPos, i - lastPos).Trim();
                        if (!String.IsNullOrEmpty(str))
                            result.Add(str);
                        i++;
                        lastPos = i;
                        break;
                    case ' ':
                        var s = input.Substring(lastPos, i - lastPos).Trim();
                        if (!String.IsNullOrEmpty(s))
                            result.Add(s);
                        lastPos = i + 1;
                        break;
                }
            }
            if (lastPos != input.Length-1)
                result.Add(input.Substring(lastPos, input.Length - lastPos).Trim());
            return result.ToArray();
        }
    }
}
