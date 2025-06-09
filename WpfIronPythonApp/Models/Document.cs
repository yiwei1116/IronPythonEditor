using System;
using System.ComponentModel;
using System.IO;

namespace WpfIronPythonApp.Models
{
    /// <summary>
    /// 文件模型類別
    /// 符合需求規格書表1中的 Document API 定義
    /// </summary>
    public class Document : INotifyPropertyChanged
    {
        private string _filePath = string.Empty;
        private string _content = string.Empty;
        private bool _isDirty;
        private int _pageCount = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 文件檔案路徑
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(FilePath));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary>
        /// 文件內容
        /// </summary>
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    IsDirty = true;
                    OnPropertyChanged(nameof(Content));
                    UpdatePageCount();
                }
            }
        }

        /// <summary>
        /// 是否已修改但未儲存
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(nameof(IsDirty));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary>
        /// 頁數（符合需求規格書中的 page_count API）
        /// </summary>
        public int PageCount
        {
            get => _pageCount;
            private set
            {
                if (_pageCount != value)
                {
                    _pageCount = value;
                    OnPropertyChanged(nameof(PageCount));
                }
            }
        }

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName
        {
            get
            {
                var name = string.IsNullOrEmpty(FilePath) ? "未命名文件" : Path.GetFileName(FilePath);
                return IsDirty ? $"{name}*" : name;
            }
        }

        /// <summary>
        /// 儲存文件
        /// 符合需求規格書表1中的 doc.save() API
        /// </summary>
        /// <param name="path">儲存路徑，如果為空則使用現有路徑</param>
        /// <returns>是否儲存成功</returns>
        public bool Save(string? path = null)
        {
            try
            {
                var savePath = path ?? FilePath;
                if (string.IsNullOrEmpty(savePath))
                {
                    throw new InvalidOperationException("未指定儲存路徑");
                }

                File.WriteAllText(savePath, Content);
                FilePath = savePath;
                IsDirty = false;

                Services.LoggingService.Instance.LogInfo($"文件已儲存: {savePath}");
                return true;
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.LogError($"儲存文件失敗: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 載入文件
        /// </summary>
        /// <param name="path">檔案路徑</param>
        /// <returns>是否載入成功</returns>
        public bool Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"檔案不存在: {path}");
                }

                Content = File.ReadAllText(path);
                FilePath = path;
                IsDirty = false;

                Services.LoggingService.Instance.LogInfo($"文件已載入: {path}");
                return true;
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.LogError($"載入文件失敗: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新頁數計算
        /// </summary>
        private void UpdatePageCount()
        {
            // 簡單的頁數計算：假設每500字符為一頁
            const int charactersPerPage = 500;
            PageCount = Math.Max(1, (Content.Length + charactersPerPage - 1) / charactersPerPage);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 