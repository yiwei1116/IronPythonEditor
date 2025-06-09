using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WpfIronPythonApp.Services.ApiRegistry
{
    /// <summary>
    /// API註冊表介面
    /// </summary>
    public interface IApiRegistry
    {
        /// <summary>
        /// 註冊API服務
        /// </summary>
        /// <typeparam name="T">服務類型</typeparam>
        /// <param name="serviceInstance">服務實例</param>
        /// <param name="serviceName">服務名稱（可選，不提供時使用屬性或類型名稱）</param>
        /// <returns>是否註冊成功</returns>
        bool RegisterService<T>(T serviceInstance, string? serviceName = null) where T : class;

        /// <summary>
        /// 註冊API服務類型（稍後實例化）
        /// </summary>
        /// <typeparam name="T">服務類型</typeparam>
        /// <param name="serviceName">服務名稱（可選）</param>
        /// <returns>是否註冊成功</returns>
        bool RegisterServiceType<T>(string? serviceName = null) where T : class, new();

        /// <summary>
        /// 註冊API服務類型（使用工廠方法）
        /// </summary>
        /// <typeparam name="T">服務類型</typeparam>
        /// <param name="factory">工廠方法</param>
        /// <param name="serviceName">服務名稱（可選）</param>
        /// <returns>是否註冊成功</returns>
        bool RegisterServiceFactory<T>(Func<T> factory, string? serviceName = null) where T : class;

        /// <summary>
        /// 卸載API服務
        /// </summary>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>是否卸載成功</returns>
        bool UnregisterService(string serviceName);

        /// <summary>
        /// 獲取API服務
        /// </summary>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>服務實例</returns>
        object? GetService(string serviceName);

        /// <summary>
        /// 獲取API服務（泛型版本）
        /// </summary>
        /// <typeparam name="T">服務類型</typeparam>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>服務實例</returns>
        T? GetService<T>(string serviceName) where T : class;

        /// <summary>
        /// 獲取API服務描述器
        /// </summary>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>服務描述器</returns>
        ApiServiceDescriptor? GetServiceDescriptor(string serviceName);

        /// <summary>
        /// 獲取所有已註冊的服務
        /// </summary>
        /// <returns>服務列表</returns>
        IReadOnlyList<ApiServiceDescriptor> GetAllServices();

        /// <summary>
        /// 獲取服務名稱列表
        /// </summary>
        /// <returns>服務名稱列表</returns>
        IReadOnlyList<string> GetServiceNames();

        /// <summary>
        /// 檢查服務是否已註冊
        /// </summary>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>是否已註冊</returns>
        bool IsServiceRegistered(string serviceName);

        /// <summary>
        /// 啟用或停用服務
        /// </summary>
        /// <param name="serviceName">服務名稱</param>
        /// <param name="enabled">是否啟用</param>
        /// <returns>是否操作成功</returns>
        bool SetServiceEnabled(string serviceName, bool enabled);

        /// <summary>
        /// 自動發現並註冊API服務
        /// </summary>
        /// <param name="assemblies">要掃描的組件（可選，不提供時掃描當前組件）</param>
        /// <returns>註冊的服務數量</returns>
        int AutoDiscoverServices(params System.Reflection.Assembly[] assemblies);

        /// <summary>
        /// 驗證API權限
        /// </summary>
        /// <param name="serviceName">服務名稱</param>
        /// <param name="methodName">方法名稱</param>
        /// <param name="requiredPermission">所需權限</param>
        /// <returns>是否有權限</returns>
        bool ValidatePermission(string serviceName, string methodName, ApiPermission requiredPermission);

        /// <summary>
        /// 生成API文件
        /// </summary>
        /// <param name="format">文件格式（markdown, json, xml等）</param>
        /// <returns>API文件內容</returns>
        string GenerateDocumentation(DocumentationFormat format = DocumentationFormat.Markdown);

        /// <summary>
        /// 生成IntelliSense數據
        /// </summary>
        /// <returns>IntelliSense數據</returns>
        Task<IntelliSenseData> GenerateIntelliSenseDataAsync();

        /// <summary>
        /// 服務註冊事件
        /// </summary>
        event EventHandler<ServiceRegisteredEventArgs>? ServiceRegistered;

        /// <summary>
        /// 服務卸載事件
        /// </summary>
        event EventHandler<ServiceUnregisteredEventArgs>? ServiceUnregistered;

        /// <summary>
        /// 服務狀態改變事件
        /// </summary>
        event EventHandler<ServiceStateChangedEventArgs>? ServiceStateChanged;
    }

    /// <summary>
    /// 文件格式
    /// </summary>
    public enum DocumentationFormat
    {
        Markdown,
        Json,
        Xml,
        Html,
        PlainText
    }

    /// <summary>
    /// IntelliSense數據
    /// </summary>
    public class IntelliSenseData
    {
        /// <summary>
        /// 補全項目列表
        /// </summary>
        public List<CompletionItem> CompletionItems { get; set; } = new();

        /// <summary>
        /// 函數簽名列表
        /// </summary>
        public List<SignatureInfo> Signatures { get; set; } = new();

        /// <summary>
        /// 懸停信息
        /// </summary>
        public Dictionary<string, HoverInfo> HoverInfos { get; set; } = new();

        /// <summary>
        /// 生成時間
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 補全項目
    /// </summary>
    public class CompletionItem
    {
        public string Label { get; set; } = "";
        public string Detail { get; set; } = "";
        public string Documentation { get; set; } = "";
        public string InsertText { get; set; } = "";
        public CompletionItemKind Kind { get; set; } = CompletionItemKind.Method;
    }

    /// <summary>
    /// 補全項目類型
    /// </summary>
    public enum CompletionItemKind
    {
        Method,
        Property,
        Event,
        Class,
        Variable,
        Keyword
    }

    /// <summary>
    /// 函數簽名信息
    /// </summary>
    public class SignatureInfo
    {
        public string Label { get; set; } = "";
        public string Documentation { get; set; } = "";
        public List<ParameterInfo> Parameters { get; set; } = new();
    }

    /// <summary>
    /// 參數信息
    /// </summary>
    public class ParameterInfo
    {
        public string Label { get; set; } = "";
        public string Documentation { get; set; } = "";
    }

    /// <summary>
    /// 懸停信息
    /// </summary>
    public class HoverInfo
    {
        public string Content { get; set; } = "";
        public string Documentation { get; set; } = "";
    }

    /// <summary>
    /// 服務註冊事件參數
    /// </summary>
    public class ServiceRegisteredEventArgs : EventArgs
    {
        public string ServiceName { get; }
        public ApiServiceDescriptor ServiceDescriptor { get; }

        public ServiceRegisteredEventArgs(string serviceName, ApiServiceDescriptor serviceDescriptor)
        {
            ServiceName = serviceName;
            ServiceDescriptor = serviceDescriptor;
        }
    }

    /// <summary>
    /// 服務卸載事件參數
    /// </summary>
    public class ServiceUnregisteredEventArgs : EventArgs
    {
        public string ServiceName { get; }

        public ServiceUnregisteredEventArgs(string serviceName)
        {
            ServiceName = serviceName;
        }
    }

    /// <summary>
    /// 服務狀態改變事件參數
    /// </summary>
    public class ServiceStateChangedEventArgs : EventArgs
    {
        public string ServiceName { get; }
        public bool IsEnabled { get; }

        public ServiceStateChangedEventArgs(string serviceName, bool isEnabled)
        {
            ServiceName = serviceName;
            IsEnabled = isEnabled;
        }
    }
} 