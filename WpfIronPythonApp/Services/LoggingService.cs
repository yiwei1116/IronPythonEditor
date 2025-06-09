using System;
using System.IO;
using System.Threading;

namespace WpfIronPythonApp.Services
{
    /// <summary>
    /// 日誌服務
    /// 符合需求規格書第 III.A.1.d 節中的錯誤處理和日誌記錄要求
    /// </summary>
    public class LoggingService
    {
        private static readonly Lazy<LoggingService> _instance = 
            new Lazy<LoggingService>(() => new LoggingService(), LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly string _logFilePath;
        private readonly object _lockObject = new object();

        private LoggingService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WpfIronPythonApp",
                "Logs");

            Directory.CreateDirectory(appDataPath);

            _logFilePath = Path.Combine(appDataPath, $"app_{DateTime.Now:yyyyMMdd}.log");
        }

        /// <summary>
        /// 單例實例
        /// </summary>
        public static LoggingService Instance => _instance.Value;

        /// <summary>
        /// 記錄訊息到應用程式日誌
        /// 符合需求規格書表1中的 host.log API
        /// </summary>
        /// <param name="message">日誌訊息</param>
        public void LogMessage(string message)
        {
            LogInfo(message);
        }

        /// <summary>
        /// 記錄資訊訊息
        /// </summary>
        /// <param name="message">訊息內容</param>
        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// 記錄警告訊息
        /// </summary>
        /// <param name="message">訊息內容</param>
        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        /// <summary>
        /// 記錄錯誤訊息
        /// </summary>
        /// <param name="message">錯誤訊息</param>
        public void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        /// <summary>
        /// 記錄腳本執行錯誤
        /// 符合需求規格書第 III.A.1.d 節的腳本錯誤記錄要求
        /// </summary>
        /// <param name="scriptName">腳本名稱</param>
        /// <param name="error">錯誤詳細資訊</param>
        /// <param name="lineNumber">錯誤行號</param>
        public void LogScriptError(string scriptName, string error, int? lineNumber = null)
        {
            var errorMessage = lineNumber.HasValue 
                ? $"腳本 '{scriptName}' 在第 {lineNumber} 行發生錯誤: {error}"
                : $"腳本 '{scriptName}' 發生錯誤: {error}";
            
            LogError(errorMessage);
        }

        /// <summary>
        /// 寫入日誌
        /// </summary>
        /// <param name="level">日誌級別</param>
        /// <param name="message">訊息內容</param>
        private void WriteLog(string level, string message)
        {
            try
            {
                lock (_lockObject)
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
                    
                    // 寫入檔案
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                    
                    // 如果是錯誤或警告，也輸出到控制台
                    if (level == "ERROR" || level == "WARN")
                    {
                        Console.WriteLine(logEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                // 日誌系統本身的錯誤不應該中斷應用程式
                Console.WriteLine($"日誌寫入失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 獲取日誌檔案路徑
        /// </summary>
        /// <returns>日誌檔案完整路徑</returns>
        public string GetLogFilePath()
        {
            return _logFilePath;
        }

        /// <summary>
        /// 清理舊日誌檔案（保留最近30天）
        /// </summary>
        public void CleanupOldLogs()
        {
            try
            {
                var logDirectory = Path.GetDirectoryName(_logFilePath);
                if (logDirectory == null) return;

                var cutoffDate = DateTime.Now.AddDays(-30);
                var logFiles = Directory.GetFiles(logDirectory, "app_*.log");

                foreach (var logFile in logFiles)
                {
                    var fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(logFile);
                        LogInfo($"已刪除舊日誌檔案: {Path.GetFileName(logFile)}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"清理舊日誌檔案失敗: {ex.Message}");
            }
        }
    }
} 