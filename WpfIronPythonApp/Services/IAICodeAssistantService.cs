using System.Threading.Tasks;

namespace WpfIronPythonApp.Services
{
    /// <summary>
    /// AI 程式碼助手服務介面
    /// 提供程式碼建議和優化功能
    /// </summary>
    public interface IAICodeAssistantService
    {
        /// <summary>
        /// 根據選取的程式碼獲取AI建議
        /// </summary>
        /// <param name="selectedCode">選取的程式碼</param>
        /// <param name="context">程式碼上下文</param>
        /// <returns>AI建議的程式碼</returns>
        Task<string> GetCodeSuggestionAsync(string selectedCode, string context = "");

        /// <summary>
        /// 優化現有程式碼
        /// </summary>
        /// <param name="code">要優化的程式碼</param>
        /// <returns>優化後的程式碼</returns>
        Task<string> OptimizeCodeAsync(string code);

        /// <summary>
        /// 解釋程式碼功能
        /// </summary>
        /// <param name="code">要解釋的程式碼</param>
        /// <returns>程式碼說明</returns>
        Task<string> ExplainCodeAsync(string code);

        /// <summary>
        /// 修復程式碼錯誤
        /// </summary>
        /// <param name="code">有錯誤的程式碼</param>
        /// <param name="errorMessage">錯誤訊息</param>
        /// <returns>修復後的程式碼</returns>
        Task<string> FixCodeAsync(string code, string errorMessage = "");
    }
} 