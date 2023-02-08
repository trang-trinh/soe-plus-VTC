using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Shared
{
    class Utils
    {
        public string FileName { get; set; }
        public string TempFolder { get; set; }
        public int MaxFileSizeMB { get; set; }
        public List<String> FileParts { get; set; }

        public Utils()
        {
            FileParts = new List<string>();
        }

        public bool MergeFile(string FileName)
        {
            bool rslt = false;
            string partToken = ".part_";
            string baseFileName = FileName.Substring(0, FileName.IndexOf(partToken));
            string trailingTokens = FileName.Substring(FileName.IndexOf(partToken) + partToken.Length);
            int FileIndex = 0;
            int FileCount = 0;
            int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
            int.TryParse(trailingTokens.Substring(trailingTokens.IndexOf(".") + 1), out FileCount);
            string Searchpattern = Path.GetFileName(baseFileName) + partToken + "*";
            string[] FilesList = Directory.GetFiles(Path.GetDirectoryName(FileName), Searchpattern);
            if (FilesList.Count() == FileCount)
            {
                if (!MergeFileManager.Instance.InUse(baseFileName))
                {
                    MergeFileManager.Instance.AddFile(baseFileName);

                    var fileNameTemp = Regex.Replace(baseFileName.Replace("\\", "/"), @"\.*/+", "/");
                    var listPath = fileNameTemp.Split('/');
                    var pathConfig = "";
                    foreach (var item in listPath)
                    {
                        if (item.Trim() != "")
                        {
                            pathConfig += "/" + Path.GetFileName(item);
                        }
                    }
                    baseFileName = pathConfig.Substring(1);
                    if (File.Exists(baseFileName))
                    {
                        File.Delete(baseFileName);
                    }

                    List<SortedFile> MergeList = new List<SortedFile>();
                    foreach (string File in FilesList)
                    {
                        SortedFile sFile = new SortedFile();
                        sFile.FileName = File;
                        baseFileName = File.Substring(0, File.IndexOf(partToken));
                        var fileInfo = new FileInfo(baseFileName);
                        trailingTokens = File.Substring(File.IndexOf(partToken) + partToken.Length);
                        int.TryParse(trailingTokens.Substring(0, trailingTokens.IndexOf(".")), out FileIndex);
                        sFile.FileOrder = FileIndex;
                        MergeList.Add(sFile);
                    }
                    // sort by the file-part number to ensure we merge back in the correct order
                    var MergeOrder = MergeList.OrderBy(s => s.FileOrder).ToList();
                    using (FileStream FS = new FileStream(baseFileName, FileMode.Create))
                    {
                        foreach (var chunk in MergeOrder)
                        {
                            var pathChunk = "";
                            if (chunk.FileName != null)
                            {
                                var chunkNameTemp = Regex.Replace(chunk.FileName.Replace("\\", "/"), @"\.*/+", "/");
                                var listPath_chunk = chunkNameTemp.Split('/');
                                var pathConfig_chunk = "";
                                foreach (var item in listPath_chunk)
                                {
                                    if (item.Trim() != "")
                                    {
                                        pathConfig_chunk += "/" + Path.GetFileName(item);
                                    }
                                }
                                pathChunk = pathConfig_chunk.Substring(1);
                            }
                            try
                            {
                                //using (FileStream fileChunk = new FileStream(chunk.FileName, FileMode.Open))
                                using (FileStream fileChunk = new FileStream(pathChunk, FileMode.Open))
                                {
                                    fileChunk.CopyTo(FS);
                                }
                            }
                            catch (IOException ex)
                            {
                                // handle      
                                string contents = "";

                            }
                            //System.IO.File.Delete(chunk.FileName);
                            System.IO.File.Delete(pathChunk);
                        }
                    }
                    rslt = true;
                    // unlock the file from singleton
                    MergeFileManager.Instance.RemoveFile(baseFileName);
                }
            }
            return rslt;
        }


    }

    public struct SortedFile
    {
        public int FileOrder { get; set; }
        public String FileName { get; set; }
    }

    public class MergeFileManager
    {
        private static MergeFileManager instance;
        private List<string> MergeFileList;

        private MergeFileManager()
        {
            try
            {
                MergeFileList = new List<string>();
            }
            catch { }
        }

        public static MergeFileManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MergeFileManager();
                return instance;
            }
        }

        public void AddFile(string BaseFileName)
        {
            MergeFileList.Add(BaseFileName);
        }

        public bool InUse(string BaseFileName)
        {
            return MergeFileList.Contains(BaseFileName);
        }

        public bool RemoveFile(string BaseFileName)
        {
            return MergeFileList.Remove(BaseFileName);
        }
    }

}



