using System;
using NUnit.Framework;

namespace SharpDuster.Tests
{
      /// <summary>
    /// Summary description for FileSanitizerTests
    /// </summary>
    [TestFixture]
    public class FileSanitizerTests
    {
        [Test]
        public void SanitizeTest()
        {
            Assert.IsNull(FileSanitizer.Sanitize(""));
            Assert.IsTrue(FileSanitizer.Sanitize("Burn.Notice.S02E01.Blah.Blah.avi").Name == "Burn.Notice.S02E01");
            Assert.IsTrue(FileSanitizer.Sanitize("Burn Notice - S02E01 - Blah Blah.avi").Name == "Burn.Notice.S02E01");
        }

        [Test]
        public void FindTitleTest()
        {
            Assert.IsTrue(FileSanitizer.FindTitle("Burn.Notice.S02E01.Blah Blah.avi") == "Burn.Notice.");
            Assert.IsTrue(FileSanitizer.FindTitle("Burn Notice - S02E01 - Blah Blah.avi") == "Burn.Notice.");Assert.IsTrue(FileSanitizer.FindTitle("burn notice - S02E01 - Blah Blah.avi") == "Burn.Notice.");
        }

        [Test]
        public void FindSeasonAndEpisodeTest()
        {
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode("Burn Notice  2 x 01 - Blah blah.avi"), 2, 1);
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode("Burn Notice  12 x 11 - Blah blah.avi"), 12, 11);
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode("S02xE01"), 2, 1);
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode("Burn Notice S02xE01 Blah blah.avi"), 2, 1);
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode("Burn Notice - S02xE01 - Blah blah.avi"), 2, 1);
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode("Burn.Notice.S02E01.Blah blah.avi"), 2, 1);
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode("Burn Notice - S02E01 - Blahvlah.avi"), 2, 1);

            // Corner cases
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode(@"How I Met Your Mother 06E15 Oh Honey.720p.HDTV.FoV-\[VTV\]"), 6, 15);
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode(@"Outcasts.1x02.HDTV_XviD-FoV.\[VTV\].avi"), 1, 2);
            AssertSeasonAndEpisode(FileSanitizer.FindSeasonAndEpisode("outcasts.1x03.hdtv_xvid-fov.avi"), 1, 3);
        }

        private static void AssertSeasonAndEpisode(Tuple<int, int> data, int expectedSeason, int expectedEpisode)
        {
            Assert.IsTrue(data.Item1 == expectedSeason, "Invalid season: expected {0}, actual {1}", expectedSeason, data.Item1);
            Assert.IsTrue(data.Item2 == expectedEpisode, "Invalid season: expected {0}, actual {1}", expectedEpisode, data.Item2);
        }
    }
}
