using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z.Diagnostics
{
    /// <summary>
    /// LJ20160105
    /// How to:
    /// Console.SetOut(new ConSoleWriter([FilePath]));
    /// Use Console.WriteLine
    /// </summary>
    public class ConSoleWriter : TextWriter, IDisposable
    {
        private FileStream fs;
        private StreamWriter sw;
        private TextWriter orout = Console.Out;
        private string LogFile;

        public ConSoleWriter(string LogFile)
        {
            string dir = Path.GetDirectoryName(LogFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            this.fs = new FileStream(LogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            this.sw = new StreamWriter(fs);
            this.sw.AutoFlush = true;
            this.LogFile = LogFile;
        }

        public override void WriteLine(string value)
        {
            orout.WriteLine(value);
            sw.WriteLine(value);
        }

        public override Encoding Encoding
        {
            get
            {
                return null;
            }
        }

        public string Logs
        {
            get
            {
                //create copy to prevent locking
                var g = this.LogFile + ".copy";
                string h;
                File.Copy(this.LogFile, g);
                using (var f = new StreamReader(g))
                    h = f.ReadToEnd();
                File.Delete(g);
                return h;
            }
        }

        public new void Dispose()
        {
            sw?.Close();
            fs?.Dispose();
            fs?.Close();
            fs?.Dispose();

            base.Dispose();

            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
