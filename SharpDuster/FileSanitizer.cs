using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpDuster
{
    public class TvShowFile
    {
        public string Extension { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public string Title { get; set; }

        public string FolderName
        {
            get { return Title.Replace(".", " ").Trim(); }
        }

        public string Info 
        {
            get
            {
                return string.Format("S{0}E{1}", FormatNumber(Season), FormatNumber(Episode));
            }
        }

        public string Name
        {
            get { return string.Format("{0}{1}", Title, Info); }
        }

        public string Filename
        {
            get { return string.Format("{0}{1}", Name, Extension); }
        }

        private static string FormatNumber(int num)
        {
            return num.ToString().Length > 1
                       ? num.ToString()
                       : string.Format("0{0}", num); 
        }
    }

    public class FileSanitizer
    {
        public static TvShowFile Sanitize (string filename)
        {
        	if (!string.IsNullOrWhiteSpace (filename))
            {
        		var show = new TvShowFile { Title = FindTitle (filename) };
        		if (!string.IsNullOrWhiteSpace (show.Title))
                {
        			var info = FindSeasonAndEpisode (filename);
        			if (info == null)
        				return null;
					
                    show.Season = info.Item1;
                    show.Episode = info.Item2;
                    if (!string.IsNullOrWhiteSpace(show.Info))
                    {
                        return show;
                    }
                }
            }
            return null;
        }

        public static string FindTitle(string str)
        {
            var m = new Regex(@"([a-zA-Z]+[\s|\.])*").Match(str);
            if (!m.Success)
                return null;

            var sb = new StringBuilder();
            foreach (var t in m.Value.Replace(" ", ".").Split('.'))
            {
                sb.Append(t.Capitalize());
                sb.Append('.');
            }
            return sb.ToString().Replace("..", ".");
        }

        public static Tuple<int,int> FindSeasonAndEpisode(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            // Title 0 x 00.avi | Title 0x00.avi
            var m = new Regex(@"(\d+)\s*x\s*(\d+)", RegexOptions.IgnoreCase).Match(str);
            if (m.Success)
            {
                return new Tuple<int, int>(GroupToInt(m.Groups[1]), GroupToInt(m.Groups[2]));
            }

            // Title S00xE00.avi | Title S00E00.avi
            m = new Regex(@"s(0?\d+)[x]*[e](\d{1,2})", RegexOptions.IgnoreCase).Match(str);
            if (m.Success)
            {
                return new Tuple<int, int>(GroupToInt(m.Groups[1]), GroupToInt(m.Groups[2]));
            }

            // Title 00E00.avi
            m = new Regex(@"(0?\d+)[x]*[e](\d{1,2})", RegexOptions.IgnoreCase).Match(str);
            if (m.Success)
            {
                return new Tuple<int, int>(GroupToInt(m.Groups[1]), GroupToInt(m.Groups[2]));
            }
            return null;
        }

        private static int GroupToInt(Group g)
        {
            int value;
            if (int.TryParse(g.ToString(), out value))
            {
                return value;
            }
            return -1;
        }
    }
}
