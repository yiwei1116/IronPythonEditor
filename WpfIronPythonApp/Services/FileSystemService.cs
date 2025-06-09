using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using WpfIronPythonApp.Services.ApiRegistry;

namespace WpfIronPythonApp.Services
{
    /// <summary>
    /// 檔案系統服務，提供檔案和目錄操作功能
    /// </summary>
    [ApiService("fs", Description = "檔案系統服務，提供檔案和目錄操作功能", Version = "1.0.0")]
    public class FileSystemService
    {
        /// <summary>
        /// 讀取文字檔案內容
        /// </summary>
        [ApiMethod(Description = "讀取文字檔案的完整內容", 
                   Example = "content = fs.read_text('readme.txt')", 
                   Category = "File Operations", 
                   Permission = ApiPermission.FileAccess)]
        public string read_text([ApiParameter(Description = "檔案路徑", Example = "'C:/temp/file.txt'")] string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"檔案不存在: {filePath}");

                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"讀取檔案失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 寫入文字到檔案
        /// </summary>
        [ApiMethod(Description = "將文字內容寫入檔案", 
                   Example = "fs.write_text('output.txt', 'Hello World!')", 
                   Category = "File Operations", 
                   Permission = ApiPermission.FileAccess)]
        public void write_text([ApiParameter(Description = "檔案路徑", Example = "'C:/temp/output.txt'")] string filePath,
                              [ApiParameter(Description = "要寫入的文字內容", Example = "'Hello World!'")] string content)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, content);
                LoggingService.Instance.LogInfo($"已寫入檔案: {filePath}");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"寫入檔案失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 檢查檔案是否存在
        /// </summary>
        [ApiMethod(Description = "檢查指定路徑的檔案是否存在", 
                   Example = "if fs.file_exists('data.txt'):\n    print('File found!')", 
                   Category = "File Operations")]
        public bool file_exists([ApiParameter(Description = "檔案路徑", Example = "'C:/temp/file.txt'")] string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 檢查目錄是否存在
        /// </summary>
        [ApiMethod(Description = "檢查指定路徑的目錄是否存在", 
                   Example = "if fs.dir_exists('C:/temp'):\n    print('Directory found!')", 
                   Category = "Directory Operations")]
        public bool dir_exists([ApiParameter(Description = "目錄路徑", Example = "'C:/temp'")] string dirPath)
        {
            return Directory.Exists(dirPath);
        }

        /// <summary>
        /// 創建目錄
        /// </summary>
        [ApiMethod(Description = "創建目錄（如果不存在）", 
                   Example = "fs.create_dir('C:/temp/new_folder')", 
                   Category = "Directory Operations", 
                   Permission = ApiPermission.FileAccess)]
        public void create_dir([ApiParameter(Description = "目錄路徑", Example = "'C:/temp/new_folder'")] string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                    LoggingService.Instance.LogInfo($"已創建目錄: {dirPath}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"創建目錄失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 列出目錄中的檔案
        /// </summary>
        [ApiMethod(Description = "列出指定目錄中的所有檔案", 
                   Example = "files = fs.list_files('C:/temp')\nfor file in files:\n    print(file)", 
                   Category = "Directory Operations")]
        public List<string> list_files([ApiParameter(Description = "目錄路徑", Example = "'C:/temp'")] string dirPath,
                                      [ApiParameter(Description = "檔案篩選模式", Example = "'*.txt'", IsOptional = true)] string pattern = "*")
        {
            try
            {
                if (!Directory.Exists(dirPath))
                    throw new DirectoryNotFoundException($"目錄不存在: {dirPath}");

                return Directory.GetFiles(dirPath, pattern).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"列出檔案失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 列出目錄中的子目錄
        /// </summary>
        [ApiMethod(Description = "列出指定目錄中的所有子目錄", 
                   Example = "dirs = fs.list_dirs('C:/temp')\nfor dir in dirs:\n    print(dir)", 
                   Category = "Directory Operations")]
        public List<string> list_dirs([ApiParameter(Description = "目錄路徑", Example = "'C:/temp'")] string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                    throw new DirectoryNotFoundException($"目錄不存在: {dirPath}");

                return Directory.GetDirectories(dirPath).ToList();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"列出目錄失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 刪除檔案
        /// </summary>
        [ApiMethod(Description = "刪除指定的檔案", 
                   Example = "fs.delete_file('temp.txt')", 
                   Category = "File Operations", 
                   Permission = ApiPermission.FileAccess)]
        public void delete_file([ApiParameter(Description = "檔案路徑", Example = "'C:/temp/file.txt'")] string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LoggingService.Instance.LogInfo($"已刪除檔案: {filePath}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"刪除檔案失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 複製檔案
        /// </summary>
        [ApiMethod(Description = "複製檔案到新位置", 
                   Example = "fs.copy_file('source.txt', 'backup.txt')", 
                   Category = "File Operations", 
                   Permission = ApiPermission.FileAccess)]
        public void copy_file([ApiParameter(Description = "來源檔案路徑", Example = "'C:/temp/source.txt'")] string sourcePath,
                             [ApiParameter(Description = "目標檔案路徑", Example = "'C:/temp/backup.txt'")] string destPath,
                             [ApiParameter(Description = "是否覆蓋現有檔案", DefaultValue = "False", IsOptional = true)] bool overwrite = false)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException($"來源檔案不存在: {sourcePath}");

                var destDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(sourcePath, destPath, overwrite);
                LoggingService.Instance.LogInfo($"已複製檔案: {sourcePath} -> {destPath}");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"複製檔案失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 取得檔案資訊
        /// </summary>
        [ApiMethod(Description = "取得檔案的詳細資訊", 
                   Example = "info = fs.get_file_info('data.txt')\nprint(f'Size: {info[\"size\"]} bytes')", 
                   Category = "File Operations")]
        public Dictionary<string, object> get_file_info([ApiParameter(Description = "檔案路徑", Example = "'C:/temp/file.txt'")] string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"檔案不存在: {filePath}");

                var fileInfo = new FileInfo(filePath);
                return new Dictionary<string, object>
                {
                    ["name"] = fileInfo.Name,
                    ["full_path"] = fileInfo.FullName,
                    ["size"] = fileInfo.Length,
                    ["created"] = fileInfo.CreationTime,
                    ["modified"] = fileInfo.LastWriteTime,
                    ["extension"] = fileInfo.Extension,
                    ["is_readonly"] = fileInfo.IsReadOnly
                };
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"取得檔案資訊失敗: {ex.Message}");
                throw;
            }
        }
    }
} 