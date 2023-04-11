using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MeltySynth;

public static class GenPresetRegionTest
{
    public static void Run(string soundFontPath, string dstPath)
    {
        var soundFont = new SoundFont(soundFontPath);

        using (var writer = new StreamWriter(dstPath))
        {
            for (var pre = 0; pre < soundFont.Presets.Count; pre++)
            {
                var preset = soundFont.Presets[pre];

                writer.WriteLine("    // ============================================================");
                writer.WriteLine("    //  " + preset.Name);
                writer.WriteLine("    // ============================================================");

                for (var reg = 0; reg < preset.Regions.Count; reg++)
                {
                    var region = preset.Regions[reg];

                    var values = GetValues(region);
                    writer.WriteLine("    values = {" + string.Join(", ", values.Select(x => Format(x))) + "}");
                    writer.WriteLine("    check_preset_region(t, &sf.presets[" + pre + "].regions[" + reg + "], values[:])");
                }

                writer.WriteLine();
            }
        }
    }

    private static double[] GetValues(PresetRegion region)
    {
        var list = new List<double>();
        list.Add(region.ModulationLfoToPitch);
        list.Add(region.VibratoLfoToPitch);
        list.Add(region.ModulationEnvelopeToPitch);
        list.Add(region.InitialFilterCutoffFrequency);
        list.Add(region.InitialFilterQ);
        list.Add(region.ModulationLfoToFilterCutoffFrequency);
        list.Add(region.ModulationEnvelopeToFilterCutoffFrequency);
        list.Add(region.ModulationLfoToVolume);
        list.Add(region.ChorusEffectsSend);
        list.Add(region.ReverbEffectsSend);
        list.Add(region.Pan);
        list.Add(region.DelayModulationLfo);
        list.Add(region.FrequencyModulationLfo);
        list.Add(region.DelayVibratoLfo);
        list.Add(region.FrequencyVibratoLfo);
        list.Add(region.DelayModulationEnvelope);
        list.Add(region.AttackModulationEnvelope);
        list.Add(region.HoldModulationEnvelope);
        list.Add(region.DecayModulationEnvelope);
        list.Add(region.SustainModulationEnvelope);
        list.Add(region.ReleaseModulationEnvelope);
        list.Add(region.KeyNumberToModulationEnvelopeHold);
        list.Add(region.KeyNumberToModulationEnvelopeDecay);
        list.Add(region.DelayVolumeEnvelope);
        list.Add(region.AttackVolumeEnvelope);
        list.Add(region.HoldVolumeEnvelope);
        list.Add(region.DecayVolumeEnvelope);
        list.Add(region.SustainVolumeEnvelope);
        list.Add(region.ReleaseVolumeEnvelope);
        list.Add(region.KeyNumberToVolumeEnvelopeHold);
        list.Add(region.KeyNumberToVolumeEnvelopeDecay);
        list.Add(region.KeyRangeStart);
        list.Add(region.KeyRangeEnd);
        list.Add(region.VelocityRangeStart);
        list.Add(region.VelocityRangeEnd);
        list.Add(region.InitialAttenuation);
        list.Add(region.CoarseTune);
        list.Add(region.FineTune);
        list.Add(region.ScaleTuning);
        return list.ToArray();
    }

    private static string Format(double x)
    {
        if (x == 0)
        {
            return "0";
        }

        if (x % 1 == 0)
        {
            return ((long)Math.Round(x)).ToString();
        }

        var digits = Math.Log10(Math.Abs(x)) + 1;

        var right = 5 - digits;

        var sb = new StringBuilder("0.");
        for (var i = 0; i < right; i++)
        {
            sb.Append("#");
        }

        return x.ToString(sb.ToString());
    }
}
