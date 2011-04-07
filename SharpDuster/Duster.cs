using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using System.Diagnostics;

namespace SharpDuster
{
    public enum ExMode
    {
        Preview,
        Normal,
        Help
    }

    public class Duster
    {
        #region Fields
		
		private static IList<string> s_suportedFiles = new List<string> { ".mkv", ".avi", ".mov", ".mp4", ".wmv"};

        private static string s_destination;

        private static int s_filesFound;

        private static ExMode s_mode = ExMode.Normal;

        private static OptionSet s_options;

        private static int s_parsedCount;

        private static int s_skipCount;

        private static string s_source;

        private static bool s_hasErrors;

        private static readonly Logger s_Logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Methods

        public static void PrintStats()
        {
            s_Logger.Info("\n\n- Parsed {0} of {1} files found, skipped {2} -", s_parsedCount, s_filesFound, s_skipCount);
        }

        #endregion

        #region Private Methods

        public static void Main (string[] args)
        {
        	ParseArgs (args);

            if (s_hasErrors)
        		return;

            switch (s_mode)
            {
        	case ExMode.Preview:
        		s_Logger.Info ("** Preview mode: no files will be altered **\n");
        		Run ();
        		break;
        	case ExMode.Normal:
        		Run ();
        		break;

                default:
        		ShowHelp ();
        		break;
        	}
        	Type t = Type.GetType ("Mono.Runtime");
        	if (t != null) 
			{
        		Console.WriteLine ("Curse you mono + NLog!");
				Process.GetCurrentProcess ().CloseMainWindow ();
			}
        	
        }

        private static void ShowHelp()
        {
            using (var writer = new StringWriter())
            {
                s_Logger.Info("Usage: SharpDuster [arguments]");
                s_options.WriteOptionDescriptions(writer);
                s_Logger.Info(writer.ToString);
            }
        }

        private static void ParseArgs(IEnumerable<string> args)
        {
            s_options = new OptionSet
                {
                    {"p", "Preview mode", v => s_mode = ExMode.Preview},
                    {"s=","source folder", v=> s_source = v},
                    {"d=","destination folder", v=> s_destination = v },
                    {"h|?|help", "Display Help menu", v=> s_mode = ExMode.Help},
                };

            try
            {
                s_options.Parse(args);
            }
            catch (OptionException e)
            {
                s_Logger.LogException(LogLevel.Error, e.Message, e);
                s_hasErrors = true;
            }
        }

        private static void RenameAndMoveFile(FileInfo file, TvShowFile tvShowFile)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            if (tvShowFile == null)
            {
                throw new ArgumentNullException("tvShowFile");
            }

            try
            {
                string dir = FindDestinationFolder(tvShowFile);
                if (string.IsNullOrWhiteSpace(dir))
                {
                    s_Logger.Error("\tFailed - unable to find destionation folder '{0}'", dir);
                    return;
                }
                File.Move(file.FullName, string.Format("{0}{1}{2}", dir, Path.DirectorySeparatorChar, tvShowFile.Filename));
            }
            catch (Exception e)
            {
                s_Logger.LogException(LogLevel.Error,"",e);
                return;
            }
            s_Logger.Info("\tcleaned and moved");
        }

        private static void Run ()
        {
        	try
            {
        		if (string.IsNullOrWhiteSpace (s_source))
        			s_source = Directory.GetCurrentDirectory ();

                if (!ValidFolders ())
        			return;
        		var files = Directory.EnumerateFiles (s_source, "*.*", SearchOption.AllDirectories)
					                 .Select (s => new FileInfo (s))
						             .Where(f => s_suportedFiles.Contains(f.Extension));
                
                s_filesFound = files.Count();
                foreach (var f in files)
                {
                    DustFile(f);
                }
            }
            catch (Exception e)
            {
                s_Logger.LogException(LogLevel.Error, "Something went wrong!", e);
            }
            PrintStats();
        }

        private static bool ValidFolders()
        {
            if (!Directory.Exists(s_source))
            {
                s_Logger.Error("Unable to find source folder '{0}'. Please make sure it exists", s_source);
                return false;
            }
            if (string.IsNullOrWhiteSpace(s_destination))
            {
                s_destination = s_source;
            }
            else
            {
                if (!Directory.Exists(s_destination))
                {
                    s_Logger.Error("Unable to find destination folder '{0}'. Please make sure it exists", s_destination);
                    return false;
                }
            }
            return true;
        }

        private static void DustFile (FileInfo file)
        {
        	if (file == null)
            {
        		throw new ArgumentNullException ("file");
        	}
        	
            s_Logger.Info ("Dusting {0}", file.Name);
        	TvShowFile tvShowFile = FileSanitizer.Sanitize (file.Name);
        	if (tvShowFile != null)
            {
        		tvShowFile.Extension = file.Extension;

                if (file.Name == tvShowFile.Filename)
                {
        			s_Logger.Info ("\talready clean");
        		}
                else
                {
        			s_Logger.Info ("\t{0}", tvShowFile.Filename);
        		}
				if (s_mode == ExMode.Preview)
                {
        			s_Logger.Info ("\t...");
        		}
                else
                {
        			s_parsedCount++;
        			RenameAndMoveFile (file, tvShowFile);
        		}
        	}
            else
            {
        		s_skipCount++;
        		s_Logger.Info ("*** Skipping: unable to sanitize");
			}            
		}

        private static string FindDestinationFolder(TvShowFile show)
        {
            if (show != null && !string.IsNullOrWhiteSpace(show.FolderName) && show.Season > 0)
            {
                try
                {
                    string path = Path.Combine(s_destination, show.FolderName);
                    CreatePath(path);
                    string subFolderPath = Path.Combine(path, string.Format("Season {0}", show.Season));
                    CreatePath(subFolderPath);
                    return subFolderPath;
                }
                catch (Exception e)
                {
                    s_Logger.LogException(LogLevel.Error, "", e);
                }
            }
            return null;
        }

        private static void CreatePath(string path)
        {
            if (Directory.Exists(path))
                return;

            s_Logger.Info("\tcreating folder for '{0}'", path);
            Directory.CreateDirectory(path);
        }

        #endregion
    }
}
