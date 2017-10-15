using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public static RegistryKey GetBethesdaKey()
        {
            // according to the last installer 64bit makes a difference so....
            return (Registry.LocalMachine.OpenSubKey(Environment.Is64BitOperatingSystem ? "Software\\WOW6432Node" : "Software", false)).OpenSubKey("Bethesda Softworks", false);
        }

        /// <summary>
        /// Uses GetBethesdaKey() to get the path for either Fallout New Vegas, or Fallout 3
        /// </summary>
        /// <param name="FNV">Set to true to use the method for Fallout New Vegas, false for Fallout 3</param>
        /// <returns>If FNV is true, returns the install path of Fallout New Vegas, if false, returns the install path of Fallout 3</returns>
        public static string PathFinder(bool FNV)
        {
            // does this if being done for FNV
            if(FNV) {
                return GetBethesdaKey().OpenSubKey("falloutnv").GetValue("installed path").ToString();
            }
            else // does this if being done for fo3
            {
                return GetBethesdaKey().OpenSubKey("fallout3").GetValue("installed path").ToString();
            }
        }

        /// <summary>
        /// Checks if the user has NVSE installed, if they dont, it alerts them
        /// </summary>
        /// <returns>Returns true if the user has NVSE installed, false if they don't.</returns>
        public static bool NVSECheck()
        {
            if (!File.Exists(PathFinder(true) + "nvse_1_4.dll"))
            {
                MessageBox.Show("Looks like you don't have NVSE, you should really get it if you want things to work.", "WARNING", MessageBoxButtons.OK);
                return false;
            }else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks the version number of the NVSE loader, now based on version number! :D 
        /// lastUpdateDate up to date as of 10/6/2017
        /// </summary>
        /// <returns>Returns true if NVSE is up to date *enough*</returns>
        public static bool NVSEVersCheck()
        {
            // old way V
            // long lastUpdateDate = 131421081690000000; // Friday, June 16, 2017 5:36:09pm
            // if (File.GetLastWriteTimeUtc(PathFinder(true) + "nvse_1_4.dll") < DateTime.FromFileTimeUtc(lastUpdateDate))
            
            Version version = Version.Parse((FileVersionInfo.GetVersionInfo(PathFinder(true) + "nvse_1_4.dll").FileVersion.Replace(',','.')));
            Console.WriteLine(version);
            if (version >= Version.Parse("0.5.1.2"))
            {
                Console.WriteLine("[Installation.cs] NVSE is up to date for this version of TTW");
                return true;
            }else
            {
                Console.WriteLine("[Installation.cs] NVSE is NOT up to date for this version of TTW");
                return false;
            }

            
        }

        /// <summary>
        /// Obtains/returns the data path of the FO3 installation as a string.... IN ONE LINE!
        /// </summary>
        /// <returns>Returns the data path of the FO3 installation as a string</returns>
        public static string FO3DataPath()
        {
            return (PathFinder(false) + "Data\\");
        }

        // FO3 DLC CHECK HELPERS START HERE!

        /// <summary>
        /// Checks if Operation Anchorage is installed
        /// </summary>
        /// <returns>Whether or not Operation Anchorage is installed</returns>
        public static bool OpAnchorageCheck()
        {
            return File.Exists(FO3DataPath() + "Anchorage.esm");
        }

        /// <summary>
        /// Checks if The Pitt is installed
        /// </summary>
        /// <returns>Whether or not The Pitt is installed</returns>
        public static bool PittCheck() // hehe, that sounds funny
        {
            return File.Exists(FO3DataPath() + "ThePitt.esm");
        }

        /// <summary>
        /// Checks if Broken Steel is installed
        /// </summary>
        /// <returns>Whether or not Broken Steel is installed</returns>
        public static bool BrokenSteelCheck()
        {
            return File.Exists(FO3DataPath() + "BrokenSteel.esm");
        }

        /// <summary>
        /// Checks if Point Lookout is installed
        /// </summary>
        /// <returns>Whether or not Point Lookout is installed</returns>
        public static bool PointLookoutCheck()
        {
            return File.Exists(FO3DataPath() + "PointLookout.esm");
        }

        /// <summary>
        /// Checks if Mothership Zeta is installed
        /// </summary>
        /// <returns>Whether or not Mothership Zeta is installed</returns>
        public static bool ZetaCheck()
        {
            return File.Exists(FO3DataPath() + "Zeta.esm");
        }

        /// <summary>
        /// Checks if all the FO3 DLC is installed
        /// </summary>
        /// <returns>Whether or not all the FO3 DLC is installed</returns>
        public static bool FO3AllDLCCheck()
        {
            return OpAnchorageCheck() && PittCheck() && BrokenSteelCheck() && PointLookoutCheck() && ZetaCheck();
        }

        // FO3 DLC CHECK HELPERS END HERE!

        /// <summary>
        /// Gets the Data path for Fallout New Vegas
        /// </summary>
        /// <returns>Data path of Fallout New Vegas as a string</returns>
        public static string FNVDataPath()
        {
            return (PathFinder(true) + "Data\\");
        }

        // FNV DLC CHECK HELPERS START HERE!

        /// <summary>
        /// Checks if The bad DLC is installed
        /// </summary>
        /// <returns>Whether or not Dead Money is installed</returns>
        public static bool DMCheck()
        {
            return File.Exists(FNVDataPath() + "DeadMoney.esm");
        }

        /// <summary>
        /// Checks if GRA is installed
        /// </summary>
        /// <returns>Whether or not GRA is installed</returns>
        public static bool GRACheck()
        {
            return File.Exists(FNVDataPath() + "GunRunnersArsenal.esm");
        }

        /// <summary>
        /// Checks if Honest Hearts is installed
        /// </summary>
        /// <returns>Whether or not Honest Hearts is installed</returns>
        public static bool HHCheck()
        {
            return File.Exists(FNVDataPath() + "HonestHearts.esm");
        }

        /// <summary>
        /// Checks if Lonesome Road is installed
        /// </summary>
        /// <returns>Whether or not Lonesome Road is installed</returns>
        public static bool LRCheck()
        {
            return File.Exists(FNVDataPath() + "LonesomeRoad.esm");
        }

        /// <summary>
        /// Checks if Old World Blues is installed
        /// </summary>
        /// <returns>Whether or not Old World Blues is installed</returns>
        public static bool OWBCheck()
        {
            return File.Exists(FNVDataPath() + "OldWorldBlues.esm");
        }
        
        /// <summary>
        /// Checks if all the FNV DLC is installed
        /// </summary>
        /// <returns>Whether or not all the FNV DLC is installed</returns>
        public static bool FNVAllDLCCheck()
        {
            return DMCheck() && GRACheck() && HHCheck() && LRCheck() && OWBCheck();
        }

        // FNV DLC CHECK HELPERS END HERE!

        /// <summary>
        /// Uses all the helpers to check if the user is able to install TTW before installing
        /// </summary>
        /// <returns>Whether or not the user is able to install TTW</returns>
        public static bool PreInstallationCheck()
        {
            return NVSECheck() && NVSEVersCheck() && FNVAllDLCCheck() && FO3AllDLCCheck();
        }
        
    }
}
