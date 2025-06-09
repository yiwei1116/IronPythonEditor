using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfIronPythonApp.Services.ApiRegistry
{
    /// <summary>
    /// API註冊服務實現
    /// </summary>
    public class ApiRegistryService : IApiRegistry
    {
        private readonly ConcurrentDictionary<string, ApiServiceDescriptor> _services;
        private readonly ConcurrentDictionary<string, Func<object>> _serviceFactories;
        private readonly object _lock = new object();

        public event EventHandler<ServiceRegisteredEventArgs>? ServiceRegistered;
        public event EventHandler<ServiceUnregisteredEventArgs>? ServiceUnregistered;
        public event EventHandler<ServiceStateChangedEventArgs>? ServiceStateChanged;

        public ApiRegistryService()
        {
            _services = new ConcurrentDictionary<string, ApiServiceDescriptor>();
            _serviceFactories = new ConcurrentDictionary<string, Func<object>>();
        }

        public bool RegisterService<T>(T serviceInstance, string? serviceName = null) where T : class
        {
            if (serviceInstance == null) return false;

            var type = typeof(T);
            var actualServiceName = GetServiceName(type, serviceName);
            
            if (_services.ContainsKey(actualServiceName))
            {
                return false; // 服務已存在
            }

            var descriptor = CreateServiceDescriptor(type, serviceInstance, actualServiceName);
            
            lock (_lock)
            {
                if (_services.TryAdd(actualServiceName, descriptor))
                {
                    ServiceRegistered?.Invoke(this, new ServiceRegisteredEventArgs(actualServiceName, descriptor));
                    return true;
                }
            }

            return false;
        }

        public bool RegisterServiceType<T>(string? serviceName = null) where T : class, new()
        {
            return RegisterServiceFactory(() => new T(), serviceName);
        }

        public bool RegisterServiceFactory<T>(Func<T> factory, string? serviceName = null) where T : class
        {
            if (factory == null) return false;

            var type = typeof(T);
            var actualServiceName = GetServiceName(type, serviceName);

            if (_services.ContainsKey(actualServiceName))
            {
                return false; // 服務已存在
            }

            // 創建延遲實例化的描述器
            var descriptor = CreateServiceDescriptor(type, null, actualServiceName);
            
            lock (_lock)
            {
                if (_services.TryAdd(actualServiceName, descriptor) && 
                    _serviceFactories.TryAdd(actualServiceName, () => factory()))
                {
                    ServiceRegistered?.Invoke(this, new ServiceRegisteredEventArgs(actualServiceName, descriptor));
                    return true;
                }
            }

            return false;
        }

        public bool UnregisterService(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) return false;

            var descriptor = GetServiceDescriptor(serviceName);
            if (descriptor?.IsCore == true)
            {
                return false; // 核心服務不可卸載
            }

            lock (_lock)
            {
                var removed = _services.TryRemove(serviceName, out _);
                if (removed)
                {
                    _serviceFactories.TryRemove(serviceName, out _);
                    ServiceUnregistered?.Invoke(this, new ServiceUnregisteredEventArgs(serviceName));
                }
                return removed;
            }
        }

        public object? GetService(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) return null;

            var descriptor = GetServiceDescriptor(serviceName);
            if (descriptor == null || !descriptor.IsEnabled) return null;

            // 如果已有實例，直接返回
            if (descriptor.ServiceInstance != null)
            {
                return descriptor.ServiceInstance;
            }

            // 使用工廠方法創建實例
            if (_serviceFactories.TryGetValue(serviceName, out var factory))
            {
                lock (_lock)
                {
                    // 雙重檢查鎖定模式
                    if (descriptor.ServiceInstance == null)
                    {
                        descriptor.ServiceInstance = factory();
                    }
                    return descriptor.ServiceInstance;
                }
            }

            return null;
        }

        public T? GetService<T>(string serviceName) where T : class
        {
            return GetService(serviceName) as T;
        }

        public ApiServiceDescriptor? GetServiceDescriptor(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName)) return null;
            _services.TryGetValue(serviceName, out var descriptor);
            return descriptor;
        }

        public IReadOnlyList<ApiServiceDescriptor> GetAllServices()
        {
            return _services.Values.ToList().AsReadOnly();
        }

        public IReadOnlyList<string> GetServiceNames()
        {
            return _services.Keys.ToList().AsReadOnly();
        }

        public bool IsServiceRegistered(string serviceName)
        {
            return !string.IsNullOrWhiteSpace(serviceName) && _services.ContainsKey(serviceName);
        }

        public bool SetServiceEnabled(string serviceName, bool enabled)
        {
            var descriptor = GetServiceDescriptor(serviceName);
            if (descriptor == null) return false;

            if (descriptor.IsCore && !enabled)
            {
                return false; // 核心服務不可停用
            }

            descriptor.IsEnabled = enabled;
            ServiceStateChanged?.Invoke(this, new ServiceStateChangedEventArgs(serviceName, enabled));
            return true;
        }

        public int AutoDiscoverServices(params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
            {
                assemblies = new[] { Assembly.GetExecutingAssembly() };
            }

            int registeredCount = 0;

            foreach (var assembly in assemblies)
            {
                try
                {
                    var serviceTypes = assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<ApiServiceAttribute>() != null)
                        .Where(t => t.IsClass && !t.IsAbstract);

                    foreach (var serviceType in serviceTypes)
                    {
                        var attr = serviceType.GetCustomAttribute<ApiServiceAttribute>()!;
                        
                        try
                        {
                            // 檢查是否有無參構造函數
                            if (serviceType.GetConstructor(Type.EmptyTypes) != null)
                            {
                                var registerMethod = typeof(ApiRegistryService)
                                    .GetMethod(nameof(RegisterServiceType))!
                                    .MakeGenericMethod(serviceType);

                                var result = (bool)registerMethod.Invoke(this, new object[] { attr.ServiceName })!;
                                if (result) registeredCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            // 記錄錯誤但繼續處理其他服務
                            System.Diagnostics.Debug.WriteLine($"Failed to register service {serviceType.Name}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to scan assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return registeredCount;
        }

        public bool ValidatePermission(string serviceName, string methodName, ApiPermission requiredPermission)
        {
            var descriptor = GetServiceDescriptor(serviceName);
            if (descriptor == null) return false;

            var method = descriptor.Methods.FirstOrDefault(m => m.MethodName == methodName);
            if (method == null) return false;

            return method.Permission <= requiredPermission;
        }

        public string GenerateDocumentation(DocumentationFormat format = DocumentationFormat.Markdown)
        {
            return format switch
            {
                DocumentationFormat.Markdown => GenerateMarkdownDocumentation(),
                DocumentationFormat.Html => GenerateHtmlDocumentation(),
                DocumentationFormat.PlainText => GeneratePlainTextDocumentation(),
                _ => GenerateMarkdownDocumentation()
            };
        }

        public async Task<IntelliSenseData> GenerateIntelliSenseDataAsync()
        {
            return await Task.Run(() =>
            {
                var intelliSenseData = new IntelliSenseData();

                foreach (var service in _services.Values.Where(s => s.IsEnabled))
                {
                    // 生成方法補全項目
                    foreach (var method in service.Methods)
                    {
                        var completionItem = new CompletionItem
                        {
                            Label = $"{service.ServiceName}.{method.MethodName}",
                            Detail = method.Description,
                            Documentation = GenerateMethodDocumentation(method),
                            InsertText = GenerateMethodInsertText(service.ServiceName, method),
                            Kind = CompletionItemKind.Method
                        };
                        intelliSenseData.CompletionItems.Add(completionItem);

                        // 生成函數簽名
                        var signatureInfo = new SignatureInfo
                        {
                            Label = GenerateMethodSignature(service.ServiceName, method),
                            Documentation = method.Description,
                            Parameters = method.Parameters.Select(p => new ParameterInfo
                            {
                                Label = p.ParameterName,
                                Documentation = p.Description
                            }).ToList()
                        };
                        intelliSenseData.Signatures.Add(signatureInfo);
                    }

                    // 生成屬性補全項目
                    foreach (var property in service.Properties)
                    {
                        var completionItem = new CompletionItem
                        {
                            Label = $"{service.ServiceName}.{property.PropertyName}",
                            Detail = property.Description,
                            Documentation = property.Description,
                            InsertText = $"{service.ServiceName}.{property.PropertyName}",
                            Kind = CompletionItemKind.Property
                        };
                        intelliSenseData.CompletionItems.Add(completionItem);
                    }
                }

                return intelliSenseData;
            });
        }

        #region Private Methods

        private string GetServiceName(Type type, string? providedName)
        {
            if (!string.IsNullOrWhiteSpace(providedName))
                return providedName;

            var attr = type.GetCustomAttribute<ApiServiceAttribute>();
            if (attr != null && !string.IsNullOrWhiteSpace(attr.ServiceName))
                return attr.ServiceName;

            // 轉換為snake_case
            return ConvertToSnakeCase(type.Name.EndsWith("Service") 
                ? type.Name.Substring(0, type.Name.Length - 7) 
                : type.Name);
        }

        private string ConvertToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var result = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (i > 0 && char.IsUpper(input[i]))
                    result.Append('_');
                result.Append(char.ToLower(input[i]));
            }
            return result.ToString();
        }

        private ApiServiceDescriptor CreateServiceDescriptor(Type type, object? instance, string serviceName)
        {
            var attr = type.GetCustomAttribute<ApiServiceAttribute>();
            
            var descriptor = new ApiServiceDescriptor
            {
                ServiceName = serviceName,
                ServiceType = type,
                ServiceInstance = instance,
                Version = attr?.Version ?? "1.0.0",
                Description = attr?.Description ?? "",
                IsCore = attr?.IsCore ?? false
            };

            // 分析方法
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<ApiMethodAttribute>() != null)
                .ToArray();

            foreach (var method in methods)
            {
                var methodAttr = method.GetCustomAttribute<ApiMethodAttribute>()!;
                var methodDescriptor = new ApiMethodDescriptor
                {
                    MethodName = method.Name,
                    MethodInfo = method,
                    Description = methodAttr.Description,
                    Example = methodAttr.Example,
                    IsAsync = methodAttr.IsAsync,
                    Permission = methodAttr.Permission,
                    IsDeprecated = methodAttr.IsDeprecated,
                    DeprecationMessage = methodAttr.DeprecationMessage,
                    Category = methodAttr.Category,
                    ReturnType = method.ReturnType
                };

                // 分析參數
                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var paramAttr = param.GetCustomAttribute<ApiParameterAttribute>();
                    
                    var paramDescriptor = new ApiParameterDescriptor
                    {
                        ParameterName = param.Name ?? "",
                        ParameterType = param.ParameterType,
                        Description = paramAttr?.Description ?? "",
                        IsOptional = param.IsOptional || paramAttr?.IsOptional == true,
                        DefaultValue = param.DefaultValue,
                        DefaultValueDescription = paramAttr?.DefaultValue ?? "",
                        Example = paramAttr?.Example ?? "",
                        Position = i
                    };
                    methodDescriptor.Parameters.Add(paramDescriptor);
                }

                descriptor.Methods.Add(methodDescriptor);
            }

            // 分析屬性
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<ApiMethodAttribute>() != null)
                .ToArray();

            foreach (var property in properties)
            {
                var propAttr = property.GetCustomAttribute<ApiMethodAttribute>()!;
                var propDescriptor = new ApiPropertyDescriptor
                {
                    PropertyName = property.Name,
                    PropertyInfo = property,
                    Description = propAttr.Description,
                    PropertyType = property.PropertyType,
                    CanRead = property.CanRead,
                    CanWrite = property.CanWrite,
                    Permission = propAttr.Permission,
                    Example = propAttr.Example
                };
                descriptor.Properties.Add(propDescriptor);
            }

            return descriptor;
        }

        private string GenerateMarkdownDocumentation()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# API Documentation");
            sb.AppendLine();
            sb.AppendLine($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            foreach (var service in _services.Values.OrderBy(s => s.ServiceName))
            {
                sb.AppendLine($"## {service.ServiceName}");
                sb.AppendLine();
                sb.AppendLine($"**Version:** {service.Version}");
                sb.AppendLine($"**Description:** {service.Description}");
                sb.AppendLine($"**Type:** {service.ServiceType.FullName}");
                sb.AppendLine();

                if (service.Methods.Any())
                {
                    sb.AppendLine("### Methods");
                    sb.AppendLine();

                    foreach (var method in service.Methods.OrderBy(m => m.MethodName))
                    {
                        sb.AppendLine($"#### {method.MethodName}");
                        sb.AppendLine();
                        sb.AppendLine($"**Description:** {method.Description}");
                        
                        if (method.Parameters.Any())
                        {
                            sb.AppendLine();
                            sb.AppendLine("**Parameters:**");
                            foreach (var param in method.Parameters)
                            {
                                sb.AppendLine($"- `{param.ParameterName}` ({param.ParameterType.Name}): {param.Description}");
                            }
                        }

                        if (!string.IsNullOrEmpty(method.Example))
                        {
                            sb.AppendLine();
                            sb.AppendLine("**Example:**");
                            sb.AppendLine("```python");
                            sb.AppendLine(method.Example);
                            sb.AppendLine("```");
                        }

                        sb.AppendLine();
                    }
                }

                if (service.Properties.Any())
                {
                    sb.AppendLine("### Properties");
                    sb.AppendLine();

                    foreach (var property in service.Properties.OrderBy(p => p.PropertyName))
                    {
                        sb.AppendLine($"#### {property.PropertyName}");
                        sb.AppendLine();
                        sb.AppendLine($"**Type:** {property.PropertyType.Name}");
                        sb.AppendLine($"**Description:** {property.Description}");
                        sb.AppendLine($"**Read:** {property.CanRead}, **Write:** {property.CanWrite}");
                        sb.AppendLine();
                    }
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GenerateHtmlDocumentation()
        {
            // 簡化的HTML生成
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><title>API Documentation</title></head><body>");
            sb.AppendLine("<h1>API Documentation</h1>");
            
            foreach (var service in _services.Values.OrderBy(s => s.ServiceName))
            {
                sb.AppendLine($"<h2>{service.ServiceName}</h2>");
                sb.AppendLine($"<p><strong>Description:</strong> {service.Description}</p>");
                
                if (service.Methods.Any())
                {
                    sb.AppendLine("<h3>Methods</h3>");
                    foreach (var method in service.Methods)
                    {
                        sb.AppendLine($"<h4>{method.MethodName}</h4>");
                        sb.AppendLine($"<p>{method.Description}</p>");
                    }
                }
            }
            
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private string GeneratePlainTextDocumentation()
        {
            var sb = new StringBuilder();
            sb.AppendLine("API DOCUMENTATION");
            sb.AppendLine(new string('=', 50));
            
            foreach (var service in _services.Values.OrderBy(s => s.ServiceName))
            {
                sb.AppendLine($"\n{service.ServiceName}");
                sb.AppendLine(new string('-', service.ServiceName.Length));
                sb.AppendLine($"Description: {service.Description}");
                
                foreach (var method in service.Methods)
                {
                    sb.AppendLine($"  {method.MethodName}: {method.Description}");
                }
            }
            
            return sb.ToString();
        }

        private string GenerateMethodDocumentation(ApiMethodDescriptor method)
        {
            var sb = new StringBuilder();
            sb.AppendLine(method.Description);
            
            if (method.Parameters.Any())
            {
                sb.AppendLine("\nParameters:");
                foreach (var param in method.Parameters)
                {
                    sb.AppendLine($"  {param.ParameterName} ({param.ParameterType.Name}): {param.Description}");
                }
            }
            
            return sb.ToString();
        }

        private string GenerateMethodInsertText(string serviceName, ApiMethodDescriptor method)
        {
            var parameters = string.Join(", ", method.Parameters.Select(p => p.ParameterName));
            return $"{serviceName}.{method.MethodName}({parameters})";
        }

        private string GenerateMethodSignature(string serviceName, ApiMethodDescriptor method)
        {
            var parameters = string.Join(", ", method.Parameters.Select(p => 
                $"{p.ParameterName}: {GetPythonTypeName(p.ParameterType)}"));
            return $"{serviceName}.{method.MethodName}({parameters})";
        }

        private string GetPythonTypeName(Type type)
        {
            if (type == typeof(string)) return "str";
            if (type == typeof(int)) return "int";
            if (type == typeof(float) || type == typeof(double)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(void)) return "None";
            return type.Name;
        }

        #endregion
    }
} 