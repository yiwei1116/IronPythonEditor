using System;
using System.Data;
using System.IO;
using System.Windows;
using WpfIronPythonApp.Models;
using WpfIronPythonApp.Services;
using WpfIronPythonApp.Services.ApiRegistry;

namespace WpfIronPythonApp.Scripting
{
    /// <summary>
    /// 主機物件介面，定義公開給 IronPython 的 API 契約
    /// 符合需求規格書第 IV.B 節的主機物件模式
    /// </summary>
    public interface IScriptHost
    {
        /// <summary>
        /// 記錄訊息到應用程式日誌
        /// </summary>
        void log(string message);

        /// <summary>
        /// 取得當前活動文件物件
        /// </summary>
        Document? active_doc { get; }

        /// <summary>
        /// UI 控制器
        /// </summary>
        IUIController UI { get; }

        /// <summary>
        /// 資料服務
        /// </summary>
        IDataService Data { get; }
    }

    /// <summary>
    /// UI 控制器介面
    /// 符合需求規格書表1中的 UIController API
    /// </summary>
    public interface IUIController
    {
        /// <summary>
        /// 顯示訊息框
        /// </summary>
        void show_message(string message, string title = "訊息");

        /// <summary>
        /// 更新狀態列文字
        /// </summary>
        void status_bar(string text);
    }

    /// <summary>
    /// 資料服務介面
    /// 符合需求規格書表1中的 DataService API
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// 從 CSV 載入數據
        /// </summary>
        DataTable load_csv(string filePath);

        /// <summary>
        /// 處理數據表
        /// </summary>
        DataTable process_table(DataTable table);
    }

    /// <summary>
    /// 腳本主機物件實作
    /// 符合需求規格書表1中的 ApplicationHost API
    /// </summary>
    [ApiService("host", Description = "應用程式主機服務，提供基本的應用程式功能", Version = "1.0.0", IsCore = true)]
    public class ScriptHost : IScriptHost
    {
        private readonly Func<Document?> _getActiveDocument;
        private readonly Action<string> _updateStatusBar;

        public ScriptHost(Func<Document?> getActiveDocument, Action<string> updateStatusBar)
        {
            _getActiveDocument = getActiveDocument;
            _updateStatusBar = updateStatusBar;
            UI = new UIController(_updateStatusBar);
            Data = new DataService();
        }

        /// <summary>
        /// 記錄訊息到應用程式日誌
        /// 符合需求規格書表1中的 host.log API
        /// </summary>
        /// <param name="message">日誌訊息</param>
        [ApiMethod(Description = "記錄訊息到應用程式日誌", 
                   Example = "host.log('Hello World!')", 
                   Category = "Logging")]
        public void log([ApiParameter(Description = "要記錄的訊息", Example = "'Hello World!'")] string message)
        {
            LoggingService.Instance.LogMessage(message);
        }

        /// <summary>
        /// 取得當前活動文件物件
        /// 符合需求規格書表1中的 host.active_doc API
        /// </summary>
        [ApiMethod(Description = "取得當前活動的文件物件", 
                   Example = "doc = host.active_doc\nif doc:\n    print(doc.file_path)", 
                   Category = "Document")]
        public Document? active_doc => _getActiveDocument();

        /// <summary>
        /// UI 控制器
        /// </summary>
        public IUIController UI { get; }

        /// <summary>
        /// 資料服務
        /// </summary>
        public IDataService Data { get; }
    }

    /// <summary>
    /// UI 控制器實作
    /// 符合需求規格書表1中的 UIController API
    /// </summary>
    [ApiService("ui", Description = "使用者介面控制服務，提供UI相關功能", Version = "1.0.0", IsCore = true)]
    public class UIController : IUIController
    {
        private readonly Action<string> _updateStatusBar;

        public UIController(Action<string> updateStatusBar)
        {
            _updateStatusBar = updateStatusBar;
        }

        /// <summary>
        /// 顯示訊息框
        /// 符合需求規格書表1中的 ui.show_message API
        /// </summary>
        /// <param name="message">訊息內容</param>
        /// <param name="title">訊息標題</param>
        [ApiMethod(Description = "顯示訊息對話框", 
                   Example = "ui.show_message('Hello World!', 'Greeting')", 
                   Category = "UI")]
        public void show_message([ApiParameter(Description = "要顯示的訊息內容", Example = "'Hello World!'")] string message, 
                                [ApiParameter(Description = "對話框標題", DefaultValue = "'訊息'", IsOptional = true)] string title = "訊息")
        {
            // 確保在 UI 執行緒上執行
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        /// <summary>
        /// 更新狀態列文字
        /// 符合需求規格書表1中的 ui.status_bar API
        /// </summary>
        /// <param name="text">狀態列文字</param>
        [ApiMethod(Description = "更新應用程式狀態列文字", 
                   Example = "ui.status_bar('Processing...')", 
                   Category = "UI")]
        public void status_bar([ApiParameter(Description = "要顯示在狀態列的文字", Example = "'Processing...'")] string text)
        {
            // 確保在 UI 執行緒上執行
            Application.Current.Dispatcher.Invoke(() =>
            {
                _updateStatusBar(text);
            });
        }
    }

    /// <summary>
    /// 資料服務實作
    /// 符合需求規格書表1中的 DataService API
    /// </summary>
    [ApiService("data", Description = "資料處理服務，提供檔案讀寫和資料處理功能", Version = "1.0.0", IsCore = true)]
    public class DataService : IDataService
    {
        /// <summary>
        /// 從 CSV 載入數據
        /// 符合需求規格書表1中的 data.load_csv API
        /// </summary>
        /// <param name="filePath">CSV 檔案路徑</param>
        /// <returns>載入的資料表</returns>
        [ApiMethod(Description = "從CSV檔案載入資料到DataTable", 
                   Example = "table = data.load_csv('data.csv')\nprint(table.Rows.Count)", 
                   Category = "Data", 
                   Permission = ApiPermission.FileAccess)]
        public DataTable load_csv([ApiParameter(Description = "CSV檔案的完整路徑", Example = "'C:/data/example.csv'")] string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"檔案不存在: {filePath}");
                }

                var dataTable = new DataTable();
                var lines = File.ReadAllLines(filePath);
                
                if (lines.Length == 0)
                {
                    return dataTable;
                }

                // 解析標題行
                var headers = lines[0].Split(',');
                foreach (var header in headers)
                {
                    dataTable.Columns.Add(header.Trim('"'));
                }

                // 解析資料行
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    var row = dataTable.NewRow();
                    
                    for (int j = 0; j < Math.Min(values.Length, dataTable.Columns.Count); j++)
                    {
                        row[j] = values[j].Trim('"');
                    }
                    
                    dataTable.Rows.Add(row);
                }

                LoggingService.Instance.LogInfo($"已載入 CSV 檔案: {filePath}，共 {dataTable.Rows.Count} 行");
                return dataTable;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"載入 CSV 檔案失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 處理數據表
        /// 符合需求規格書表1中的 data.process_table API
        /// </summary>
        /// <param name="table">要處理的資料表</param>
        /// <returns>處理後的資料表</returns>
        [ApiMethod(Description = "處理資料表，移除空白行並進行基本清理", 
                   Example = "cleaned_table = data.process_table(table)", 
                   Category = "Data")]
        public DataTable process_table([ApiParameter(Description = "要處理的DataTable物件")] DataTable table)
        {
            try
            {
                if (table == null)
                {
                    throw new ArgumentNullException(nameof(table));
                }

                // 建立處理後的資料表副本
                var processedTable = table.Copy();
                
                // 範例處理：移除空白行
                for (int i = processedTable.Rows.Count - 1; i >= 0; i--)
                {
                    var row = processedTable.Rows[i];
                    bool isEmpty = true;
                    
                    foreach (var item in row.ItemArray)
                    {
                        if (item != null && !string.IsNullOrWhiteSpace(item.ToString()))
                        {
                            isEmpty = false;
                            break;
                        }
                    }
                    
                    if (isEmpty)
                    {
                        processedTable.Rows.RemoveAt(i);
                    }
                }

                LoggingService.Instance.LogInfo($"已處理資料表，處理後共 {processedTable.Rows.Count} 行");
                return processedTable;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"處理資料表失敗: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// 文件包裝器，為腳本提供文件操作 API
    /// 符合需求規格書表1中的 Document API
    /// </summary>
    public class DocumentWrapper
    {
        private readonly Document _document;

        public DocumentWrapper(Document document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        /// <summary>
        /// 儲存文件
        /// 符合需求規格書表1中的 doc.save() API
        /// </summary>
        /// <param name="path">儲存路徑</param>
        /// <returns>是否儲存成功</returns>
        public bool Save(string? path = null)
        {
            return _document.Save(path);
        }

        /// <summary>
        /// 取得文件頁數
        /// 符合需求規格書表1中的 doc.page_count API
        /// </summary>
        public int page_count => _document.PageCount;

        /// <summary>
        /// 文件內容
        /// </summary>
        public string content 
        { 
            get => _document.Content; 
            set => _document.Content = value; 
        }

        /// <summary>
        /// 檔案路徑
        /// </summary>
        public string file_path => _document.FilePath;

        /// <summary>
        /// 是否已修改
        /// </summary>
        public bool is_dirty => _document.IsDirty;
    }
} 