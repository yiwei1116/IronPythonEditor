using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using WpfIronPythonApp.Services;
using WpfIronPythonApp.Services.ApiRegistry;

namespace WpfIronPythonApp.Scripting
{
    /// <summary>
    /// IronPython 腳本執行引擎
    /// 符合需求規格書第 III.A.1 節和第 IV.A 節的 DLR 託管 API 要求
    /// 實現 ScriptEngine, ScriptScope, ScriptSource 的管理
    /// </summary>
    public class IronPythonEngine : IDisposable
    {
        private readonly ScriptEngine _engine;
        private readonly Dictionary<string, ScriptScope> _scopes;
        private readonly object _lockObject = new object();
        private readonly IApiRegistry _apiRegistry;
        private bool _disposed = false;

        /// <summary>
        /// 當前活動的腳本範圍
        /// </summary>
        public ScriptScope? CurrentScope { get; private set; }

        /// <summary>
        /// 腳本執行完成事件
        /// </summary>
        public event EventHandler<ScriptExecutionEventArgs>? ScriptExecuted;

        /// <summary>
        /// 腳本執行錯誤事件
        /// </summary>
        public event EventHandler<ScriptErrorEventArgs>? ScriptError;

        public IronPythonEngine(IApiRegistry? apiRegistry = null)
        {
            try
            {
                // 建立 IronPython ScriptEngine (符合需求規格書第 IV.A 節)
                _engine = Python.CreateEngine();
                _scopes = new Dictionary<string, ScriptScope>();
                
                // 初始化API註冊系統
                _apiRegistry = apiRegistry ?? new ApiRegistryService();

                // 配置 Python 模組搜尋路徑
                ConfigureSearchPaths();

                // 建立預設範圍
                CreateScope("default");
                SwitchToScope("default");

                // 自動註冊API服務
                AutoRegisterApiServices();

                LoggingService.Instance.LogInfo("IronPython 引擎初始化完成");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"IronPython 引擎初始化失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 建立新的腳本範圍
        /// 符合需求規格書中關於隔離不同巨集執行環境的要求
        /// </summary>
        /// <param name="scopeName">範圍名稱</param>
        /// <returns>建立的範圍</returns>
        public ScriptScope CreateScope(string scopeName)
        {
            lock (_lockObject)
            {
                if (_scopes.ContainsKey(scopeName))
                {
                    throw new InvalidOperationException($"範圍 '{scopeName}' 已存在");
                }

                var scope = _engine.CreateScope();
                _scopes[scopeName] = scope;

                LoggingService.Instance.LogInfo($"已建立腳本範圍: {scopeName}");
                return scope;
            }
        }

        /// <summary>
        /// 切換到指定的腳本範圍
        /// </summary>
        /// <param name="scopeName">範圍名稱</param>
        public void SwitchToScope(string scopeName)
        {
            lock (_lockObject)
            {
                if (!_scopes.TryGetValue(scopeName, out var scope))
                {
                    throw new ArgumentException($"範圍 '{scopeName}' 不存在");
                }

                CurrentScope = scope;
                LoggingService.Instance.LogInfo($"已切換到腳本範圍: {scopeName}");
            }
        }

        /// <summary>
        /// 刪除腳本範圍
        /// </summary>
        /// <param name="scopeName">範圍名稱</param>
        public void RemoveScope(string scopeName)
        {
            lock (_lockObject)
            {
                if (scopeName == "default")
                {
                    throw new InvalidOperationException("不能刪除預設範圍");
                }

                if (_scopes.Remove(scopeName))
                {
                    ScriptScope? removedScope;
                    _scopes.TryGetValue(scopeName, out removedScope);
                    if (CurrentScope == removedScope)
                    {
                        SwitchToScope("default");
                    }
                    LoggingService.Instance.LogInfo($"已刪除腳本範圍: {scopeName}");
                }
            }
        }

        /// <summary>
        /// 自動註冊API服務並公開給腳本範圍
        /// </summary>
        private void AutoRegisterApiServices()
        {
            try
            {
                // 自動發現並註冊API服務
                var registeredCount = _apiRegistry.AutoDiscoverServices();
                LoggingService.Instance.LogInfo($"自動註冊了 {registeredCount} 個API服務");

                // 將所有已註冊的服務公開給當前腳本範圍
                RegisterServicesToCurrentScope();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"自動註冊API服務失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 將已註冊的服務公開給當前腳本範圍
        /// </summary>
        private void RegisterServicesToCurrentScope()
        {
            if (CurrentScope == null) return;

            foreach (var service in _apiRegistry.GetAllServices())
            {
                if (service.IsEnabled)
                {
                    var serviceInstance = _apiRegistry.GetService(service.ServiceName);
                    if (serviceInstance != null)
                    {
                        CurrentScope.SetVariable(service.ServiceName, serviceInstance);
                        LoggingService.Instance.LogInfo($"已將服務 '{service.ServiceName}' 公開給腳本範圍");
                    }
                }
            }
        }

        /// <summary>
        /// 註冊新的API服務
        /// </summary>
        /// <typeparam name="T">服務類型</typeparam>
        /// <param name="serviceInstance">服務實例</param>
        /// <param name="serviceName">服務名稱（可選）</param>
        /// <returns>是否註冊成功</returns>
        public bool RegisterApiService<T>(T serviceInstance, string? serviceName = null) where T : class
        {
            var success = _apiRegistry.RegisterService(serviceInstance, serviceName);
            if (success)
            {
                // 立即將服務公開給當前腳本範圍
                var actualServiceName = serviceName ?? _apiRegistry.GetServiceNames().LastOrDefault();
                if (actualServiceName != null && CurrentScope != null)
                {
                    CurrentScope.SetVariable(actualServiceName, serviceInstance);
                }
            }
            return success;
        }

        /// <summary>
        /// 卸載API服務
        /// </summary>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>是否卸載成功</returns>
        public bool UnregisterApiService(string serviceName)
        {
            var success = _apiRegistry.UnregisterService(serviceName);
            if (success && CurrentScope != null)
            {
                // 從腳本範圍中移除變數
                try
                {
                    CurrentScope.RemoveVariable(serviceName);
                }
                catch
                {
                    // 忽略移除變數的錯誤
                }
            }
            return success;
        }

        /// <summary>
        /// 獲取API註冊表
        /// </summary>
        public IApiRegistry ApiRegistry => _apiRegistry;

        /// <summary>
        /// 將 C# 物件公開給當前腳本範圍
        /// 符合需求規格書第 III.A.1.b 節和第 IV.B 節的主機物件模式
        /// </summary>
        /// <param name="variableName">在 IronPython 中的變數名稱</param>
        /// <param name="value">C# 物件實例</param>
        public void SetVariable(string variableName, object value)
        {
            if (CurrentScope == null)
            {
                throw new InvalidOperationException("目前沒有活動的腳本範圍");
            }

            try
            {
                CurrentScope.SetVariable(variableName, value);
                LoggingService.Instance.LogInfo($"已將變數 '{variableName}' 公開給腳本範圍");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"設定變數失敗 '{variableName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 從當前腳本範圍獲取變數值
        /// </summary>
        /// <typeparam name="T">期望的返回類型</typeparam>
        /// <param name="variableName">變數名稱</param>
        /// <returns>變數值</returns>
        public T? GetVariable<T>(string variableName)
        {
            if (CurrentScope == null)
            {
                throw new InvalidOperationException("目前沒有活動的腳本範圍");
            }

            try
            {
                if (CurrentScope.TryGetVariable(variableName, out var value))
                {
                    return (T?)value;
                }
                return default(T);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"獲取變數失敗 '{variableName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 執行 IronPython 腳本字串
        /// 符合需求規格書第 III.A.1.a 節關於動態載入和執行腳本的要求
        /// </summary>
        /// <param name="scriptCode">Python 腳本程式碼</param>
        /// <param name="scriptName">腳本名稱（用於錯誤報告）</param>
        /// <returns>執行結果</returns>
        public async Task<dynamic?> ExecuteScriptAsync(string scriptCode, string scriptName = "anonymous")
        {
            if (CurrentScope == null)
            {
                throw new InvalidOperationException("目前沒有活動的腳本範圍");
            }

            return await Task.Run(() =>
            {
                try
                {
                    LoggingService.Instance.LogInfo($"開始執行腳本: {scriptName}");

                    // 建立 ScriptSource (符合需求規格書第 IV.A 節)
                    var source = _engine.CreateScriptSourceFromString(scriptCode, scriptName);
                    
                    // 編譯腳本以檢查語法錯誤
                    var compiledCode = source.Compile();
                    
                    // 在當前範圍中執行
                    var result = compiledCode.Execute(CurrentScope);

                    LoggingService.Instance.LogInfo($"腳本執行完成: {scriptName}");
                    
                    // 觸發執行完成事件
                    ScriptExecuted?.Invoke(this, new ScriptExecutionEventArgs
                    {
                        ScriptName = scriptName,
                        Success = true,
                        Result = result
                    });

                    return result;
                }
                catch (SyntaxErrorException syntaxEx)
                {
                    var errorMsg = $"語法錯誤在第 {syntaxEx.Line} 行: {syntaxEx.Message}";
                    LoggingService.Instance.LogScriptError(scriptName, errorMsg, syntaxEx.Line);
                    
                    ScriptError?.Invoke(this, new ScriptErrorEventArgs
                    {
                        ScriptName = scriptName,
                        ErrorMessage = errorMsg,
                        LineNumber = syntaxEx.Line,
                        Exception = syntaxEx
                    });
                    
                    throw new ScriptExecutionException(errorMsg, syntaxEx);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"執行時錯誤: {ex.Message}";
                    LoggingService.Instance.LogScriptError(scriptName, errorMsg);
                    
                    ScriptError?.Invoke(this, new ScriptErrorEventArgs
                    {
                        ScriptName = scriptName,
                        ErrorMessage = errorMsg,
                        Exception = ex
                    });
                    
                    throw new ScriptExecutionException(errorMsg, ex);
                }
            });
        }

        /// <summary>
        /// 從檔案執行 IronPython 腳本
        /// </summary>
        /// <param name="scriptFilePath">腳本檔案路徑</param>
        /// <returns>執行結果</returns>
        public async Task<dynamic?> ExecuteScriptFileAsync(string scriptFilePath)
        {
            if (!File.Exists(scriptFilePath))
            {
                throw new FileNotFoundException($"腳本檔案不存在: {scriptFilePath}");
            }

            var scriptCode = File.ReadAllText(scriptFilePath);
            var scriptName = Path.GetFileName(scriptFilePath);
            
            return await ExecuteScriptAsync(scriptCode, scriptName);
        }

        /// <summary>
        /// 重置當前腳本範圍
        /// 符合需求規格書第 III.A.1.e 節關於重新初始化腳本環境的要求
        /// </summary>
        public void ResetCurrentScope()
        {
            if (CurrentScope == null) return;

            try
            {
                // 清除所有使用者定義的變數，但保留系統變數
                var variablesToRemove = new List<string>();
                foreach (var varName in CurrentScope.GetVariableNames())
                {
                    if (!varName.StartsWith("__"))
                    {
                        variablesToRemove.Add(varName);
                    }
                }

                foreach (var varName in variablesToRemove)
                {
                    CurrentScope.RemoveVariable(varName);
                }

                LoggingService.Instance.LogInfo("已重置當前腳本範圍");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"重置腳本範圍失敗: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 配置 Python 模組搜尋路徑
        /// 符合需求規格書第 III.A.1.e 節關於配置搜尋路徑的要求
        /// </summary>
        private void ConfigureSearchPaths()
        {
            try
            {
                var searchPaths = _engine.GetSearchPaths();
                
                // 添加應用程式腳本目錄
                var appPath = AppDomain.CurrentDomain.BaseDirectory;
                var scriptsPath = Path.Combine(appPath, "Scripts");
                
                if (Directory.Exists(scriptsPath))
                {
                    searchPaths.Add(scriptsPath);
                }

                // 添加使用者巨集目錄
                var userMacrosPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "WpfIronPythonApp",
                    "Macros");

                Directory.CreateDirectory(userMacrosPath);
                searchPaths.Add(userMacrosPath);

                _engine.SetSearchPaths(searchPaths);
                LoggingService.Instance.LogInfo("已配置 Python 模組搜尋路徑");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"配置搜尋路徑失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 獲取所有範圍名稱
        /// </summary>
        /// <returns>範圍名稱列表</returns>
        public IEnumerable<string> GetScopeNames()
        {
            lock (_lockObject)
            {
                return new List<string>(_scopes.Keys);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _scopes.Clear();
                CurrentScope = null;
                _disposed = true;
                LoggingService.Instance.LogInfo("IronPython 引擎已釋放");
            }
        }
    }

    /// <summary>
    /// 腳本執行事件參數
    /// </summary>
    public class ScriptExecutionEventArgs : EventArgs
    {
        public string ScriptName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public dynamic? Result { get; set; }
    }

    /// <summary>
    /// 腳本錯誤事件參數
    /// </summary>
    public class ScriptErrorEventArgs : EventArgs
    {
        public string ScriptName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public int? LineNumber { get; set; }
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// 腳本執行異常
    /// </summary>
    public class ScriptExecutionException : Exception
    {
        public ScriptExecutionException(string message) : base(message) { }
        public ScriptExecutionException(string message, Exception innerException) : base(message, innerException) { }
    }
} 