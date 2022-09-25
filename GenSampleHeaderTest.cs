using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MeltySynth;

public static class GenSampleHeaderTest
{
    public static void Run(string soundFontPath, string dstPath)
    {
        var soundFont = new SoundFont(soundFontPath);

        using (var writer = new StreamWriter(dstPath))
        {
            for (var sh = 0; sh < soundFont.SampleHeaders.Count; sh++)
            {
                var header = soundFont.SampleHeaders[sh];
                var values = GetValues(header);

                writer.WriteLine("    // " + header.Name);
                writer.WriteLine("    let values: [i32; 7] = [" + string.Join(", ", values) + "];");
                writer.WriteLine("    sample_util::check(&sf.get_sample_headers()[" + sh + "], &values);");
                writer.WriteLine();
            }
        }
    }

    private static int[] GetValues(SampleHeader sample)
    {
        var list = new List<int>();
        list.Add(sample.Start);
        list.Add(sample.End);
        list.Add(sample.StartLoop);
        list.Add(sample.EndLoop);
        list.Add(sample.SampleRate);
        list.Add(sample.OriginalPitch);
        list.Add(sample.PitchCorrection);
        return list.ToArray();
    }
}
