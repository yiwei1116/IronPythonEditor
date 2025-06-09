using System;
using System.Collections.Generic;
using System.Reflection;

namespace WpfIronPythonApp.Services.ApiRegistry
{
    /// <summary>
    /// API服務描述器
    /// </summary>
    public class ApiServiceDescriptor
    {
        /// <summary>
        /// 服務名稱
        /// </summary>
        public string ServiceName { get; set; } = "";

        /// <summary>
        /// 服務類型
        /// </summary>
        public Type ServiceType { get; set; } = null!;

        /// <summary>
        /// 服務實例
        /// </summary>
        public object? ServiceInstance { get; set; }

        /// <summary>
        /// 服務版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 服務描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 是否為核心服務
        /// </summary>
        public bool IsCore { get; set; } = false;

        /// <summary>
        /// 服務方法列表
        /// </summary>
        public List<ApiMethodDescriptor> Methods { get; set; } = new();

        /// <summary>
        /// 服務屬性列表
        /// </summary>
        public List<ApiPropertyDescriptor> Properties { get; set; } = new();

        /// <summary>
        /// 註冊時間
        /// </summary>
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否已啟用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// API方法描述器
    /// </summary>
    public class ApiMethodDescriptor
    {
        /// <summary>
        /// 方法名稱
        /// </summary>
        public string MethodName { get; set; } = "";

        /// <summary>
        /// 方法信息
        /// </summary>
        public MethodInfo MethodInfo { get; set; } = null!;

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

        /// <summary>
        /// 參數列表
        /// </summary>
        public List<ApiParameterDescriptor> Parameters { get; set; } = new();

        /// <summary>
        /// 返回值類型
        /// </summary>
        public Type ReturnType { get; set; } = typeof(void);

        /// <summary>
        /// 返回值描述
        /// </summary>
        public string ReturnDescription { get; set; } = "";
    }

    /// <summary>
    /// API屬性描述器
    /// </summary>
    public class ApiPropertyDescriptor
    {
        /// <summary>
        /// 屬性名稱
        /// </summary>
        public string PropertyName { get; set; } = "";

        /// <summary>
        /// 屬性信息
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; } = null!;

        /// <summary>
        /// 屬性描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 屬性類型
        /// </summary>
        public Type PropertyType { get; set; } = typeof(object);

        /// <summary>
        /// 是否可讀
        /// </summary>
        public bool CanRead { get; set; } = true;

        /// <summary>
        /// 是否可寫
        /// </summary>
        public bool CanWrite { get; set; } = false;

        /// <summary>
        /// 權限等級
        /// </summary>
        public ApiPermission Permission { get; set; } = ApiPermission.Standard;

        /// <summary>
        /// 使用範例
        /// </summary>
        public string Example { get; set; } = "";
    }

    /// <summary>
    /// API參數描述器
    /// </summary>
    public class ApiParameterDescriptor
    {
        /// <summary>
        /// 參數名稱
        /// </summary>
        public string ParameterName { get; set; } = "";

        /// <summary>
        /// 參數類型
        /// </summary>
        public Type ParameterType { get; set; } = typeof(object);

        /// <summary>
        /// 參數描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 是否為可選參數
        /// </summary>
        public bool IsOptional { get; set; } = false;

        /// <summary>
        /// 預設值
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// 預設值說明
        /// </summary>
        public string DefaultValueDescription { get; set; } = "";

        /// <summary>
        /// 參數範例
        /// </summary>
        public string Example { get; set; } = "";

        /// <summary>
        /// 參數位置
        /// </summary>
        public int Position { get; set; } = 0;
    }

    /// <summary>
    /// API事件描述器
    /// </summary>
    public class ApiEventDescriptor
    {
        /// <summary>
        /// 事件名稱
        /// </summary>
        public string EventName { get; set; } = "";

        /// <summary>
        /// 事件信息
        /// </summary>
        public EventInfo EventInfo { get; set; } = null!;

        /// <summary>
        /// 事件描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 事件委派類型
        /// </summary>
        public Type HandlerType { get; set; } = typeof(EventHandler);

        /// <summary>
        /// 使用範例
        /// </summary>
        public string Example { get; set; } = "";
    }
} 