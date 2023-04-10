using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class GenInstrumentRegion
{
    private static readonly string srcPath = "cs_instrument_region.txt";
    private static readonly string dstPath = "odin_instrument_region.txt";

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

                writer.WriteLine("instrument_get_" + ToLowerSnake(info.Name) + " :: proc(ir: ^Instrument_Region) -> " + CsTypeToRustType(info.Type) + " {");

                var body = info.Body.Replace(";", "");

                body = regGeneratorType.Replace(body, match =>
                {
                    var value = match.Groups[1].Value;
                    return "Generator_Type." + ToOdin(value);
                });

                body = regSoundFontMath.Replace(body, match =>
                {
                    var value = match.Groups[1].Value;
                    return ToLowerSnake(value);
                });

                body = regFloatValue.Replace(body, match =>
                {
                    var value = match.Groups[1].Value;
                    return "f32(" + value + ")";
                });

                body = body.Replace("sample.StartLoop", "ir.sample.start_loop");
                body = body.Replace("sample.EndLoop", "ir.sample.end_loop");
                body = body.Replace("sample.Start", "ir.sample.start");
                body = body.Replace("sample.End", "ir.sample.end");
                body = body.Replace("sample.PitchCorrection", "ir.sample.pitch_correction");

                body = body.Replace("StartLoopAddressOffset", "instrument_get_start_loop_address_offset(ir)");
                body = body.Replace("EndLoopAddressOffset", "instrument_get_end_loop_address_offset(ir)");
                body = body.Replace("StartAddressOffset", "instrument_get_start_address_offset(ir)");
                body = body.Replace("EndAddressOffset", "instrument_get_end_address_offset(ir)");

                body = body.Replace("this[", "ir.gs[");

                if (info.Type == "float")
                {
                    body = body.Replace("ir.gs[", "f32(ir.gs[");
                    body = body.Replace("]", "])");
                }
                else
                {
                    body = body.Replace("ir.gs[", "i32(ir.gs[");
                    body = body.Replace("]", "])");
                }

                if (info.Name == "FineTune")
                {
                    body = "i32(ir.gs[Generator_Type.Fine_Tune]) + i32(ir.sample.pitch_correction)";
                }
                else if (info.Name == "SampleModes")
                {
                    body = "ir.gs[Generator_Type.Sample_Modes] != 2 ? i32(ir.gs[Generator_Type.Sample_Modes]) : i32(Loop_Mode.No_Loop)";
                }
                else if (info.Name == "RootKey")
                {
                    body = "ir.gs[Generator_Type.Overriding_Root_Key] != -1 ? i32(ir.gs[Generator_Type.Overriding_Root_Key]) : i32(ir.sample.original_pitch)";
                }

                writer.WriteLine("    return " + body);

                writer.WriteLine("}");
                writer.WriteLine();
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
