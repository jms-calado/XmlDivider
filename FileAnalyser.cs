using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace XmlDivider
{
    class FileAnalyser
    {
        private void Analyser(string file, string filename)
        {
            String path = Path.GetDirectoryName(file);
            //FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            StreamReader reader = new StreamReader(file);
            
            String[] header = new String[7];
            String[] body = new String[12];
            bool eof = false;
            for (int i = 0; i < 7; i++)
            {
                try
                {
                    header[i] = reader.ReadLine();
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex);
                    eof = true;
                }
            }
            int id = 0;
            do
            {
                try
                {
                    var part = File.Create(Directory.GetCurrentDirectory() + @"/parts/" + Regex.Replace(filename, ".xml", " " + id.ToString() + ".xml"));
                    Console.WriteLine(Directory.GetCurrentDirectory() + @"/parts/" + Regex.Replace(filename, ".xml", " " + id.ToString() + ".xml"));
                    using(StreamWriter writer = new StreamWriter(part))
                    {
                        foreach (String str in header)
                        {
                            writer.WriteLine(str);
                        }
                        int j = 0;
                        while (j < 250 && !eof)
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                try
                                {
                                    String line = null;
                                    if ((line = reader.ReadLine()) != null)
                                    {
                                        if(line.Contains("</emotionml>"))
                                        {                                    
                                            eof = true;
                                            break;
                                        }
                                        writer.WriteLine(line);
                                    }
                                    else
                                    {
                                        eof = true;
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    eof = true;
                                    break;
                                }
                            }
                            j++;
                        }
                        writer.WriteLine("</emotionml>");
                        writer.Close();
                        id++;
                    }
                }
                catch(AccessViolationException ex)
                {
                    Console.WriteLine(ex);
                    eof = true;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    eof = true;
                }
            }
            while (!eof);
            reader.Close();
        }
        public void FileWatcher(String watcherFolder)
        {
            //https://msdn.microsoft.com/en-us/library/system.io.filesystemeventhandler(v=vs.110).aspx
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = watcherFolder;
            //openFileDialog1.Filter = "Xml|*.xml";
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.xml";
            //  Register a handler that gets called when a file is created
            watcher.Created += new FileSystemEventHandler(OnChanged);
            //  Register a handler that gets called if the 
            //  FileSystemWatcher needs to report an error.
            watcher.Error += new ErrorEventHandler(OnError);
            //  Begin watching.
            watcher.EnableRaisingEvents = true;
        }
        //  This method is called when a file is created, changed, or deleted.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //  Show that a file has been created, changed, or deleted.
            WatcherChangeTypes wct = e.ChangeType;
            Console.WriteLine("File {0} {1}", e.FullPath, wct.ToString());
            while (IsFileLocked(new FileInfo(e.FullPath)))
            {
                Task.Delay(1000).Wait();
                Console.WriteLine(e.FullPath.ToString() + " is locked");
            }
            if (!IsFileLocked(new FileInfo(e.FullPath)))
            {
                Task.Delay(10000).Wait();
                string filename = Path.GetFileName(e.FullPath);
                Analyser(e.FullPath, filename);
            }
        }
        //  This method is called when the FileSystemWatcher detects an error.
        private static void OnError(object source, ErrorEventArgs e)
        {
            //  Show that an error has been detected.
            Console.WriteLine("The FileSystemWatcher has detected an error");
            //  Give more information if the error is due to an internal buffer overflow.
            if (e.GetException().GetType() == typeof(System.IO.InternalBufferOverflowException))
            {
                //  This can happen if Windows is reporting many file system events quickly 
                //  and internal buffer of the  FileSystemWatcher is not large enough to handle this
                //  rate of events. The InternalBufferOverflowException error informs the application
                //  that some of the file system events are being lost.
                Console.WriteLine(("The file system watcher experienced an internal buffer overflow: " + e.GetException().Message));
            }
            Console.WriteLine(e.GetException().Message);
        }
        //Restart FileWatcher
        private static void RestartFileWatcher(FileSystemWatcher watcher)
        {

        }
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (IOException err)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                Console.WriteLine("Exception {0}", err);
                return true;
            }
            catch(UnauthorizedAccessException ex)
            {                
                Console.WriteLine("Exception {0}", ex);
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }

            //file is not locked
            return false;
        }
        public static double ConvertToUnixTimestamp(DateTime date)
        {
            //http://stackoverflow.com/questions/3354893/how-can-i-convert-a-datetime-to-the-number-of-seconds-since-1970
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalMilliseconds);
        }
    }
}