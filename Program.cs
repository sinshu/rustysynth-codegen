GenInstrumentRegion.Run();
GenInstrumentRegionTestUtil.Run();
GenPresetRegion.Run();
GenPresetRegionTestUtil.Run();

GenInstrumentRegionTest.Run(
    @"C:\Users\sinsh\Desktop\sf2\TimGM6mb.sf2",
    "timgm6mb_instrument_test.txt");

GenInstrumentRegionTest.Run(
    @"C:\Users\sinsh\Desktop\sf2\GeneralUser GS MuseScore v1.442.sf2",
    "musescore_instrument_test.txt");

GenPresetRegionTest.Run(
    @"C:\Users\sinsh\Desktop\sf2\TimGM6mb.sf2",
    "timgm6mb_preset_test.txt");

GenPresetRegionTest.Run(
    @"C:\Users\sinsh\Desktop\sf2\GeneralUser GS MuseScore v1.442.sf2",
    "musescore_preset_test.txt");

GenSampleHeaderTest.Run(
    @"C:\Users\sinsh\Desktop\sf2\TimGM6mb.sf2",
    "timgm6mb_sample_test.txt");

GenSampleHeaderTest.Run(
    @"C:\Users\sinsh\Desktop\sf2\GeneralUser GS MuseScore v1.442.sf2",
    "musescore_sample_test.txt");
