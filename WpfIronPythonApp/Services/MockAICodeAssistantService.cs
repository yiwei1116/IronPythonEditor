using System;
using System.Threading.Tasks;

namespace WpfIronPythonApp.Services
{
    /// <summary>
    /// AI 程式碼助手服務的模擬實現
    /// 在真實環境中，這裡會整合實際的AI服務（如OpenAI API、Azure OpenAI等）
    /// </summary>
    public class MockAICodeAssistantService : IAICodeAssistantService
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// 模擬AI建議
        /// </summary>
        public async Task<string> GetCodeSuggestionAsync(string selectedCode, string context = "")
        {
            // 模擬AI處理時間
            await Task.Delay(10000 + _random.Next(1000));

            LoggingService.Instance.LogInfo($"AI請求: 為程式碼提供建議 - {selectedCode?.Substring(0, Math.Min(50, selectedCode?.Length ?? 0))}...");

            // 根據選取的程式碼提供模擬建議
            if (string.IsNullOrWhiteSpace(selectedCode))
            {
                return "# AI 建議: 開始寫一些Python程式碼\nprint('Hello, IronPython!')";
            }

            // 簡單的模式匹配來提供相關建議
            if (selectedCode.Contains("print"))
            {
                return "# AI 建議: 改進的輸出格式\nprint(f\"結果: {result}\")\nhost.log(\"已完成輸出操作\")";
            }
            else if (selectedCode.Contains("for") || selectedCode.Contains("while"))
            {
                return "# AI 建議: 迴圈優化\n# 考慮使用列表推導式或內建函數來提高效率\nresult = [process_item(item) for item in items if condition(item)]";
            }
            else if (selectedCode.Contains("def"))
            {
                return "# AI 建議: 函數改進\n# 添加類型提示和文檔字串\ndef improved_function(param: str) -> str:\n    \"\"\"改進的函數說明\"\"\"\n    return param.upper()";
            }
            else if (selectedCode.Contains("import"))
            {
                return "# AI 建議: 使用可用的API\nhost.log('開始處理')\ndata = data.load_csv('input.csv')\nui.show_message('處理完成', '通知')";
            }
            else if (selectedCode.Contains("try"))
            {
                return "# AI 建議: 更好的異常處理\ntry:\n    # 您的程式碼\n    pass\nexcept SpecificException as e:\n    host.log(f'特定錯誤: {e}')\nexcept Exception as e:\n    host.log(f'未預期錯誤: {e}')\n    raise";
            }
            else
            {
                return GenerateContextualSuggestion(selectedCode);
            }
        }

        /// <summary>
        /// 優化程式碼
        /// </summary>
        public async Task<string> OptimizeCodeAsync(string code)
        {
            await Task.Delay(800 + _random.Next(700));

            LoggingService.Instance.LogInfo("AI請求: 優化程式碼");

            if (string.IsNullOrWhiteSpace(code))
            {
                return "# 沒有程式碼需要優化";
            }

            return $"# AI 優化建議:\n# 原始程式碼:\n# {code.Replace("\n", "\n# ")}\n\n# 優化後的程式碼:\n{GenerateOptimizedCode(code)}";
        }

        /// <summary>
        /// 解釋程式碼
        /// </summary>
        public async Task<string> ExplainCodeAsync(string code)
        {
            await Task.Delay(600 + _random.Next(500));

            LoggingService.Instance.LogInfo("AI請求: 解釋程式碼");

            if (string.IsNullOrWhiteSpace(code))
            {
                return "# 沒有程式碼需要解釋";
            }

            return $"# AI 程式碼解釋:\n# 這段程式碼的功能是執行 Python 操作\n# 主要用途: 資料處理和系統互動\n# 建議: 添加更多註解以提高可讀性\n\n# 原始程式碼:\n{code}";
        }

        /// <summary>
        /// 修復程式碼錯誤
        /// </summary>
        public async Task<string> FixCodeAsync(string code, string errorMessage = "")
        {
            await Task.Delay(1200 + _random.Next(800));

            LoggingService.Instance.LogInfo($"AI請求: 修復程式碼錯誤 - {errorMessage}");

            if (string.IsNullOrWhiteSpace(code))
            {
                return "# 沒有程式碼需要修復";
            }

            return $"# AI 錯誤修復建議:\n# 錯誤訊息: {errorMessage}\n# 修復後的程式碼:\n{GenerateFixedCode(code, errorMessage)}";
        }

        /// <summary>
        /// 根據程式碼內容生成上下文相關建議
        /// </summary>
        private string GenerateContextualSuggestion(string selectedCode)
        {
            var suggestions = new[]
            {
                "# AI 建議: 使用主機API\nhost.log('開始處理')\nresult = data.load_csv('data.csv')\nui.status_bar('處理完成')",
                "# AI 建議: 資料處理流程\ndata_table = data.load_csv('input.csv')\nprocessed = data.process_table(data_table)\nhost.log(f'處理了 {processed.Rows.Count} 行資料')",
                "# AI 建議: 檔案操作\ndocument = host.active_doc\nif document:\n    document.save()\n    ui.show_message('檔案已儲存', '成功')",
                "# AI 建議: 迴圈處理\nitems = ['item1', 'item2', 'item3']\nfor item in items:\n    host.log(f'處理項目: {item}')\n    # 處理邏輯\nui.show_message('所有項目處理完成', '完成')"
            };

            return suggestions[_random.Next(suggestions.Length)];
        }

        /// <summary>
        /// 生成優化後的程式碼
        /// </summary>
        private string GenerateOptimizedCode(string originalCode)
        {
            // 簡單的優化建議
            if (originalCode.Contains("print"))
            {
                return "# 使用日誌記錄替代 print\nhost.log('程式執行狀態')\nui.status_bar('更新狀態列')";
            }
            else if (originalCode.Contains("for"))
            {
                return "# 使用更高效的迭代方式\nresults = [process(item) for item in items if is_valid(item)]\nhost.log(f'處理了 {len(results)} 個項目')";
            }
            else
            {
                return $"# 建議重構程式碼以提高可讀性\n# 原始邏輯保持不變，但增加了註解和錯誤處理\ntry:\n    {originalCode.Replace("\n", "\n    ")}\nexcept Exception as e:\n    host.log(f'執行錯誤: {{e}}')";
            }
        }

        /// <summary>
        /// 生成修復後的程式碼
        /// </summary>
        private string GenerateFixedCode(string originalCode, string errorMessage)
        {
            if (errorMessage.Contains("NameError"))
            {
                return $"# 修復變數未定義錯誤\n# 添加必要的變數定義\nvariable = None  # 定義缺失的變數\n{originalCode}";
            }
            else if (errorMessage.Contains("IndentationError"))
            {
                return $"# 修復縮排錯誤\n# 正確的縮排版本:\n{originalCode.Replace("\n", "\n    ")}";
            }
            else if (errorMessage.Contains("SyntaxError"))
            {
                return $"# 修復語法錯誤\n# 檢查括號、冒號和引號的配對\ntry:\n    {originalCode}\nexcept SyntaxError as e:\n    host.log(f'語法錯誤: {{e}}')";
            }
            else
            {
                return $"# 通用錯誤修復\ntry:\n    {originalCode}\nexcept Exception as e:\n    host.log(f'執行錯誤: {{e}}')\n    ui.show_message(f'發生錯誤: {{e}}', '錯誤')";
            }
        }
    }
} 