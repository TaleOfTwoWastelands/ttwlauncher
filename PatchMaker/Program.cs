using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using org.foesmm.libBSA;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using deltaq;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using NDesk.Options;
using System.Windows.Forms;


namespace com.taleoftwowastelands.patchmaker
{
    class Program
    {
        private static readonly Queue<Action> patchQueue = new Queue<Action>();
        private static bool _suspendWorkers = false;
        private static bool _verbose = false;
        private static bool _skipEsm = false;
        private static bool _skipBsa = false;

        private static readonly XmlWriterSettings XmlSettings = new XmlWriterSettings()
        {
            Indent = true,
            NewLineHandling = NewLineHandling.Entitize,
            CloseOutput = true
        };

        private static void WorkerRoutine()
        {
            do
            {
                if (_suspendWorkers)
                {
                    Console.Write("Waiting for workers to stop ... {0}\r", patchQueue.Count);
                }
                Action action = null;
                lock (patchQueue)
                {
                    if (patchQueue.Count > 0)
                    {
                        action = patchQueue.Dequeue();
                    }
                }

                if (action != null)
                {
                    action();
                }
                else
                {
                    Thread.Sleep(250);
                }

            } while (patchQueue.Count > 0 || !_suspendWorkers);
        }

        static void Main(string[] args)
        {
            var localPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var settingsFile = Path.Combine(localPath, "settings.xml");

            Settings settings = null;

            // DEBUG SHIT STARTS HERE!

            //DebugMethod();

            // DEBUG SHIT ENDS HERE!

            if (!Installation.PreInstallationCheck())
            {
                DialogResult result = MessageBox.Show("Looks like you dont have everything you need to install TTW, Make sure you have NVSE installed and up to date, and all the DLC for FNV AND FO3!", "Uh oh! Something isn't right!", MessageBoxButtons.OK);
                if (!result.Equals(DialogResult.None))
                {
                    Application.Exit();
                }
            }

            if (!File.Exists(settingsFile))
            {
                CreateDefaultSettings(settingsFile);
            }
            else
            {
                using (var xmlReader = new StreamReader(settingsFile))
                {
                    var serializer = new XmlSerializer(typeof(Settings));
                    try
                    {
                        settings = (Settings)serializer.Deserialize(xmlReader);
                    }
                    catch (InvalidOperationException ioe)
                    {
                        xmlReader.Close();
                        CreateDefaultSettings(settingsFile);
                    }
                }
            }

            var p = new OptionSet()
            {
                {"skip-esm", "Skip generation of ESM patches", v => { _skipEsm = v != null; } },
                {"skip-bsa", "Skip generation of BSA patches", v => { _skipBsa = v != null; } },
                {"v", "verbosity", v => { _verbose = v != null; }}
            };

            var extra = p.Parse(args);

            var createPatch = Stopwatch.StartNew();
            Console.WriteLine("Preparing TTW v{0} recipe...\n", settings.Version);
            settings.PatchPath = Directory.CreateDirectory(Path.Combine(settings.PatchPath, settings.Version)).FullName;
            Directory.CreateDirectory(settings.OutputPath);
            Directory.CreateDirectory(settings.TempPath);

            Console.Write("Spawning {0} workers ... ", Environment.ProcessorCount);
            var workers = new List<Thread>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                Thread thread = new Thread(WorkerRoutine) { Priority = ThreadPriority.BelowNormal };
                thread.Start();
                workers.Add(thread);
            }
            Console.WriteLine("Done");

            var recipe = new Recipe();
            //using (var xmlReader = new StreamReader(string.Format("{0}\\recipe.xml", settings.PatchPath)))
            //{
            //    var serializer = new XmlSerializer(typeof(Recipe));
            //    recipe = (Recipe)serializer.Deserialize(xmlReader);
            //}
            recipe.VersionTo = settings.Version;
            if (!_skipEsm)
            {
                ProcessESMs(recipe, settings);
            }
            if (!_skipBsa)
            {
                ProcessBSAs(recipe, settings);
            }

            _suspendWorkers = true;
            Console.Write("Waiting for workers to stop ... \r");
            foreach (var worker in workers)
            {
                worker.Join();
            }
            Console.WriteLine("Waiting for workers to stop ... Done               \n");
            _suspendWorkers = false;

            Console.Write("Spawning {0} workers ... ", Environment.ProcessorCount);
            workers = new List<Thread>();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                Thread thread = new Thread(WorkerRoutine) { Priority = ThreadPriority.BelowNormal };
                thread.Start();
                workers.Add(thread);
            }
            Console.WriteLine("Done");
            var assetsDir = new DirectoryInfo(settings.LooseAssets);
            Parallel.ForEach(assetsDir.EnumerateFiles("ttw.name.txt", SearchOption.AllDirectories),
                new ParallelOptions {MaxDegreeOfParallelism = Environment.ProcessorCount},
                file =>
                {
                    CreateTTWBSA(recipe, settings, file.Directory, File.ReadAllText(file.FullName));
                });
            _suspendWorkers = true;
            Console.Write("Waiting for workers to stop ... \r");
            foreach (var worker in workers)
            {
                worker.Join();
            }
            Console.WriteLine("Waiting for workers to stop ... Done               \n");
            _suspendWorkers = false;
            FlushRecipe(settings, recipe);

            CreatePatchAtlas(recipe, settings);
            //CopyAssets(recipe, settings);
            //PackAssets(recipe, settings);
            //PackRecipe(recipe, settings);

            FlushRecipe(settings, recipe);

            createPatch.Stop();
            Console.Write("All finished. {0}", createPatch.Elapsed);
            //var bsas = new DirectoryInfo(WORKDIR_FO3).GetFiles("*.bsa").OrderBy(f => f.Length);
            //Parallel.ForEach(bsas, new ParallelOptions { MaxDegreeOfParallelism = 2 }, fi =>
            //{
            //    bool prefixed = false;
            //    Console.WriteLine("Processing file: {0} ...", fi.Name);
            //    var stopwatch = Stopwatch.StartNew();
            //    using (var archive = BSA.Open(fi.FullName))
            //    {
            //        archive.WriteDescriptor();
            //        prefixed = archive.IsNamePrefixedToData;

            //        foreach (var folder in archive.Folders.Values)
            //        {
            //            foreach (Fo3File file in folder.Files.Values)
            //            {
            //                byte[] buffer = file.GetData(archive.Reader);
            //                var dir = Directory.CreateDirectory(WORKDIR_TEMP + "\\" + fi.Name + "\\" + folder.Name);
            //                using (var fs = new FileStream(WORKDIR_TEMP + "\\" + fi.Name + "\\" + folder.Name + "\\" + file.Name, FileMode.Create, FileAccess.Write))
            //                {
            //                    fs.Write(buffer, 0, buffer.Length);
            //                }

            //            }
            //        }

            //    }
            //    stopwatch.Stop();
            //    Console.WriteLine("Files unpacked. {0}", stopwatch.Elapsed);
            //    //Console.ReadLine();
            //    Console.WriteLine("Packing");
            //    stopwatch = Stopwatch.StartNew();

            //    var newBsa = BSA.Create(BSA.Game.FalloutNewVegas, WORKDIR_ROOT + "\\packed\\" + fi.Name, WORKDIR_TEMP + "\\" + fi.Name);
            //    newBsa.IsNamePrefixedToData = prefixed;
            //    newBsa.Save();

            //    Console.WriteLine("Files packed. {0}", stopwatch.Elapsed);
            //    Directory.Delete(WORKDIR_TEMP + "\\" + fi.Name, true);
            //    //Console.ReadLine();
            //});

            Console.ReadLine();
        }

        /*
        public static void DebugMethod()
        {
            Console.WriteLine("FNV Data Path: " + Installation.FNVDataPath());
            Console.WriteLine("FO3 Data Path: " + Installation.FO3DataPath());
            Console.WriteLine("Broken Steel = " + Installation.BrokenSteelCheck());
            
        }
        */

        private static void CreatePatches(Recipe recipe, Settings settings, DirectoryInfo bsaDirectory)
        {
            
        }

        private static SortedDictionary<string, FileInfo> PrepareLooseFiles(DirectoryInfo dir)
        {
            Console.Write("Building loose assets index ... ");
            var looseFiles = new SortedDictionary<string, FileInfo>();
            foreach (var fi in dir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                looseFiles.Add(fi.GetFullNameRelative(dir.FullName).ToLower(), fi);
            }
            Console.WriteLine("Done");
            return looseFiles;
        }

        private static void PackRecipe(Recipe recipe, Settings settings)
        {

        }

        private static void PackAssets(Recipe recipe, Settings settings)
        {
            Console.Write("Packing assets ... \r");

            using (var assetsPackage = new BufferedStream(new FileStream(string.Format("{0}\\package\\patch.assets", settings.PatchPath), FileMode.Create), 8 * 1024 * 1024))
            {
                var assetsPath = string.Format("{0}\\assets", settings.PatchPath);
                var assets = new DirectoryInfo(assetsPath).GetFiles("*.*", SearchOption.AllDirectories);
                var i = 0;
                foreach (var file in assets)
                {
                    Console.Write("Packing assets ... {0} of {1}\r", ++i, assets.Length);
                    using (var assetStream = new MemoryStream())
                    using (var inputStream = new FileStream(file.FullName, FileMode.Open))
                    {
                        var asset = new Patch { Name = file.GetFullNameRelative(assetsPath), Offset = assetsPackage.Position };

                        bz2portable.BZip2.BZip2.Compress(inputStream, assetStream, false, 5);

                        if (assetStream.Length >= file.Length)
                        {
                            asset.Size = file.Length;
                            asset.Compressed = false;
                            inputStream.Seek(0, SeekOrigin.Begin);
                            inputStream.CopyTo(assetsPackage);
                        }
                        else
                        {
                            asset.Size = assetStream.Length;
                            asset.Compressed = true;
                            assetStream.Seek(0, SeekOrigin.Begin);
                            assetStream.CopyTo(assetsPackage);
                        }

                        recipe.Assets.Add(asset);
                    }
                }
            }

            Console.WriteLine("Done");
        }

        private static void CopyAssets(Recipe recipe, Settings settings)
        {
            Console.Write("Copying assets ... \r");

            var assetsDirectory = string.Format("{0}\\assets", settings.PatchPath);
            Directory.CreateDirectory(assetsDirectory);

            var total = recipe.Files.Sum(act => act.Actions.Count(a => a.Type == ActionType.New));
            var i = 0;

            foreach (var filepair in recipe.Files)
            {
                foreach (var action in filepair.Actions)
                {
                    if (action.Type == ActionType.New)
                    {
                        var fileName = string.Format("{0}\\{1}", action.Path, action.Name);
                        var sourceFile = Path.Combine(settings.LooseAssets, fileName);
                        if (!File.Exists(sourceFile))
                        {
                            var referenceFile = string.Format("{0}\\{1}", settings.CurrentDataPath, filepair.ResultName);
                            var referenceDir = Path.ChangeExtension(referenceFile, null);
                            sourceFile = Path.Combine(referenceDir, fileName);
                        }
                        Directory.CreateDirectory(Path.Combine(assetsDirectory, action.Path));
                        File.Copy(sourceFile, Path.Combine(assetsDirectory, fileName), true);
                        Console.Write("Copying assets ... {0}/{1}\r", ++i, total);
                    }
                }
            }

            Console.WriteLine("Copying assets ... Done              ");
        }

        private static void FlushRecipe(Settings settings, Recipe recipe)
        {
            using (var writer = XmlWriter.Create(new StreamWriter(string.Format("{0}\\recipe.xml", settings.PatchPath)), XmlSettings))
            {
                var serializer = new XmlSerializer(typeof(Recipe));
                serializer.Serialize(writer, recipe);
            }
        }

        private static void ProcessESMs(Recipe recipe, Settings settings)
        {
            Console.WriteLine("Processing ESMs ...");
            foreach (var filepair in settings.Files.Where(fp => fp.Type == ContentType.ESP))
            {
                Console.Write("{0} => {1} ... ", filepair.OriginalName, filepair.ResultName);

                string patchName;
                string patchFile;

                // @todo: memory stream and name with md5
                using (var patchStream = new MemoryStream())
                {
                    var originalBytes = File.ReadAllBytes(string.Format("{0}\\{1}", settings.Fo3DataPath, filepair.OriginalName));
                    var resultBytes = File.ReadAllBytes(string.Format("{0}\\{1}", settings.CurrentDataPath, filepair.ResultName));

                    BsDiff.Create(originalBytes, resultBytes, patchStream);

                    patchStream.Seek(0, SeekOrigin.Begin);
                    patchName = new Guid(MD5.Create().ComputeHash(patchStream)).ToString();
                    patchFile = string.Format("{0}\\{1}.patch", settings.GetTempFolder("Patches"), patchName);

                    if (!File.Exists(patchFile))
                    {
                        using (var fileStream = new FileStream(patchFile, FileMode.CreateNew))
                        {
                            patchStream.Seek(0, SeekOrigin.Begin);
                            patchStream.CopyTo(fileStream);
                        }
                    }
                }

                filepair.PatchName = patchName;
                recipe.Files.Add(filepair);

                Console.WriteLine("Done");
                FlushRecipe(settings, recipe);
            }
            Console.WriteLine("ESMs completed");
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        private static void ProcessBSAs(Recipe recipe, Settings settings)
        {
            Console.WriteLine("Processing BSAs ...");
            foreach (var filepair in settings.Files.Where(fp => fp.Type == ContentType.BSA))
            {
                Console.Write("{0} => {1} .", filepair.OriginalName, filepair.ResultName);

                var before = GC.GetTotalMemory(false);
                var original = BSA.Open(string.Format("{0}\\{1}", settings.Fo3DataPath, filepair.OriginalName));
                original.BuildIndex();
                var after = GC.GetTotalMemory(false);

                var referenceFile = string.Format("{0}\\{1}", settings.CurrentDataPath, filepair.ResultName);
                var referenceDir = new DirectoryInfo(Path.ChangeExtension(referenceFile, null));

                Console.Write(".");
                SortedDictionary<string, Fo3File> reference = new SortedDictionary<string, Fo3File>();
                Parallel.ForEach(referenceDir.EnumerateFiles("*.*", SearchOption.AllDirectories).AsParallel(), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, file =>
                {
                    var assetFile = new Fo3File(file) { Path = file.GetPathRelative(referenceDir.FullName).ToLower() };
                    lock (reference)
                    {
                        reference.Add(file.GetFullNameRelative(referenceDir.FullName).ToLower(), assetFile);
                    }
                });
                Console.Write(". ");

                Parallel.ForEach(reference, new ParallelOptions { MaxDegreeOfParallelism = 1 /* Environment.ProcessorCount */ }, record =>
                {
                    Debug.Write(record.Key);
                    if (original.IndexFullPath.ContainsKey(record.Key))
                    {
                        if (!ByteArrayCompare(record.Value.Checksum, original.IndexFullPath[record.Key].Checksum))
                        {
                            var originalRecord = original.IndexFullPath[record.Key];
                            var resultRecord = record.Value;

                            var recordAction = new RecordAction { Path = record.Value.Path, Name = record.Value.Name, Type = ActionType.Copy | ActionType.Patch };
                            patchQueue.Enqueue(() => CreateAssetPatch(settings, original, originalRecord, resultRecord, recordAction));
                            filepair.Actions.Add(recordAction);

                            Debug.WriteLine(" patch");
                        }
                        else
                        {
                            filepair.Actions.Add(new RecordAction { Path = record.Value.Path, Name = record.Value.Name, Type = ActionType.Copy });
                            Debug.WriteLine(" copy");
                        }
                        original.IndexFullPath.Remove(record.Key);
                    }
                    else if (original.IndexFileName.ContainsKey(BitConverter.ToString(record.Value.Checksum)))
                    {
                        var originalRecord = original.IndexFileName[BitConverter.ToString(record.Value.Checksum)];
                        if (original.IndexFullPath.ContainsKey(record.Key))
                        {
                            original.IndexFullPath.Remove(record.Key);
                        }

                        filepair.Actions.Add(new RecordAction { Path = originalRecord.Path, Name = originalRecord.Name, Type = ActionType.Rename, NewPath = record.Value.Path, NewName = record.Value.Name });
                        Debug.WriteLine(" rename");
                    }
                    else
                    {
                        filepair.Actions.Add(new RecordAction { Path = record.Value.Path, Name = record.Value.Name, Type = ActionType.New });
                        Debug.WriteLine(" new");
                    }

                    Console.Write("\r{0} => {1} ... {2}/{3}", filepair.OriginalName, filepair.ResultName, filepair.Actions.Count, reference.Count);
                });

                var i = 0;
                foreach (var newRecord in original.IndexFullPath)
                {
                    filepair.Actions.Add(new RecordAction { Path = newRecord.Value.Path, Name = newRecord.Value.Name, Type = ActionType.Delete });
                    Console.Write("\r{0} => {1} ... D{2}/{3}               ", filepair.OriginalName, filepair.ResultName, ++i, original.IndexFullPath.Count);
                    Debug.WriteLine(newRecord.Key);
                }

                recipe.Files.Add(filepair);

                Console.Write("\r{0} => {1} ... ", filepair.OriginalName, filepair.ResultName);
                Console.WriteLine("Done                               ");
#if DEBUG
                FlushRecipe(settings, recipe);
#endif
            }
            Console.WriteLine("BSAs completed");
        }

        private static void CreateAssetPatch(Settings settings, IBSA original, Fo3File originalRecord, Fo3File resultRecord, RecordAction recordAction)
        {
            string patchName;
            string patchFile;

            // @todo: memory stream and name with md5
            using (var patchStream = new MemoryStream())
            {
                // @todo: lock
                byte[] originalRecordBytes;
                if (original != null)
                {
                    lock (original)
                    {
                        originalRecordBytes = originalRecord.GetData(original.Reader);
                    }
                }
                else
                {
                    originalRecordBytes = originalRecord.GetData();
                }
                var referenceBytes = resultRecord.GetData();
                BsDiff.Create(originalRecordBytes, referenceBytes, patchStream);

                patchStream.Seek(0, SeekOrigin.Begin);
                patchName = new Guid(MD5.Create().ComputeHash(patchStream)).ToString();
                recordAction.PatchName = patchName;
                patchFile = string.Format("{0}\\{1}.patch", settings.GetTempFolder("Patches"), patchName);

                try
                {
                    using (var fileStream = new FileStream(patchFile, FileMode.CreateNew))
                    {
                        patchStream.Seek(0, SeekOrigin.Begin);
                        patchStream.CopyTo(fileStream);
                    }
                }
                catch (IOException exc)
                {
                    // ignored
                }
            }
        }

        private static void CreateTTWBSA(Recipe recipe, Settings settings, DirectoryInfo assetsDir, string name)
        {
            Console.Write("Creating {0} ... ", name);
            var filepair = new FilePair()
            {
                ResultName = name,
                Type = ContentType.BSA,
            };

            var looseFiles = PrepareLooseFiles(assetsDir);
            var patchFile = Path.Combine(assetsDir.FullName, "ttw.patch.txt");
            SortedDictionary<string, Tuple<string, string, string>> patches = null;
            if (File.Exists(patchFile))
            {
                patches = ReadPatchFile(patchFile);
            }

            foreach (var file in looseFiles)
            {
                if (patches != null && patches.ContainsKey(file.Key))
                {
                    var patch = patches[file.Key];
                    var recordAction = new RecordAction
                    {
                        Origin = patch.Item1,
                        Path = patch.Item2.Substring(0, patch.Item2.LastIndexOf("\\", StringComparison.Ordinal)),
                        Name = patch.Item2.Substring(patch.Item2.LastIndexOf("\\", StringComparison.Ordinal) + 1),
                        NewPath = patch.Item3.Substring(0, patch.Item3.LastIndexOf("\\", StringComparison.Ordinal)),
                        NewName = patch.Item3.Substring(patch.Item3.LastIndexOf("\\", StringComparison.Ordinal) + 1),
                        Type = ActionType.Rename | ActionType.Patch
                    };

                    var referenceFile = $"{settings.CurrentDataPath}\\{patch.Item1}";
                    var referenceDir = new DirectoryInfo(Path.ChangeExtension(referenceFile, null));

                    var reference = new SortedDictionary<string, Fo3File>();
                    Parallel.ForEach(referenceDir.EnumerateFiles("*.*", SearchOption.AllDirectories).AsParallel(), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, asset =>
                    {
                        var assetFile = new Fo3File(asset) { Path = asset.GetPathRelative(referenceDir.FullName).ToLower() };
                        lock (reference)
                        {
                            reference.Add(asset.GetFullNameRelative(referenceDir.FullName).ToLower(), assetFile);
                        }
                    });

                    try
                    {
                        var originalRecord = reference[patch.Item2];
                        var resultRecord = new Fo3File(new FileInfo(Path.Combine(assetsDir.FullName, patch.Item3)));

                        patchQueue.Enqueue(() => CreateAssetPatch(settings, null, originalRecord, resultRecord,
                            recordAction));
                    }
                    catch (Exception e)
                    {
                        recordAction.Error = e.Message;
                    }

                    filepair.Actions.Add(recordAction);
                }
                else
                {
                    filepair.Actions.Add(new RecordAction
                    {
                        Path = file.Value.GetPathRelative(assetsDir.FullName).ToLower(),
                        Name = file.Value.Name.ToLower(),
                        Type = ActionType.New
                    });
                }
            }

            recipe.Files.Add(filepair);
            FlushRecipe(settings, recipe);
            Console.WriteLine("Done");
        }

        private static SortedDictionary<string, Tuple<string, string, string>> ReadPatchFile(string patchFile)
        {
            var result = new SortedDictionary<string, Tuple<string, string, string>>();

            foreach (var line in File.ReadAllLines(patchFile))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var components = line.Split(',');
                result.Add(components[2].ToLower(), Tuple.Create(components[0].ToLower(), components[1].ToLower(), components[2].ToLower()));
            }

            return result;
        }

        private static void CreatePatchAtlas(Recipe recipe, Settings settings)
        {
            Console.Write("Creating patch atlas ... \r");
            var patchesDirectory = new DirectoryInfo(settings.GetTempFolder("Patches"));
            var atlasFile = string.Format("{0}\\patch.atlas", settings.PatchPath);

            using (var patchAtlas = new BufferedStream(new FileStream(atlasFile, FileMode.Create), 8 * 1024 * 1024))
            {
                var patchFiles = patchesDirectory.GetFiles();
                foreach (var patchFile in patchFiles)
                {
                    recipe.Patches.Add(new Patch { Name = patchFile.Name.Replace(".patch", ""), Offset = patchAtlas.Position, Size = patchFile.Length });
                    using (var patch = new FileStream(patchFile.FullName, FileMode.Open))
                    {
                        patch.CopyTo(patchAtlas);
                    }
                    Console.Write("Creating patch atlas ... {0}/{1}\r", recipe.Patches.Count, patchFiles.Length);
                }
            }

            Console.WriteLine("Creating patch atlas ... Done                  ");
        }

        private static void CreateDefaultSettings(string settingsFile)
        {
            using (var writer = XmlWriter.Create(new StreamWriter(settingsFile), XmlSettings))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(writer, Settings.Default());
                writer.Flush();
                writer.Close();
            }

            Console.WriteLine("Settings template generated.");
            Console.Write("Press any key...");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
