using System;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace WpfIronPythonApp.Services
{
    /// <summary>
    /// 程式碼建議管理器
    /// 負責在編輯器中顯示和管理AI建議
    /// </summary>
    public class CodeSuggestionManager
    {
        private readonly TextEditor _textEditor;
        private string? _currentSuggestion;
        private int _suggestionOffset;
        private bool _isShowingSuggestion = false;

        public CodeSuggestionManager(TextEditor textEditor)
        {
            _textEditor = textEditor ?? throw new ArgumentNullException(nameof(textEditor));
            _textEditor.KeyDown += TextEditor_KeyDown;
        }

        /// <summary>
        /// 顯示AI建議
        /// </summary>
        /// <param name="suggestion">建議的程式碼</param>
        /// <param name="insertPosition">插入位置</param>
        public void ShowSuggestion(string suggestion, int insertPosition)
        {
            if (string.IsNullOrWhiteSpace(suggestion))
                return;

            ClearCurrentSuggestion();

            _currentSuggestion = suggestion;
            _suggestionOffset = insertPosition;
            _isShowingSuggestion = true;

            // 在編輯器中顯示建議文字（作為選取範圍）
            DisplaySuggestionInEditor();

            LoggingService.Instance.LogInfo($"顯示AI建議: {suggestion.Substring(0, Math.Min(50, suggestion.Length))}...");
        }

        /// <summary>
        /// 在編輯器中顯示建議
        /// </summary>
        private void DisplaySuggestionInEditor()
        {
            if (string.IsNullOrEmpty(_currentSuggestion))
                return;

            try
            {
                // 暫時插入建議文字
                var originalCaretOffset = _textEditor.CaretOffset;
                _textEditor.Document.Insert(_suggestionOffset, _currentSuggestion);
                
                // 選取插入的文字以顯示為建議
                _textEditor.Select(_suggestionOffset, _currentSuggestion.Length);
                
                // 設定選取文字的背景顏色為灰色
                _textEditor.TextArea.TextView.Redraw();
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"顯示建議失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 應用當前建議
        /// </summary>
        public void ApplySuggestion()
        {
            if (string.IsNullOrEmpty(_currentSuggestion) || !_isShowingSuggestion)
                return;

            try
            {
                // 建議已經在編輯器中，只需要取消選取
                _textEditor.Select(0, 0);
                _textEditor.CaretOffset = _suggestionOffset + _currentSuggestion.Length;

                LoggingService.Instance.LogInfo("AI建議已應用");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"應用AI建議失敗: {ex.Message}");
            }
            finally
            {
                _currentSuggestion = null;
                _suggestionOffset = 0;
                _isShowingSuggestion = false;
            }
        }

        /// <summary>
        /// 清除當前建議
        /// </summary>
        public void ClearCurrentSuggestion()
        {
            if (!_isShowingSuggestion || string.IsNullOrEmpty(_currentSuggestion))
                return;

            try
            {
                // 如果建議文字在編輯器中被選取，刪除它
                if (_textEditor.SelectionLength > 0 && 
                    _textEditor.SelectionStart == _suggestionOffset && 
                    _textEditor.SelectionLength == _currentSuggestion.Length)
                {
                    _textEditor.Document.Remove(_suggestionOffset, _currentSuggestion.Length);
                }
                
                _textEditor.Select(0, 0);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.LogError($"清除建議失敗: {ex.Message}");
            }
            finally
            {
                _currentSuggestion = null;
                _suggestionOffset = 0;
                _isShowingSuggestion = false;
            }
        }

        /// <summary>
        /// 檢查是否有活動的建議
        /// </summary>
        public bool HasActiveSuggestion => _isShowingSuggestion && !string.IsNullOrEmpty(_currentSuggestion);

        /// <summary>
        /// 處理鍵盤事件
        /// </summary>
        private void TextEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && HasActiveSuggestion)
            {
                e.Handled = true;
                ApplySuggestion();
            }
            else if (e.Key == Key.Escape && HasActiveSuggestion)
            {
                e.Handled = true;
                ClearCurrentSuggestion();
            }
            else if (HasActiveSuggestion && 
                     (e.Key == Key.Back || e.Key == Key.Delete || 
                      e.Key == Key.Enter || e.Key == Key.Space ||
                      (e.Key >= Key.A && e.Key <= Key.Z) ||
                      (e.Key >= Key.D0 && e.Key <= Key.D9)))
            {
                // 用戶開始編輯，清除建議
                ClearCurrentSuggestion();
            }
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            _textEditor.KeyDown -= TextEditor_KeyDown;
            ClearCurrentSuggestion();
        }
    }
} 