using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace WpfIronPythonApp.IntelliSense
{
    /// <summary>
    /// 代碼補全數據類別
    /// 符合需求規格書第 IV.C 節關於 IntelliSense 實作的要求
    /// </summary>
    public class ApiCompletionData : ICompletionData
    {
        public ApiCompletionData(string text, string description, string category = "API", bool isMethod = false)
        {
            Text = text;
            Description = description;
            Category = category;
            IsMethod = isMethod;
            
            // 設定顯示文字
            if (isMethod && !text.EndsWith("()"))
            {
                Content = text + "()";
            }
            else
            {
                Content = text;
            }
        }

        /// <summary>
        /// 顯示在補全列表中的圖示
        /// </summary>
        public ImageSource? Image => null; // 可以後續添加圖標

        /// <summary>
        /// 顯示在補全列表中的文字
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// 顯示在補全列表中的內容
        /// </summary>
        public object Content { get; private set; }

        /// <summary>
        /// 詳細描述，顯示在工具提示中
        /// </summary>
        public object Description { get; private set; }

        /// <summary>
        /// API 分類
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// 是否為方法
        /// </summary>
        public bool IsMethod { get; private set; }

        /// <summary>
        /// 優先級，數字越大優先級越高
        /// </summary>
        public double Priority 
        { 
            get 
            {
                // API 優先級高於其他項目
                if (Category == "API") return 1.0;
                if (Category == "Method") return 0.9;
                if (Category == "Property") return 0.8;
                return 0.5;
            } 
        }

        /// <summary>
        /// 完成操作，插入選中的補全項目
        /// </summary>
        /// <param name="textArea">文字區域</param>
        /// <param name="completionSegment">補全段落</param>
        /// <param name="insertionRequestEventArgs">插入請求參數</param>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // 獲取要插入的文字
            string insertText = Text;
            
            // 如果是方法且不包含括號，添加括號
            if (IsMethod && !Text.EndsWith("()"))
            {
                insertText += "()";
                
                // 將游標移動到括號內
                textArea.Document.Replace(completionSegment, insertText);
                if (insertText.EndsWith("()"))
                {
                    textArea.Caret.Offset -= 1; // 移動到括號內
                }
            }
            else
            {
                textArea.Document.Replace(completionSegment, insertText);
            }
        }
    }

    /// <summary>
    /// Python 關鍵字補全數據
    /// </summary>
    public class PythonKeywordCompletionData : ICompletionData
    {
        public PythonKeywordCompletionData(string keyword, string description = "")
        {
            Text = keyword;
            Content = keyword;
            Description = string.IsNullOrEmpty(description) ? $"Python 關鍵字: {keyword}" : description;
        }

        public ImageSource? Image => null;
        public string Text { get; private set; }
        public object Content { get; private set; }
        public object Description { get; private set; }
        public double Priority => 0.3; // 關鍵字優先級較低

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
} 