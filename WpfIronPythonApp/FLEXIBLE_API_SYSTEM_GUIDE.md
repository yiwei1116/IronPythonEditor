# 彈性API系統指南

## 概述

本專案已重構為使用高度彈性的API註冊系統，讓開發者可以輕鬆新增、管理和擴展C# API，並自動生成文件和IntelliSense支援。

## 系統架構

### 核心組件

1. **API註冊系統** (`Services/ApiRegistry/`)
   - `IApiRegistry` - API註冊介面
   - `ApiRegistryService` - API註冊服務實現
   - `ApiAttributes` - API標註屬性
   - `ApiDescriptor` - API描述器

2. **自動發現機制**
   - 使用反射自動掃描標記的API服務
   - 支援屬性驅動的API配置
   - 動態服務註冊和卸載

3. **文件生成系統**
   - 支援多種格式：Markdown、JSON、HTML、純文字
   - 自動從屬性提取API文件
   - 包含範例代碼和參數說明

4. **IntelliSense整合**
   - 從API註冊系統自動生成補全數據
   - 動態更新IntelliSense內容
   - 支援方法簽名和參數提示

## 如何新增API服務

### 步驟1：創建API服務類

```csharp
using WpfIronPythonApp.Services.ApiRegistry;

[ApiService("my_service", Description = "我的自定義服務", Version = "1.0.0")]
public class MyCustomService
{
    [ApiMethod(Description = "執行某個操作", 
               Example = "my_service.do_something('hello')", 
               Category = "Operations")]
    public string do_something([ApiParameter(Description = "輸入文字")] string input)
    {
        return $"處理結果: {input}";
    }

    [ApiMethod(Description = "計算兩數之和", 
               Example = "result = my_service.add(5, 3)", 
               Category = "Math")]
    public int add([ApiParameter(Description = "第一個數字")] int a,
                   [ApiParameter(Description = "第二個數字")] int b)
    {
        return a + b;
    }
}
```

### 步驟2：註冊服務

有三種方式註冊服務：

#### 方式1：自動發現（推薦）
```csharp
// 系統會自動掃描並註冊所有標記了 [ApiService] 的類
var registeredCount = apiRegistry.AutoDiscoverServices();
```

#### 方式2：手動註冊實例
```csharp
var myService = new MyCustomService();
apiRegistry.RegisterService(myService, "my_service");
```

#### 方式3：註冊類型（延遲實例化）
```csharp
apiRegistry.RegisterServiceType<MyCustomService>("my_service");
```

### 步驟3：在Python中使用

```python
# 使用新註冊的API
result = my_service.do_something("Hello World")
print(result)  # 輸出: 處理結果: Hello World

# 數學運算
sum_result = my_service.add(10, 20)
print(f"Sum: {sum_result}")  # 輸出: Sum: 30
```

## API屬性說明

### ApiService 屬性

```csharp
[ApiService(serviceName, 
    Description = "服務描述",
    Version = "版本號", 
    IsCore = false)]  // 是否為核心服務（不可卸載）
```

### ApiMethod 屬性

```csharp
[ApiMethod(Description = "方法描述",
    Example = "使用範例",
    Category = "分類",
    Permission = ApiPermission.Standard,  // 權限等級
    IsAsync = false,                      // 是否為異步方法
    IsDeprecated = false,                 // 是否已棄用
    DeprecationMessage = "棄用訊息")]
```

### ApiParameter 屬性

```csharp
[ApiParameter(Description = "參數描述",
    Example = "參數範例",
    IsOptional = false,                   // 是否為可選參數
    DefaultValue = "預設值說明")]
```

### 權限等級

```csharp
public enum ApiPermission
{
    Standard = 0,        // 標準權限
    FileAccess = 1,      // 檔案存取權限
    NetworkAccess = 2,   // 網路存取權限
    SystemAccess = 3,    // 系統操作權限
    Administrative = 4   // 管理員權限
}
```

## 內建API服務

### 1. 主機服務 (host)
```python
host.log("記錄訊息")
doc = host.active_doc
```

### 2. UI控制服務 (ui)
```python
ui.show_message("Hello", "標題")
ui.status_bar("狀態訊息")
```

### 3. 資料服務 (data)
```python
table = data.load_csv("data.csv")
cleaned = data.process_table(table)
```

### 4. 檔案系統服務 (fs)
```python
content = fs.read_text("file.txt")
fs.write_text("output.txt", "Hello World")
files = fs.list_files("C:/temp", "*.txt")
```

### 5. 數學服務 (math)
```python
avg = math.average([1, 2, 3, 4, 5])
stats = math.statistics([1, 2, 3, 4, 5])
fib = math.fibonacci(10)
```

## API管理器

### 功能特色

1. **服務管理**
   - 查看所有已註冊的API服務
   - 啟用/停用服務
   - 搜尋和篩選服務

2. **詳細資訊**
   - 服務基本資訊（名稱、版本、描述）
   - 方法列表和參數詳情
   - 屬性列表

3. **文件生成**
   - 支援多種格式輸出
   - 即時預覽
   - 一鍵複製和匯出

### 開啟API管理器

- 菜單：工具 → API管理器
- 或在代碼中：
```csharp
var apiManager = new ApiManagerWindow(apiRegistry);
apiManager.Show();
```

## 文件生成

### 支援格式

1. **Markdown** - 適合GitHub和文件網站
2. **JSON** - 適合程式化處理
3. **HTML** - 適合網頁顯示
4. **純文字** - 適合簡單查看

### 生成方式

#### 程式化生成
```csharp
var documentation = apiRegistry.GenerateDocumentation(DocumentationFormat.Markdown);
File.WriteAllText("api_docs.md", documentation);
```

#### UI生成
- 菜單：工具 → 生成API文件
- API管理器中的生成按鈕

## IntelliSense整合

### 自動更新

系統會自動從API註冊表生成IntelliSense數據：

```csharp
var intelliSenseData = await apiRegistry.GenerateIntelliSenseDataAsync();
```

### 手動重新載入

- 菜單：工具 → 重新載入IntelliSense
- 或在代碼中調用 `UpdateIntelliSenseFromApiRegistry()`

## 高級功能

### 服務工廠

支援使用工廠方法註冊服務：

```csharp
apiRegistry.RegisterServiceFactory(() => new MyService(config), "my_service");
```

### 事件系統

監聽API註冊事件：

```csharp
apiRegistry.ServiceRegistered += (sender, e) => {
    Console.WriteLine($"服務已註冊: {e.ServiceName}");
};

apiRegistry.ServiceUnregistered += (sender, e) => {
    Console.WriteLine($"服務已卸載: {e.ServiceName}");
};
```

### 權限驗證

```csharp
bool hasPermission = apiRegistry.ValidatePermission(
    "fs", "delete_file", ApiPermission.FileAccess);
```

## 最佳實踐

### 1. 命名規範

- 服務名稱使用 snake_case：`file_system`, `math_utils`
- 方法名稱使用 snake_case：`read_file`, `calculate_sum`
- 參數名稱使用 snake_case：`file_path`, `input_data`

### 2. 文件撰寫

- 提供清晰的描述
- 包含實用的範例
- 說明參數用途和格式
- 標明權限需求

### 3. 錯誤處理

```csharp
[ApiMethod(Description = "安全的檔案讀取")]
public string safe_read_file(string path)
{
    try
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"檔案不存在: {path}");
        
        return File.ReadAllText(path);
    }
    catch (Exception ex)
    {
        LoggingService.Instance.LogError($"讀取檔案失敗: {ex.Message}");
        throw;
    }
}
```

### 4. 版本管理

- 使用語義化版本號：`1.0.0`, `1.1.0`, `2.0.0`
- 重大變更時增加主版本號
- 新功能時增加次版本號
- 錯誤修復時增加修訂版本號

## 擴展範例

### 創建網路服務

```csharp
[ApiService("http", Description = "HTTP客戶端服務", Version = "1.0.0")]
public class HttpService
{
    private readonly HttpClient _client = new HttpClient();

    [ApiMethod(Description = "發送GET請求", 
               Permission = ApiPermission.NetworkAccess)]
    public async Task<string> get_async(string url)
    {
        var response = await _client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    [ApiMethod(Description = "發送POST請求", 
               Permission = ApiPermission.NetworkAccess)]
    public async Task<string> post_async(string url, string data)
    {
        var content = new StringContent(data);
        var response = await _client.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }
}
```

### 創建資料庫服務

```csharp
[ApiService("db", Description = "資料庫操作服務", Version = "1.0.0")]
public class DatabaseService
{
    [ApiMethod(Description = "執行查詢", 
               Permission = ApiPermission.SystemAccess)]
    public DataTable query(string sql, params object[] parameters)
    {
        // 實現資料庫查詢邏輯
        return new DataTable();
    }

    [ApiMethod(Description = "執行非查詢命令", 
               Permission = ApiPermission.SystemAccess)]
    public int execute(string sql, params object[] parameters)
    {
        // 實現資料庫執行邏輯
        return 0;
    }
}
```

## 故障排除

### 常見問題

1. **服務未自動註冊**
   - 檢查是否有 `[ApiService]` 屬性
   - 確認類別是 public 且有無參構造函數
   - 檢查組件是否被掃描

2. **IntelliSense不顯示**
   - 嘗試重新載入IntelliSense
   - 檢查服務是否已啟用
   - 確認方法有 `[ApiMethod]` 屬性

3. **文件生成失敗**
   - 檢查檔案寫入權限
   - 確認路徑存在
   - 查看錯誤日誌

### 調試技巧

```csharp
// 查看已註冊的服務
var services = apiRegistry.GetAllServices();
foreach (var service in services)
{
    Console.WriteLine($"服務: {service.ServiceName}, 狀態: {service.IsEnabled}");
}

// 檢查特定服務
var serviceDesc = apiRegistry.GetServiceDescriptor("my_service");
if (serviceDesc != null)
{
    Console.WriteLine($"方法數量: {serviceDesc.Methods.Count}");
}
```

## 結論

新的彈性API系統提供了：

- **易於擴展** - 只需添加屬性即可註冊新API
- **自動化** - 自動生成文件和IntelliSense
- **管理友好** - 圖形化API管理界面
- **向後相容** - 保持現有API的正常運作
- **高度彈性** - 支援多種註冊和配置方式

這個系統讓開發者可以專注於業務邏輯，而不需要擔心API註冊、文件生成和IntelliSense配置的複雜性。 