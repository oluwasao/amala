using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace PublicLogic
{
    public class SystemIOHelper
    {
        /// <summary>
        /// Create and Save file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <param name="filePath"></param>
        public static FileInfo CreateAndSaveFile(string fileContent, string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            File.WriteAllText(filePath, fileContent);
            return new FileInfo(filePath);
        }
    }
}
