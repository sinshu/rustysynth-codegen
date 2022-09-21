﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class GenInstrumentRegion
{
    private static readonly string srcPath = "cs_instrument_region.txt";
    private static readonly string dstPath = "rs_instrument_region.txt";

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

                writer.WriteLine("    pub fn get_" + ToLowerSnake(info.Name) + "(&self) -> " + CsTypeToRustType(info.Type));
                writer.WriteLine("    {");

                var body = info.Body.Replace(";", "");

                body = regGeneratorType.Replace(body, match =>
                {
                    var value = match.Groups[1].Value;
                    return "GeneratorType::" + ToUpperSnake(value) + " as usize";
                });

                body = regSoundFontMath.Replace(body, match =>
                {
                    var value = match.Groups[1].Value;
                    return "SoundFontMath::" + ToLowerSnake(value);
                });

                body = regFloatValue.Replace(body, match =>
                {
                    var value = match.Groups[1].Value;
                    return value + "f32";
                });

                body = body.Replace("sample.StartLoop", "self.sample_start_loop");
                body = body.Replace("sample.EndLoop", "self.sample_end_loop");
                body = body.Replace("sample.Start", "self.sample_start");
                body = body.Replace("sample.End", "self.sample_end");
                body = body.Replace("sample.PitchCorrection", "self.sample_pitch_correction");

                body = body.Replace("StartLoopAddressOffset", "self.get_start_loop_address_offset()");
                body = body.Replace("EndLoopAddressOffset", "self.get_end_loop_address_offset()");
                body = body.Replace("StartAddressOffset", "self.get_start_address_offset()");
                body = body.Replace("EndAddressOffset", "self.get_end_address_offset()");

                body = body.Replace("this[", "self.gs[");

                if (info.Type == "float")
                {
                    body = body.Replace("]", "] as f32");
                }
                else
                {
                    body = body.Replace("]", "] as i32");
                }

                if (info.Name == "SampleModes")
                {
                    body = "if self.gs[GeneratorType::SAMPLE_MODES as usize] != 2 { self.gs[GeneratorType::SAMPLE_MODES as usize] as i32 } else { LoopMode::NO_LOOP }";
                }
                else if (info.Name == "RootKey")
                {
                    body = "if self.gs[GeneratorType::OVERRIDING_ROOT_KEY as usize] != -1 { self.gs[GeneratorType::OVERRIDING_ROOT_KEY as usize] as i32 } else { self.sample_original_pitch }";
                }

                writer.WriteLine("        " + body);

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
