using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using org.foesmm.libbsa;
using System.Diagnostics;

namespace PatchMaker
{
    class Program
    {
        const string WORKDIR_ROOT = "D:\\Work\\TTW\\Test";
        const string WORKDIR_FO3 = WORKDIR_ROOT + "\\fo3";
        const string WORKDIR_TTW = WORKDIR_ROOT + "\\ttw";
        const string WORKDIR_TEMP = WORKDIR_ROOT + "\\temp";
        const string WORKDIR_ASSETS = WORKDIR_ROOT + "\\assets";

        static void Main(string[] args)
        {
            var bsas = Directory.GetFiles(WORKDIR_FO3, "*.bsa");
            foreach (var bsa in bsas)
            {
                var fi = new FileInfo(bsa);
                bool prefixed = false;
                Console.WriteLine("Processing file: {0} ...", fi.Name);
                var stopwatch = Stopwatch.StartNew();
                using (var archive = BSArchive.Open(bsa))
                {
                    archive.WriteDescriptor();
                    prefixed = archive.IsNamePrefixedToData;
                    /*
                    foreach (var record in archive.Folders)
                    {
                        var folder = (Fo3Folder)record.Value;
                        foreach (Fo3File file in folder.Files.Values)
                        {
                            byte[] buffer = file.GetData();
                            var dir = Directory.CreateDirectory(WORKDIR_TEMP + "\\" + folder.Name);
                            using (var fs = new FileStream(WORKDIR_TEMP + "\\" + folder.Name + "\\" + file.Name, FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(buffer, 0, buffer.Length);
                            }

                        }
                    }*/
                    //archive.WriteDescriptor();
                }
                stopwatch.Stop();
                Console.WriteLine("Files unpacked. {0}", stopwatch.Elapsed);
                //Console.ReadLine();
                Console.WriteLine("Packing");
                stopwatch = Stopwatch.StartNew();
                /*
                var newBsa = BSArchive.Create(BSArchive.Version.FalloutNewVegas, WORKDIR_ROOT + "\\packed\\" + fi.Name, WORKDIR_TEMP);
                newBsa.IsNamePrefixedToData = prefixed;
                newBsa.Save();
                */
                Console.WriteLine("Files packed. {0}", stopwatch.Elapsed);
                //Directory.Delete(WORKDIR_TEMP, true);
                //Console.ReadLine();
            }

            Console.Write("All finished");
            Console.ReadLine();
        }
    }
}
