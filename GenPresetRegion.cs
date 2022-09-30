using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class GenPresetRegion
{
    private static readonly string srcPath = "cs_preset_region.txt";
    private static readonly string dstPath = "rs_preset_region.txt";

    private static readonly Regex regWordsInCamelCaseSymbol = new Regex(@"[A-Z][0-9a-z]*");
    private static readonly Regex regGeneratorType = new Regex(@"GeneratorType\.([0-9A-Za-z]+)");
    private static readonly Regex regSoundFontMath = new Regex(@"SoundFontMath\.([0-9A-Za-z]+)");
    private static readonly Regex regFloatValue = new Regex(@"([0-9]+\.[0-9]+)F");

    public static void Run()
    {
        using (var writer = new StreamWriter(dstPath))
        {
            foreach (var line in File.ReadLines(srcPath))
            {
                var info = new FuncInfo(line);

                writer.WriteLine("    pub fn get" + info.Name + "(self: *const Self) " + CsTypeToRustType(info.Type));
                writer.WriteLine("    {");

                var body = info.Body.Replace(";", "");

                body = regGeneratorType.Replace(body, match =>
                {
                    var value = match.Groups[1].Value;
                    return "GeneratorType." + ToUpperSnake(value);
                });

                body = regSoundFontMath.Replace(body, match =>
                {
                    var value = match.Groups[1].Value;
                    return "SoundFontMath." + ToLowerCamel(value);
                });

                body = regFloatValue.Replace(body, match =>
                {
                    var value = match.Groups[1].Value;
                    return value + "";
                });

                if (info.Type == "float")
                {
                    body = body.Replace("this[", "@intToFloat(f32, self.gs[");
                    body = body.Replace("]", "])");
                }
                else
                {
                    body = body.Replace("this[", "@intCast(i32, self.gs[");
                    body = body.Replace("]", "])");
                }

                writer.WriteLine("        return " + body + ";");

                writer.WriteLine("    }");
                writer.WriteLine();
            }
        }
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

    private static string ToLowerCamel(string value)
    {
        var matches = regWordsInCamelCaseSymbol.Matches(value);

        var sb = new StringBuilder();
        foreach (var match in matches.AsEnumerable())
        {
            if (sb.Length == 0)
            {
                sb.Append(match.Value.ToLower());
            }
            else
            {
                sb.Append(match.Value);
            }
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
