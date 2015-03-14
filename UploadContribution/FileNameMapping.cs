using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace UploadContribution
{
    //class NoVersionNameMapping : IFileNameMapping
    //{
    //    /// <summary>
    //    /// Non MSI file has no version and also has the last as the filename 
    //    /// for example, BackupReporter_x86_EN_ProductDescription    BackupReporter_x86_EN
    //    /// 
    //    /// </summary>
    //    /// <param name="sourceName"></param>
    //    /// <returns></returns>
    //    public string CreateDestination(string sourceName)
    //    {
    //        sourceName = Path.GetFileNameWithoutExtension(sourceName);
    //        string destName = sourceName;

    //        int pos = destName.LastIndexOf("_");
    //        destName = destName.Substring(0, pos);


    //        //string[] parts = destName.Split('_');
    //        //parts = parts.Reverse().Skip(1).ToArray();
    //        //parts = parts.Reverse().ToArray();
    //        //// remove the last one
    //        //destName = string.Join("_", parts);
    //        return destName;
    //    }
    //}

    class FileNameMapping 
    {
        public static List<string> FolderList = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetVesion(string path)
        {
            Match match = Regex.Match(path,@"\d+(\.\d+)+", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[0].Value;
            else
                return "";
        }
        /// <summary>
        /// Ther may be modifiers
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetVersionModifier(string path)
        {
            string returnVersion = ""; 
            string versionPattern = @"\d+(\.\d+)+";
            if (!String.IsNullOrEmpty(Program.Settings.VersionModifiers))
            {
                string [] mods = Program.Settings.VersionModifiers.Split(new char [] {','});
                foreach (string m in mods)
                {
                    string pattern = string.Format("{0}_{1}", m, versionPattern);
                    Match match = Regex.Match(path, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        returnVersion = match.Groups[0].Value;
                        break;
                    }
                }
            }
            if (String.IsNullOrEmpty(returnVersion))
            {
                returnVersion = GetVesion(path);
            }
            return returnVersion;    
        }
        /// <summary>
        /// Remove the version from the string and return the destination 
        /// for example,  BackupReporter_2.0.0.14_x86_EN.msi  =>  BackupReporter_x86_EN
        /// 
        /// </summary>
        /// <param name="sourceName"></param>
        /// <returns></returns>
        public static string CreateDestination(string sourceName)
        {
            sourceName = Path.GetFileNameWithoutExtension(sourceName);
            string destName = sourceName;
            string version = GetVesion(sourceName);
            try
            {
                if (!String.IsNullOrEmpty(version))
                {
                    int loc = sourceName.IndexOf(version);
                    destName = sourceName.Remove(loc, version.Length + 1);  // 
                }
                else
                {
                    int pos = destName.LastIndexOf("_");
                    if (pos > 0)
                        destName = destName.Substring(0, pos);
                }
            }
            catch (Exception)
            { }
            return destName;
        }

        /// <summary>
        /// Verify against the known list of remote folders
        /// </summary>
        /// <param name="destPath"></param>
        /// <returns></returns>
        public static string VerifyDestinationPath(string destPath)
        {
            return FolderList.Find(x => String.Compare(x, destPath, true) == 0 );
        }
    }

    
   
}
