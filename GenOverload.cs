using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class GenOverload
{
    private static readonly string srcPath1 = "cs_instrument_region.txt";
    private static readonly string srcPath2 = "cs_preset_region.txt";
    private static readonly string dstPath = "odin_overload.txt";

    private static readonly Regex regWordsInCamelCaseSymbol = new Regex(@"[A-Z][0-9a-z]*");
    private static readonly Regex regGeneratorType = new Regex(@"GeneratorType\.([0-9A-Za-z]+)");
    private static readonly Regex regSoundFontMath = new Regex(@"SoundFontMath\.([0-9A-Za-z]+)");
    private static readonly Regex regFloatValue = new Regex(@"([0-9]+\.[0-9]+)F");

    public static void Run()
    {
        using (var writer = new StreamWriter(dstPath))
        {
            var presetFuncs = File.ReadLines(srcPath2).Select(line => new FuncInfo(line)).ToDictionary(info => info.Name, info => info);

            foreach (var line in File.ReadLines(srcPath1))
            {
                var info = new FuncInfo(line);

                var insName = "instrument_get_" + ToLowerSnake(info.Name);
                var overload = "get_" + ToLowerSnake(info.Name);
                if (presetFuncs.ContainsKey(info.Name))
                {
                    var preName = "preset_get_" + ToLowerSnake(presetFuncs[info.Name].Name);
                    writer.WriteLine(overload + " :: proc{" + insName + ", " + preName + "}");
                }
                else
                {
                    writer.WriteLine(overload + " :: proc{" + insName + "}");
                }
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

    private static string ToUpperSnake(string value)
    {
        var matches = regWordsInCamelCaseSymbol.Matches(value);

        var sb = new StringBuilder();
        foreach (var match in matches.AsEnumerable())
        {
            if (sb.Length > 0)
            {
                sb.Append("_");
            }

            sb.Append(match.Value.ToUpper());
        }
        return sb.ToString();
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
