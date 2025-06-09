using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace WpfIronPythonApp.IntelliSense
{
    /// <summary>
    /// IntelliSense 提供者
    /// 符合需求規格書第 III.A.3 節關於 IntelliSense 功能的要求
    /// 整合 API 註冊系統的動態補全支援
    /// </summary>
    public class IntelliSenseProvider
    {
        private readonly TextEditor _editor;
        private readonly Dictionary<string, List<ApiCompletionData>> _apiDatabase;
        private readonly List<PythonKeywordCompletionData> _pythonKeywords;
        private readonly List<ApiCompletionData> _dynamicCompletions; // 新增：動態補全項目
        private CompletionWindow? _completionWindow;

        public IntelliSenseProvider(TextEditor editor)
        {
            _editor = editor;
            _apiDatabase = new Dictionary<string, List<ApiCompletionData>>();
            _pythonKeywords = new List<PythonKeywordCompletionData>();
            _dynamicCompletions = new List<ApiCompletionData>();
            
            InitializeApiDatabase();
            InitializePythonKeywords();
            SetupEditor();
        }

        /// <summary>
        /// 添加補全項目（從 API 註冊系統）
        /// </summary>
        /// <param name="label">項目標籤</param>
        /// <param name="detail">詳細說明</param>
        /// <param name="documentation">文檔</param>
        /// <param name="insertText">插入文字（可選，預設使用 label）</param>
        /// <param name="kind">項目類型（可選，預設為方法）</param>
        public void AddCompletionItem(string label, string detail, string documentation, 
            string? insertText = null, string kind = "Method")
        {
            if (string.IsNullOrWhiteSpace(label)) return;

            // 判斷是否為頂級 API 對象（不包含點號）
            bool isTopLevelApi = !label.Contains('.');

            var completionData = new ApiCompletionData(
                insertText ?? label,
                $"{detail}\n{documentation}",
                kind,
                kind.Equals("Method", StringComparison.OrdinalIgnoreCase)
            );

            if (isTopLevelApi)
            {
                // 頂級 API 對象添加到動態補全項目（會在一般補全中顯示）
                _dynamicCompletions.RemoveAll(c => c.Text.Equals(label, StringComparison.OrdinalIgnoreCase));
                _dynamicCompletions.Add(completionData);
            }
            else
            {
                // 包含點號的項目（如 host.log）需要解析並添加到對應的服務
                var parts = label.Split('.');
                if (parts.Length == 2)
                {
                    var serviceName = parts[0];
                    var methodName = parts[1];
                    
                    // 為方法創建新的補全項目（只包含方法名，不包含服務名）
                    var methodCompletionData = new ApiCompletionData(
                        methodName,
                        $"{detail}\n{documentation}",
                        kind,
                        kind.Equals("Method", StringComparison.OrdinalIgnoreCase)
                    );

                    // 添加到對應服務的 API 資料庫
                    if (!_apiDatabase.ContainsKey(serviceName))
                    {
                        _apiDatabase[serviceName] = new List<ApiCompletionData>();
                    }
                    
                    // 移除重複項目並添加新項目
                    _apiDatabase[serviceName].RemoveAll(c => c.Text.Equals(methodName, StringComparison.OrdinalIgnoreCase));
                    _apiDatabase[serviceName].Add(methodCompletionData);
                }
            }
        }

        /// <summary>
        /// 清除所有動態補全項目
        /// </summary>
        public void ClearDynamicCompletions()
        {
            _dynamicCompletions.Clear();
        }

        /// <summary>
        /// 更新 API 資料庫中的特定服務
        /// </summary>
        /// <param name="serviceName">服務名稱</param>
        /// <param name="items">補全項目</param>
        public void UpdateServiceCompletions(string serviceName, IEnumerable<ApiCompletionData> items)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) return;
            
            _apiDatabase[serviceName] = items.ToList();
        }

        /// <summary>
        /// 取得統計資訊
        /// </summary>
        /// <returns>補全項目統計</returns>
        public CompletionStatistics GetStatistics()
        {
            return new CompletionStatistics
            {
                ApiItemsCount = _apiDatabase.Values.Sum(list => list.Count),
                PythonKeywordsCount = _pythonKeywords.Count,
                DynamicItemsCount = _dynamicCompletions.Count,
                TotalServicesCount = _apiDatabase.Count
            };
        }

        /// <summary>
        /// 初始化 C# API 數據庫
        /// 符合需求規格書表1中定義的所有 API
        /// </summary>
        private void InitializeApiDatabase()
        {
            // host API
            _apiDatabase["host"] = new List<ApiCompletionData>
            {
                new ApiCompletionData("log", 
                    "記錄訊息到應用程式日誌\n參數: message (str) - 日誌訊息\n範例: host.log('腳本開始執行')", 
                    "Method", true),
                new ApiCompletionData("active_doc", 
                    "取得當前活動文件物件\n返回: Document 物件或 None\n範例: doc = host.active_doc", 
                    "Property", false)
            };

            // ui API
            _apiDatabase["ui"] = new List<ApiCompletionData>
            {
                new ApiCompletionData("show_message", 
                    "顯示訊息框\n參數: \n  message (str) - 訊息內容\n  title (str) - 標題 (可選)\n範例: ui.show_message('Hello!', '歡迎')", 
                    "Method", true),
                new ApiCompletionData("status_bar", 
                    "更新狀態列文字\n參數: text (str) - 狀態列文字\n範例: ui.status_bar('處理完成')", 
                    "Method", true)
            };

            // data API
            _apiDatabase["data"] = new List<ApiCompletionData>
            {
                new ApiCompletionData("load_csv", 
                    "從 CSV 檔案載入數據\n參數: file_path (str) - CSV 檔案路徑\n返回: DataTable 物件\n範例: table = data.load_csv('data.csv')", 
                    "Method", true),
                new ApiCompletionData("process_table", 
                    "處理數據表\n參數: table (DataTable) - 要處理的資料表\n返回: 處理後的 DataTable\n範例: result = data.process_table(table)", 
                    "Method", true)
            };

            // doc API (當有活動文件時)
            _apiDatabase["doc"] = new List<ApiCompletionData>
            {
                new ApiCompletionData("save", 
                    "儲存文件\n參數: path (str) - 儲存路徑 (可選)\n返回: bool - 是否成功\n範例: doc.save() 或 doc.save('新路徑.txt')", 
                    "Method", true),
                new ApiCompletionData("page_count", 
                    "取得文件頁數\n返回: int - 頁數\n範例: pages = doc.page_count", 
                    "Property", false),
                new ApiCompletionData("content", 
                    "文件內容\n類型: str\n範例: text = doc.content 或 doc.content = '新內容'", 
                    "Property", false),
                new ApiCompletionData("file_path", 
                    "檔案路徑\n返回: str - 檔案完整路徑\n範例: path = doc.file_path", 
                    "Property", false),
                new ApiCompletionData("is_dirty", 
                    "是否已修改但未儲存\n返回: bool\n範例: if doc.is_dirty: doc.save()", 
                    "Property", false)
            };

            // 全域 API 物件
            var globalObjects = new List<ApiCompletionData>
            {
                new ApiCompletionData("host", 
                    "主機物件，提供應用程式核心功能\n可用方法: log(), active_doc\n範例: host.log('訊息')", 
                    "API", false),
                new ApiCompletionData("ui", 
                    "UI 控制器，提供使用者介面操作\n可用方法: show_message(), status_bar()\n範例: ui.show_message('Hello')", 
                    "API", false),
                new ApiCompletionData("data", 
                    "資料服務，提供資料處理功能\n可用方法: load_csv(), process_table()\n範例: data.load_csv('file.csv')", 
                    "API", false),
                new ApiCompletionData("doc", 
                    "當前活動文件 (如果有)\n可用屬性/方法: save(), page_count, content, file_path, is_dirty\n範例: doc.save()", 
                    "API", false)
            };

            _apiDatabase["global"] = globalObjects;
        }

        /// <summary>
        /// 初始化 Python 關鍵字
        /// </summary>
        private void InitializePythonKeywords()
        {
            var keywords = new[]
            {
                ("def", "定義函數\n語法: def function_name(parameters):"),
                ("class", "定義類別\n語法: class ClassName:"),
                ("if", "條件判斷\n語法: if condition:"),
                ("elif", "否則如果\n語法: elif condition:"),
                ("else", "否則\n語法: else:"),
                ("for", "for 迴圈\n語法: for item in iterable:"),
                ("while", "while 迴圈\n語法: while condition:"),
                ("try", "異常處理\n語法: try:"),
                ("except", "捕獲異常\n語法: except ExceptionType:"),
                ("finally", "最終執行\n語法: finally:"),
                ("import", "匯入模組\n語法: import module_name"),
                ("from", "從模組匯入\n語法: from module import name"),
                ("return", "返回值\n語法: return value"),
                ("print", "輸出函數\n語法: print(value)"),
                ("len", "獲取長度\n語法: len(object)"),
                ("str", "字串類型\n語法: str(value)"),
                ("int", "整數類型\n語法: int(value)"),
                ("float", "浮點數類型\n語法: float(value)"),
                ("bool", "布林類型\n語法: bool(value)"),
                ("list", "列表類型\n語法: list() 或 []"),
                ("dict", "字典類型\n語法: dict() 或 {}"),
                ("True", "布林值真"),
                ("False", "布林值假"),
                ("None", "空值"),
                ("and", "邏輯與運算子"),
                ("or", "邏輯或運算子"),
                ("not", "邏輯非運算子"),
                ("in", "成員運算子"),
                ("is", "身份運算子")
            };

            foreach (var (keyword, description) in keywords)
            {
                _pythonKeywords.Add(new PythonKeywordCompletionData(keyword, description));
            }
        }

        /// <summary>
        /// 設定編輯器事件處理
        /// </summary>
        private void SetupEditor()
        {
            _editor.TextArea.TextEntering += OnTextEntering;
            _editor.TextArea.TextEntered += OnTextEntered;
            _editor.TextArea.KeyDown += OnKeyDown;
        }

        /// <summary>
        /// 文字輸入前事件
        /// </summary>
        private void OnTextEntering(object? sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_')
                {
                    // 插入非字母數字字符時，如果補全視窗開啟且有選中項目，則完成補全
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        /// <summary>
        /// 文字輸入後事件
        /// </summary>
        private void OnTextEntered(object? sender, TextCompositionEventArgs e)
        {
            // 在輸入 '.' 時觸發 API 補全
            if (e.Text == ".")
            {
                ShowApiCompletion();
            }
            // 在輸入字母時觸發一般補全
            else if (char.IsLetter(e.Text[0]) || e.Text[0] == '_')
            {
                ShowGeneralCompletion();
            }
        }

        /// <summary>
        /// 按鍵事件處理
        /// </summary>
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            // Ctrl+Space 手動觸發補全
            if (e.Key == Key.Space && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                ShowGeneralCompletion();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 顯示 API 補全
        /// </summary>
        private void ShowApiCompletion()
        {
            var currentWord = GetWordBeforeCaret();
            
            if (string.IsNullOrEmpty(currentWord))
                return;

            var completions = new List<ICompletionData>();

            // 檢查是否為已知的 API 物件
            if (_apiDatabase.ContainsKey(currentWord))
            {
                completions.AddRange(_apiDatabase[currentWord]);
            }

            if (completions.Any())
            {
                ShowCompletionWindow(completions);
            }
        }

        /// <summary>
        /// 顯示一般補全
        /// </summary>
        private void ShowGeneralCompletion()
        {
            var completions = new List<ICompletionData>();
            var currentWord = GetCurrentWord();

            // 添加全域 API 物件
            if (_apiDatabase.ContainsKey("global"))
            {
                var filtered = _apiDatabase["global"]
                    .Where(api => string.IsNullOrEmpty(currentWord) || 
                                  api.Text.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                completions.AddRange(filtered);
            }

            // 添加動態補全項目
            var filteredDynamic = _dynamicCompletions
                .Where(item => string.IsNullOrEmpty(currentWord) || 
                              item.Text.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                .ToList();
            completions.AddRange(filteredDynamic);

            // 添加 Python 關鍵字
            var filteredKeywords = _pythonKeywords
                .Where(kw => string.IsNullOrEmpty(currentWord) || 
                            kw.Text.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                .ToList();
            completions.AddRange(filteredKeywords);

            if (completions.Any())
            {
                ShowCompletionWindow(completions);
            }
        }

        /// <summary>
        /// 顯示補全視窗
        /// </summary>
        private void ShowCompletionWindow(IEnumerable<ICompletionData> completions)
        {
            if (_completionWindow != null)
                return;

            _completionWindow = new CompletionWindow(_editor.TextArea);
            
            // 設定正確的補全範圍，避免重複字符
            var currentWord = GetCurrentWord();
            if (!string.IsNullOrEmpty(currentWord))
            {
                var wordStart = GetCurrentWordStart();
                _completionWindow.StartOffset = wordStart;
                _completionWindow.EndOffset = _editor.CaretOffset;
            }
            
            foreach (var completion in completions.OrderByDescending(c => c.Priority).ThenBy(c => c.Text))
            {
                _completionWindow.CompletionList.CompletionData.Add(completion);
            }

            if (_completionWindow.CompletionList.CompletionData.Count > 0)
            {
                _completionWindow.Show();
                _completionWindow.Closed += (s, e) => _completionWindow = null;
            }
            else
            {
                _completionWindow = null;
            }
        }

        /// <summary>
        /// 取得游標前的單詞
        /// </summary>
        private string GetWordBeforeCaret()
        {
            var document = _editor.Document;
            var offset = _editor.CaretOffset;
            
            if (offset == 0) return string.Empty;

            // 檢查游標前一個字符是否為 '.'
            if (offset > 0 && document.GetCharAt(offset - 1) == '.')
            {
                // 從 '.' 前面開始找單詞
                var start = offset - 2;
                while (start >= 0 && (char.IsLetterOrDigit(document.GetCharAt(start)) || document.GetCharAt(start) == '_'))
                {
                    start--;
                }
                start++;
                
                if (start < offset - 1)
                {
                    return document.GetText(start, offset - 1 - start);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 取得當前單詞
        /// </summary>
        private string GetCurrentWord()
        {
            var document = _editor.Document;
            var offset = _editor.CaretOffset;
            
            if (offset == 0) return string.Empty;

            // 向前找到單詞開始
            var start = offset - 1;
            while (start >= 0 && (char.IsLetterOrDigit(document.GetCharAt(start)) || document.GetCharAt(start) == '_'))
            {
                start--;
            }
            start++;

            // 向後找到單詞結束
            var end = offset;
            while (end < document.TextLength && (char.IsLetterOrDigit(document.GetCharAt(end)) || document.GetCharAt(end) == '_'))
            {
                end++;
            }

            if (start >= end) return string.Empty;
            
            return document.GetText(start, end - start);
        }

        /// <summary>
        /// 取得當前單詞的開始位置
        /// </summary>
        private int GetCurrentWordStart()
        {
            var document = _editor.Document;
            var offset = _editor.CaretOffset;
            
            if (offset == 0) return offset;

            // 向前找到單詞開始
            var start = offset - 1;
            while (start >= 0 && (char.IsLetterOrDigit(document.GetCharAt(start)) || document.GetCharAt(start) == '_'))
            {
                start--;
            }
            
            return start + 1;
        }

        /// <summary>
        /// 清理資源
        /// </summary>
        public void Dispose()
        {
            _editor.TextArea.TextEntering -= OnTextEntering;
            _editor.TextArea.TextEntered -= OnTextEntered;
            _editor.TextArea.KeyDown -= OnKeyDown;
            
            _completionWindow?.Close();
        }
    }

    /// <summary>
    /// 補全統計資訊
    /// </summary>
    public class CompletionStatistics
    {
        public int ApiItemsCount { get; set; }
        public int PythonKeywordsCount { get; set; }
        public int DynamicItemsCount { get; set; }
        public int TotalServicesCount { get; set; }

        public override string ToString()
        {
            return $"API: {ApiItemsCount}, 關鍵字: {PythonKeywordsCount}, 動態: {DynamicItemsCount}, 服務: {TotalServicesCount}";
        }
    }
} 