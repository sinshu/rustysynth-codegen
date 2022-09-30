using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class GenPresetRegionTestUtil
{
    private static readonly string srcPath = "cs_preset_region.txt";
    private static readonly string dstPath = "rs_preset_region_test_util.txt";

    private static readonly Regex regWordsInCamelCaseSymbol = new Regex(@"[A-Z][0-9a-z]*");

    public static void Run()
    {
        using (var writer = new StreamWriter(dstPath))
        {
            var index = 0;

            foreach (var line in File.ReadLines(srcPath))
            {
                var info = new FuncInfo(line);

                var rsValue = "region.get" + info.Name + "()";

                if (info.Type != "float")
                {
                    rsValue = "@intToFloat(f64, " + rsValue + ")";
                }

                writer.WriteLine("    debug.assert(areEqual(" + rsValue + ", values[" + index + "]));");

                index++;
            }
        }
    }

    private static string ToLowerSnake(string value)
    {
        var matches = regWordsInCamelCaseSymbol.Matches(value);

        var sb = new StringBuilder();
        foreach (var match in matches.AsEnumerable())
        {
            if (sb.Length > 0)
            {
                sb.Append("_");
            }

            sb.Append(match.Value.ToLower());
        }
        return sb.ToString();
    }

    private static string CsTypeToRustType(string value)
    {
        switch (value)
        {
            case "int":
                return "i32";
            case "float":
                return "f32";
            default:
                return "i32";
        }
    }

    private class FuncInfo
    {
        private string type;
        private string name;
        private string body;

        public FuncInfo(string line)
        {
            var split = line.Split(' ');
            type = split[0];
            name = split[1];
            body = string.Join(' ', split.Skip(3));
        }

        public string Type => type;
        public string Name => name;
        public string Body => body;

        public override string ToString()
        {
            return type + ", " + name + ", " + body;
        }
    }
}
