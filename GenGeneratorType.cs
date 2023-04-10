using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class GenGeneratorType
{
    private static readonly string srcPath = "cs_generator_type.txt";
    private static readonly string dstPath = "odin_generator_type.txt";

    private static readonly Regex regWordsInCamelCaseSymbol = new Regex(@"[A-Z][0-9a-z]*");

    public static void Run()
    {
        using (var writer = new StreamWriter(dstPath))
        {
            foreach (var line in File.ReadLines(srcPath))
            {
                var split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var name = split[0];
                var value = split[2];
                writer.WriteLine("    " + ToOdin(name) + " = " + value);
            }
        }
    }

    private static string ToOdin(string value)
    {
        var matches = regWordsInCamelCaseSymbol.Matches(value);

        var sb = new StringBuilder();
        foreach (var match in matches.AsEnumerable())
        {
            if (sb.Length > 0)
            {
                sb.Append("_");
            }

            sb.Append(match.Value);
        }
        sb.Replace("I_D", "ID");
        return sb.ToString();
    }
}
