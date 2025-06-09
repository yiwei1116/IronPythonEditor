using System;

namespace WpfIronPythonApp.Services.ApiRegistry
{
    /// <summary>
    /// 標記API服務類的屬性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ApiServiceAttribute : Attribute
    {
        /// <summary>
        /// 服務名稱（在Python中使用的變數名）
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// 服務版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 服務描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 是否為核心服務（不可卸載）
        /// </summary>
        public bool IsCore { get; set; } = false;

        public ApiServiceAttribute(string serviceName)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        }
    }

    /// <summary>
    /// 標記可公開給Python的方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class ApiMethodAttribute : Attribute
    {
        /// <summary>
        /// 方法描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 使用範例
        /// </summary>
        public string Example { get; set; } = "";

        /// <summary>
        /// 是否為異步方法
        /// </summary>
        public bool IsAsync { get; set; } = false;

        /// <summary>
        /// 權限等級
        /// </summary>
        public ApiPermission Permission { get; set; } = ApiPermission.Standard;

        /// <summary>
        /// 是否已棄用
        /// </summary>
        public bool IsDeprecated { get; set; } = false;

        /// <summary>
        /// 棄用訊息
        /// </summary>
        public string DeprecationMessage { get; set; } = "";

        /// <summary>
        /// 方法分類
        /// </summary>
        public string Category { get; set; } = "General";
    }

    /// <summary>
    /// 標記方法參數
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ApiParameterAttribute : Attribute
    {
        /// <summary>
        /// 參數描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 是否為可選參數
        /// </summary>
        public bool IsOptional { get; set; } = false;

        /// <summary>
        /// 預設值說明
        /// </summary>
        public string DefaultValue { get; set; } = "";

        /// <summary>
        /// 參數範例
        /// </summary>
        public string Example { get; set; } = "";
    }

    /// <summary>
    /// API權限等級
    /// </summary>
    public enum ApiPermission
    {
        /// <summary>
        /// 標準權限 - 一般功能
        /// </summary>
        Standard = 0,

        /// <summary>
        /// 檔案存取權限
        /// </summary>
        FileAccess = 1,

        /// <summary>
        /// 網路存取權限
        /// </summary>
        NetworkAccess = 2,

        /// <summary>
        /// 系統操作權限
        /// </summary>
        SystemAccess = 3,

        /// <summary>
        /// 管理員權限
        /// </summary>
        Administrative = 4
    }
} 