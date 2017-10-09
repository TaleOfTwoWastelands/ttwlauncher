using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace com.taleoftwowastelands.patchmaker
{
    /// <summary>
    /// This class deals with all things relating to your installation of the 2 games
    /// </summary>
    public class Installation
    {

        /// <summary>
        /// Returns the "Bethesda Softworks" Registry key
        /// </summary>
        public RegistryKey GetBethesdaKey()
        {
            // according to the last installer 64bit makes a difference so....
            return (Registry.LocalMachine.OpenSubKey(Environment.Is64BitOperatingSystem ? "Software\\WOW6432Node" : "Software", RegistryKeyPermissionCheck.ReadWriteSubTree)).CreateSubKey("Bethesda Softworks", RegistryKeyPermissionCheck.ReadWriteSubTree);
        }

        /// <summary>
        /// Uses GetBethesdaKey() to get the path for either Fallout New Vegas, or Fallout 3
        /// </summary>
        /// <param name="FNV">Set to true to use the method for Fallout New Vegas, false for Fallout 3</param>
        public string PathFinder(bool FNV)
        {
            // does this if being done for FNV
            if(FNV) {
                return GetBethesdaKey().CreateSubKey("falloutnv").GetValue("installed path").ToString();
            }
            else // does this if being done for fo3
            {
                return GetBethesdaKey().CreateSubKey("fallout3").GetValue("installed path").ToString();
            }
        }

        /// <summary>
        /// Checks if the user has NVSE installed, if they dont, it alerts them
        /// </summary>
        /// <returns>Returns true if the user has NVSE installed, false if they don't.</returns>
        public bool NVSECheck()
        {
            if (!File.Exists(PathFinder(true) + "nvse_loader.exe"))
            {
                MessageBox.Show("Looks like you don't have NVSE, you should really get it if you want things to work.", "WARNING", MessageBoxButtons.OK);
                return false;
            }else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks the version number of the NVSE loader
        /// lastUpdateDate up to date as of 10/6/2017
        /// </summary>
        /// <returns>Returns true if NVSE is up to date *enough*</returns>
        public bool NVSEVersCheck()
        {
            // long lastUpdateDate = 131421081690000000; // Friday, June 16, 2017 5:36:09pm
            // if (File.GetLastWriteTimeUtc(PathFinder(true) + "nvse_1_4.dll") < DateTime.FromFileTimeUtc(lastUpdateDate))
            
            Version version = Version.Parse((FileVersionInfo.GetVersionInfo(PathFinder(true) + "nvse_loader.exe").FileVersion.Replace(',','.')));
            if (version < Version.Parse("0.5.1.2"))
            {
                Console.WriteLine("[Installation.cs] NVSE is up to date for this version of TTW");
                return true;
            }else
            {
                Console.WriteLine("[Installation.cs] NVSE is NOT up to date for this version of TTW");
                return false;
            }

            
        }
    }
}
