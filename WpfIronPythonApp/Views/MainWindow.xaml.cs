using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WpfIronPythonApp.Models;
using WpfIronPythonApp.Scripting;
using WpfIronPythonApp.Services;
using WpfIronPythonApp.Services.ApiRegistry;
using WpfIronPythonApp.IntelliSense;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace WpfIronPythonApp.Views
{
    /// <summary>
    /// 主視窗類別
    /// 符合需求規格書第 III.A.2 節關於整合式腳本編輯器的要求
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IronPythonEngine _pythonEngine = null!;
        private readonly ScriptHost _scriptHost = null!;
        private readonly IApiRegistry _apiRegistry = null!;
        private IntelliSenseProvider? _intelliSenseProvider;
        private CodeSuggestionManager? _suggestionManager;
        private IAICodeAssistantService? _aiService;
        private Document? _currentDocument;
        private CancellationTokenSource? _scriptCancellationSource;
        private bool _isScriptRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                // 初始化API註冊系統
                _apiRegistry = new ApiRegistryService();
                
                // 初始化 IronPython 引擎（使用API註冊系統）
                _pythonEngine = new IronPythonEngine(_apiRegistry);
                _pythonEngine.ScriptExecuted += OnScriptExecuted;
                _pythonEngine.ScriptError += OnScriptError;

                // 建立主機物件
                _scriptHost = new ScriptHost(
                    getActiveDocument: () => _currentDocument,
                    updateStatusBar: UpdateStatusBar
                );

                // 註冊額外的API服務
                RegisterAdditionalServices();

                // 公開主機物件給 IronPython（向後相容）
                SetupHostObjects();

                // 設定編輯器
                SetupEditor();

                // 初始化 IntelliSense
                SetupIntelliSense();

                // 初始化 AI 助手
                SetupAIAssistant();

                // 初始化 UI 狀態
                UpdateUI();
                UpdateStatusBar("IronPython 引擎已就緒 (IntelliSense + AI 助手已啟用)");

                LoggingService.Instance.LogInfo("主視窗初始化完成");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"主視窗初始化失敗: {ex.Message}");
                MessageBox.Show($"應用程式初始化失敗:\n{ex.Message}", "錯誤", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        /// <summary>
        /// 註冊額外的API服務
        /// </summary>
        private void RegisterAdditionalServices()
        {
            try
            {
                // 註冊檔案系統服務
                _pythonEngine.RegisterApiService(new FileSystemService());
                
                // 註冊數學計算服務
                _pythonEngine.RegisterApiService(new MathService());
                
                // 註冊核心主機服務（向後相容）
                _pythonEngine.RegisterApiService(_scriptHost, "host");
                _pythonEngine.RegisterApiService(_scriptHost.UI, "ui");
                _pythonEngine.RegisterApiService(_scriptHost.Data, "data");

                LoggingService.Instance.LogInfo("已註冊額外的API服務");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"註冊額外API服務失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 設定主機物件，公開 C# API 給 IronPython
        /// 符合需求規格書第 III.A.1.b 節和第 IV.B 節的要求
        /// </summary>
        private void SetupHostObjects()
        {
            try
            {
                // 公開主機物件 (符合需求規格書表1)
                _pythonEngine.SetVariable("host", _scriptHost);
                _pythonEngine.SetVariable("ui", _scriptHost.UI);
                _pythonEngine.SetVariable("data", _scriptHost.Data);

                LoggingService.Instance.LogInfo("已設定主機物件");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"設定主機物件失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 設定 Python 編輯器
        /// 符合需求規格書第 III.A.2 節關於編輯器功能的要求
        /// </summary>
        private void SetupEditor()
        {
            try
            {
                // 設定 Python 語法高亮
                PythonEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Python");
                
                // 設定預設腳本內容
                PythonEditor.Text = @"# WPF IronPython 巨集範例
# 符合需求規格書表1中定義的 API
# 開始輸入 'host.' 或 'ui.' 來查看可用的 API

# 記錄日誌
host.log('腳本開始執行')

# 顯示訊息
ui.show_message('Hello from IronPython!', '歡迎')

# 更新狀態列
ui.status_bar('腳本執行完成')

# 取得活動文件 (如果有的話)
doc = host.active_doc
if doc:
    host.log(f'當前文件頁數: {doc.page_count}')
else:
    host.log('目前沒有活動文件')

print('IronPython 腳本執行成功!')";

                // 設定編輯器選項
                PythonEditor.Options.ConvertTabsToSpaces = true;
                PythonEditor.Options.IndentationSize = 4;
                PythonEditor.Options.EnableRectangularSelection = true;
                PythonEditor.Options.EnableTextDragDrop = true;

                LoggingService.Instance.LogInfo("Python 編輯器設定完成");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"設定編輯器失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 初始化 IntelliSense 功能
        /// 符合需求規格書第 III.A.3 節關於 IntelliSense 實作的要求
        /// </summary>
        private async void SetupIntelliSense()
        {
            try
            {
                _intelliSenseProvider = new IntelliSenseProvider(PythonEditor);
                
                // 使用API註冊系統生成IntelliSense數據
                await UpdateIntelliSenseFromApiRegistry();
                
                LoggingService.Instance.LogInfo("IntelliSense 初始化完成");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"IntelliSense 初始化失敗: {ex.Message}");
                // IntelliSense 失敗不應該影響整個應用程式
            }
        }

        /// <summary>
        /// 從API註冊系統更新IntelliSense數據
        /// </summary>
        private async Task UpdateIntelliSenseFromApiRegistry()
        {
            try
            {
                if (_intelliSenseProvider != null)
                {
                    var intelliSenseData = await _apiRegistry.GenerateIntelliSenseDataAsync();
                    
                    // 將API註冊系統的數據轉換為IntelliSense提供者可用的格式
                    foreach (var item in intelliSenseData.CompletionItems)
                    {
                        _intelliSenseProvider.AddCompletionItem(item.Label, item.Detail, item.Documentation);
                    }
                    
                    LoggingService.Instance.LogInfo($"已從API註冊系統載入 {intelliSenseData.CompletionItems.Count} 個IntelliSense項目");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"更新IntelliSense數據失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化 AI 助手功能
        /// </summary>
        private void SetupAIAssistant()
        {
            try
            {
                _aiService = new MockAICodeAssistantService();
                _suggestionManager = new CodeSuggestionManager(PythonEditor);
                LoggingService.Instance.LogInfo("AI 助手初始化完成");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"AI 助手初始化失敗: {ex.Message}");
                // AI 助手失敗不應該影響整個應用程式
            }
        }

        /// <summary>
        /// 更新狀態列
        /// </summary>
        /// <param name="message">狀態訊息</param>
        private void UpdateStatusBar(string message)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = message;
            });
        }

        /// <summary>
        /// 更新 UI 狀態
        /// </summary>
        private void UpdateUI()
        {
            RunButton.IsEnabled = !_isScriptRunning;
            StopButton.IsEnabled = _isScriptRunning;
        }

        /// <summary>
        /// 新建檔案
        /// </summary>
        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                _currentDocument = new Document();
                PythonEditor.Text = string.Empty;
                UpdateStatusBar("新建檔案");
                LoggingService.Instance.LogInfo("建立新檔案");
            }
        }

        /// <summary>
        /// 開啟檔案
        /// </summary>
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckUnsavedChanges()) return;

            var openDialog = new OpenFileDialog
            {
                Filter = "Python 檔案 (*.py)|*.py|所有檔案 (*.*)|*.*",
                DefaultExt = ".py"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    _currentDocument = new Document();
                    if (_currentDocument.Load(openDialog.FileName))
                    {
                        PythonEditor.Text = _currentDocument.Content;
                        UpdateStatusBar($"已開啟: {Path.GetFileName(openDialog.FileName)}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"開啟檔案失敗:\n{ex.Message}", "錯誤", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 儲存檔案
        /// </summary>
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (_currentDocument == null)
            {
                SaveAsFile_Click(sender, e);
                return;
            }

            try
            {
                _currentDocument.Content = PythonEditor.Text;
                
                if (string.IsNullOrEmpty(_currentDocument.FilePath))
                {
                    SaveAsFile_Click(sender, e);
                }
                else
                {
                    if (_currentDocument.Save())
                    {
                        UpdateStatusBar("檔案已儲存");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存檔案失敗:\n{ex.Message}", "錯誤", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 另存為檔案
        /// </summary>
        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Python 檔案 (*.py)|*.py|所有檔案 (*.*)|*.*",
                DefaultExt = ".py"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    if (_currentDocument == null)
                    {
                        _currentDocument = new Document();
                    }
                    
                    _currentDocument.Content = PythonEditor.Text;
                    
                    if (_currentDocument.Save(saveDialog.FileName))
                    {
                        UpdateStatusBar($"已儲存: {Path.GetFileName(saveDialog.FileName)}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"儲存檔案失敗:\n{ex.Message}", "錯誤", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 執行 Python 腳本
        /// 符合需求規格書第 III.A.1.a 節關於腳本執行的要求
        /// </summary>
        private async void RunScript_Click(object sender, RoutedEventArgs e)
        {
            if (_isScriptRunning) return;

            try
            {
                _isScriptRunning = true;
                _scriptCancellationSource = new CancellationTokenSource();
                UpdateUI();

                // 清除之前的輸出
                OutputTextBox.Clear();
                ErrorTextBox.Clear();

                // 更新文件物件
                if (_currentDocument != null)
                {
                    _currentDocument.Content = PythonEditor.Text;
                    
                    // 公開文件物件給腳本
                    var docWrapper = new DocumentWrapper(_currentDocument);
                    _pythonEngine.SetVariable("doc", docWrapper);
                }

                var scriptCode = PythonEditor.Text;
                var scriptName = _currentDocument?.FilePath ?? "editor_script";

                UpdateStatusBar("正在執行腳本...");

                // 重定向輸出
                RedirectPythonOutput();

                // 非同步執行腳本 (符合需求規格書第 III.B.1.a 節關於響應式執行的要求)
                var result = await _pythonEngine.ExecuteScriptAsync(scriptCode, scriptName);
                
                // 顯示執行結果
                if (result != null)
                {
                    AppendOutput($"執行結果: {result}");
                }

                UpdateStatusBar("腳本執行完成");
            }
            catch (ScriptExecutionException ex)
            {
                ErrorTextBox.Text = ex.Message;
                UpdateStatusBar("腳本執行錯誤");
            }
            catch (Exception ex)
            {
                ErrorTextBox.Text = $"執行失敗: {ex.Message}";
                UpdateStatusBar("腳本執行失敗");
                LoggingService.Instance.LogError($"腳本執行異常: {ex}");
            }
            finally
            {
                _isScriptRunning = false;
                _scriptCancellationSource?.Dispose();
                _scriptCancellationSource = null;
                UpdateUI();
            }
        }

        /// <summary>
        /// 停止腳本執行
        /// </summary>
        private void StopScript_Click(object sender, RoutedEventArgs e)
        {
            if (_scriptCancellationSource != null && !_scriptCancellationSource.IsCancellationRequested)
            {
                _scriptCancellationSource.Cancel();
                UpdateStatusBar("正在停止腳本執行...");
            }
        }

        /// <summary>
        /// 重置腳本環境
        /// 符合需求規格書第 III.A.1.e 節關於重置環境的要求
        /// </summary>
        private void ResetEnvironment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _pythonEngine.ResetCurrentScope();
                SetupHostObjects(); // 重新設定主機物件
                
                OutputTextBox.Clear();
                ErrorTextBox.Clear();
                
                UpdateStatusBar("腳本環境已重置");
                LoggingService.Instance.LogInfo("腳本環境已重置");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"重置環境失敗:\n{ex.Message}", "錯誤", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 結束應用程式
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 編輯器文字改變事件
        /// </summary>
        private void PythonEditor_TextChanged(object sender, EventArgs e)
        {
            if (_currentDocument != null)
            {
                _currentDocument.Content = PythonEditor.Text;
            }
        }

        /// <summary>
        /// 檢查未儲存的變更
        /// </summary>
        /// <returns>是否可以繼續操作</returns>
        private bool CheckUnsavedChanges()
        {
            if (_currentDocument != null && _currentDocument.IsDirty)
            {
                var result = MessageBox.Show(
                    "目前檔案有未儲存的變更，是否要儲存？",
                    "未儲存的變更",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        SaveFile_Click(this, new RoutedEventArgs());
                        return !_currentDocument.IsDirty; // 如果儲存失敗，IsDirty 仍為 true
                    case MessageBoxResult.No:
                        return true;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 重定向 Python 輸出到 UI
        /// </summary>
        private void RedirectPythonOutput()
        {
            // 設定 Python 的 stdout 重定向
            var outputWriter = new TextBoxWriter(OutputTextBox);
            _pythonEngine.SetVariable("sys", outputWriter);
        }

        /// <summary>
        /// 附加輸出內容
        /// </summary>
        /// <param name="text">輸出文字</param>
        private void AppendOutput(string text)
        {
            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText(text + Environment.NewLine);
                OutputTextBox.ScrollToEnd();
            });
        }

        /// <summary>
        /// 腳本執行完成事件處理
        /// </summary>
        private void OnScriptExecuted(object? sender, ScriptExecutionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AppendOutput($"[{DateTime.Now:HH:mm:ss}] 腳本 '{e.ScriptName}' 執行完成");
            });
        }

        /// <summary>
        /// 腳本執行錯誤事件處理
        /// </summary>
        private void OnScriptError(object? sender, ScriptErrorEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var errorMessage = e.LineNumber.HasValue 
                    ? $"[{DateTime.Now:HH:mm:ss}] 腳本 '{e.ScriptName}' 第 {e.LineNumber} 行錯誤: {e.ErrorMessage}"
                    : $"[{DateTime.Now:HH:mm:ss}] 腳本 '{e.ScriptName}' 錯誤: {e.ErrorMessage}";
                
                ErrorTextBox.AppendText(errorMessage + Environment.NewLine);
                ErrorTextBox.ScrollToEnd();
            });
        }

        #region AI 助手事件處理

        /// <summary>
        /// 獲取AI程式碼建議
        /// </summary>
        private async void GetAISuggestion_Click(object sender, RoutedEventArgs e)
        {
            await HandleAIRequest("suggestion");
        }

        /// <summary>
        /// 優化程式碼
        /// </summary>
        private async void OptimizeCode_Click(object sender, RoutedEventArgs e)
        {
            await HandleAIRequest("optimize");
        }

        /// <summary>
        /// 解釋程式碼
        /// </summary>
        private async void ExplainCode_Click(object sender, RoutedEventArgs e)
        {
            await HandleAIRequest("explain");
        }

        /// <summary>
        /// 修復程式碼
        /// </summary>
        private async void FixCode_Click(object sender, RoutedEventArgs e)
        {
            await HandleAIRequest("fix");
        }

        /// <summary>
        /// 處理AI請求的通用方法
        /// </summary>
        private async Task HandleAIRequest(string requestType)
        {
            if (_aiService == null || _suggestionManager == null)
            {
                MessageBox.Show("AI 助手未初始化", "錯誤", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                UpdateStatusBar("AI 助手處理中...");

                // 獲取選取的程式碼或全部程式碼
                string selectedCode = PythonEditor.SelectedText;
                if (string.IsNullOrWhiteSpace(selectedCode))
                {
                    selectedCode = PythonEditor.Text;
                }

                if (string.IsNullOrWhiteSpace(selectedCode))
                {
                    selectedCode = "";
                }

                // 獲取插入位置
                int insertPosition = PythonEditor.SelectionLength > 0 
                    ? PythonEditor.SelectionStart + PythonEditor.SelectionLength
                    : PythonEditor.CaretOffset;

                string aiResponse = "";

                // 根據請求類型調用對應的AI服務
                switch (requestType)
                {
                    case "suggestion":
                        aiResponse = await _aiService.GetCodeSuggestionAsync(selectedCode, PythonEditor.Text);
                        break;
                    case "optimize":
                        aiResponse = await _aiService.OptimizeCodeAsync(selectedCode);
                        break;
                    case "explain":
                        aiResponse = await _aiService.ExplainCodeAsync(selectedCode);
                        // 解釋程式碼不插入程式碼，而是顯示在輸出面板
                        AppendOutput($"\n=== AI 程式碼解釋 ===\n{aiResponse}\n");
                        UpdateStatusBar("AI 解釋已顯示在輸出面板");
                        return;
                    case "fix":
                        // 如果沒有選取程式碼，詢問錯誤訊息
                        string errorMessage = "";
                        if (string.IsNullOrWhiteSpace(PythonEditor.SelectedText))
                        {
                            var inputDialog = new InputDialog("請輸入錯誤訊息（可選）:", "修復程式碼");
                            if (inputDialog.ShowDialog() == true)
                            {
                                errorMessage = inputDialog.InputText;
                            }
                        }
                        aiResponse = await _aiService.FixCodeAsync(selectedCode, errorMessage);
                        break;
                }

                if (!string.IsNullOrWhiteSpace(aiResponse))
                {
                    // 顯示AI建議
                    _suggestionManager.ShowSuggestion(aiResponse, insertPosition);
                    UpdateStatusBar($"AI {GetRequestTypeDisplayName(requestType)} 已準備好 (按 Tab 應用，Esc 取消)");
                }
                else
                {
                    UpdateStatusBar("AI 沒有提供建議");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"AI 請求失敗: {ex.Message}");
                MessageBox.Show($"AI 請求失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatusBar("AI 請求失敗");
            }
        }

        /// <summary>
        /// 獲取請求類型的顯示名稱
        /// </summary>
        private string GetRequestTypeDisplayName(string requestType)
        {
            return requestType switch
            {
                "suggestion" => "建議",
                "optimize" => "優化",
                "explain" => "解釋",
                "fix" => "修復",
                _ => "處理"
            };
        }

        #endregion

        #region 編輯器右鍵選單事件

        /// <summary>
        /// 復原
        /// </summary>
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (PythonEditor.CanUndo)
                PythonEditor.Undo();
        }

        /// <summary>
        /// 重做
        /// </summary>
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (PythonEditor.CanRedo)
                PythonEditor.Redo();
        }

        /// <summary>
        /// 剪下
        /// </summary>
        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            PythonEditor.Cut();
        }

        /// <summary>
        /// 複製
        /// </summary>
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            PythonEditor.Copy();
        }

        /// <summary>
        /// 貼上
        /// </summary>
        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            PythonEditor.Paste();
        }

        /// <summary>
        /// 全選
        /// </summary>
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            PythonEditor.SelectAll();
        }

        #endregion

        #region 工具菜單事件

        /// <summary>
        /// API管理器菜單點擊事件
        /// </summary>
        private void ApiManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var apiManagerWindow = new ApiManagerWindow(_apiRegistry)
                {
                    Owner = this
                };
                apiManagerWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"開啟API管理器失敗:\n{ex.Message}", "錯誤", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LoggingService.Instance.LogError($"開啟API管理器失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成API文件菜單點擊事件
        /// </summary>
        private void GenerateApiDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "生成API文件",
                    Filter = "Markdown 檔案 (*.md)|*.md|JSON 檔案 (*.json)|*.json|HTML 檔案 (*.html)|*.html|文字檔案 (*.txt)|*.txt",
                    DefaultExt = "md",
                    FileName = "API_Documentation"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var format = GetDocumentFormatFromExtension(Path.GetExtension(saveDialog.FileName));
                    var documentation = _apiRegistry.GenerateDocumentation(format);
                    
                    File.WriteAllText(saveDialog.FileName, documentation);
                    UpdateStatusBar($"API文件已生成: {saveDialog.FileName}");
                    
                    MessageBox.Show($"API文件已成功生成到:\n{saveDialog.FileName}", "生成成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成API文件失敗:\n{ex.Message}", "錯誤", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LoggingService.Instance.LogError($"生成API文件失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 重新載入IntelliSense菜單點擊事件
        /// </summary>
        private async void ReloadIntelliSense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateStatusBar("正在重新載入IntelliSense...");
                await UpdateIntelliSenseFromApiRegistry();
                UpdateStatusBar("IntelliSense已重新載入");
            }
            catch (Exception ex)
            {
                UpdateStatusBar("重新載入IntelliSense失敗");
                MessageBox.Show($"重新載入IntelliSense失敗:\n{ex.Message}", "錯誤", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LoggingService.Instance.LogError($"重新載入IntelliSense失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 根據檔案副檔名獲取文件格式
        /// </summary>
        private DocumentationFormat GetDocumentFormatFromExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".md" => DocumentationFormat.Markdown,
                ".json" => DocumentationFormat.Json,
                ".html" => DocumentationFormat.Html,
                ".txt" => DocumentationFormat.PlainText,
                _ => DocumentationFormat.Markdown
            };
        }

        #endregion

        /// <summary>
        /// 窗口關閉事件
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!CheckUnsavedChanges())
            {
                e.Cancel = true;
                return;
            }

            try
            {
                // 停止正在執行的腳本
                if (_isScriptRunning)
                {
                    _scriptCancellationSource?.Cancel();
                }

                // 釋放資源
                _intelliSenseProvider?.Dispose();
                _suggestionManager?.Dispose();
                _pythonEngine?.Dispose();
                
                LoggingService.Instance.LogInfo("應用程式正常關閉");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"關閉應用程式時發生錯誤: {ex.Message}");
            }

            base.OnClosing(e);
        }
    }

    /// <summary>
    /// TextBox 輸出寫入器，用於重定向 Python 輸出
    /// </summary>
    public class TextBoxWriter : System.IO.TextWriter
    {
        private readonly System.Windows.Controls.TextBox _textBox;

        public TextBoxWriter(System.Windows.Controls.TextBox textBox)
        {
            _textBox = textBox;
        }

        public override void Write(char value)
        {
            _textBox.Dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value.ToString());
                _textBox.ScrollToEnd();
            });
        }

        public override void Write(string? value)
        {
            if (value != null)
            {
                _textBox.Dispatcher.Invoke(() =>
                {
                    _textBox.AppendText(value);
                    _textBox.ScrollToEnd();
                });
            }
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
    }
} 