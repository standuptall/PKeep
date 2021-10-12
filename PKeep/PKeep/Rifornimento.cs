using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PKeep
{
    public class Rifornimento
    {
        private const string FILENAME = "data.txt";
        public Guid Guid { get; set; }
        public DateTime Data { get; set; }
        public double Importo { get; set; }
        public int Tachimetro { get; set; }
        public double Costo { get; set; }
        public int Residuo { get; set; }

        internal void Save()
        {
            string row = Guid.ToString()+";"+Data.ToString("yyyyMMdd") +";"+ Importo.ToString() + ";" + Tachimetro.ToString() + ";" + Costo.ToString() + ";" + Residuo.ToString()+Environment.NewLine;
            var data = this.ReadFile(Android.App.Application.Context, FILENAME);
            var lines = data.Split(new[] { Environment.NewLine },
                                        StringSplitOptions.None);
            var newl = new List<string>();
            newl.Add(row);
            WriteFileOnInternalStorage(Android.App.Application.Context, FILENAME, string.Join(Environment.NewLine, newl));
        }

        internal void Remove()
        {
            var data = this.ReadFile(Android.App.Application.Context, FILENAME);
            var lines = data.Split(new[] { Environment.NewLine },
                                        StringSplitOptions.None);
            lines = lines.Where(c => !c.StartsWith(Guid.ToString())).ToArray();
            WriteFileOnInternalStorage(Android.App.Application.Context, FILENAME, string.Join(Environment.NewLine,lines));
        }
        public void WriteFileOnInternalStorage(Android.Content.Context mcoContext, String sFileName, String sBody)
        {
            try
            {
                System.IO.File.WriteAllText(Path.Combine(mcoContext.FilesDir.Path, sFileName), sBody); ;
            }
            catch (Exception e)
            {

            }
        }
        public string ReadFile(Android.Content.Context mcoContext, String sFileName)
        {
            try
            {
                return System.IO.File.ReadAllText(Path.Combine(mcoContext.FilesDir.Path, sFileName)); ;
            }
            catch (Exception e)
            {
                return "";
            }
        }
    }
}
