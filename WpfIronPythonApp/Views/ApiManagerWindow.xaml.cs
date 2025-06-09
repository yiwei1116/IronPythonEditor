using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WpfIronPythonApp.Services.ApiRegistry;
using WpfIronPythonApp.Services;

namespace WpfIronPythonApp.Views
{
    /// <summary>
    /// API管理器視窗
    /// </summary>
    public partial class ApiManagerWindow : Window
    {
        private readonly IApiRegistry _apiRegistry;
        private List<ApiServiceDescriptor> _allServices = new();
        private List<ApiServiceDescriptor> _filteredServices = new();
        private ApiServiceDescriptor? _selectedService;

        public ApiManagerWindow(IApiRegistry apiRegistry)
        {
            InitializeComponent();
            _apiRegistry = apiRegistry ?? throw new ArgumentNullException(nameof(apiRegistry));
            
            // 訂閱API註冊事件
            _apiRegistry.ServiceRegistered += OnServiceRegistered;
            _apiRegistry.ServiceUnregistered += OnServiceUnregistered;
            _apiRegistry.ServiceStateChanged += OnServiceStateChanged;
            
            LoadServices();
        }

        /// <summary>
        /// 載入所有服務
        /// </summary>
        private void LoadServices()
        {
            try
            {
                _allServices = _apiRegistry.GetAllServices().ToList();
                _filteredServices = new List<ApiServiceDescriptor>(_allServices);
                
                UpdateServicesDisplay();
                UpdateServiceCount();
                UpdateStatus("已載入服務清單");
            }
            catch (Exception ex)
            {
                UpdateStatus($"載入服務失敗: {ex.Message}");
                LoggingService.Instance.LogError($"載入API服務失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新服務顯示
        /// </summary>
        private void UpdateServicesDisplay()
        {
            ServicesListBox.ItemsSource = null;
            ServicesListBox.ItemsSource = _filteredServices;
        }

        /// <summary>
        /// 更新服務數量顯示
        /// </summary>
        private void UpdateServiceCount()
        {
            ServiceCountText.Text = $"服務數量: {_filteredServices.Count}";
        }

        /// <summary>
        /// 更新狀態列
        /// </summary>
        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }

        /// <summary>
        /// 重新整理按鈕點擊事件
        /// </summary>
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadServices();
        }

        /// <summary>
        /// 生成文件按鈕點擊事件
        /// </summary>
        private void GenerateDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var format = GetSelectedDocumentFormat();
                var documentation = _apiRegistry.GenerateDocumentation(format);
                DocumentationTextBox.Text = documentation;
                
                // 切換到API文件標籤
                DetailsTabControl.SelectedIndex = 3;
                
                UpdateStatus("已生成API文件");
            }
            catch (Exception ex)
            {
                UpdateStatus($"生成文件失敗: {ex.Message}");
                MessageBox.Show($"生成API文件失敗:\n{ex.Message}", "錯誤", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 匯出按鈕點擊事件
        /// </summary>
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "匯出API文件",
                    Filter = "Markdown 檔案 (*.md)|*.md|JSON 檔案 (*.json)|*.json|HTML 檔案 (*.html)|*.html|文字檔案 (*.txt)|*.txt",
                    DefaultExt = "md"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var format = GetDocumentFormatFromExtension(Path.GetExtension(saveDialog.FileName));
                    var documentation = _apiRegistry.GenerateDocumentation(format);
                    
                    File.WriteAllText(saveDialog.FileName, documentation);
                    UpdateStatus($"已匯出API文件到: {saveDialog.FileName}");
                    
                    MessageBox.Show($"API文件已成功匯出到:\n{saveDialog.FileName}", "匯出成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"匯出失敗: {ex.Message}");
                MessageBox.Show($"匯出API文件失敗:\n{ex.Message}", "錯誤", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 搜尋框文字變更事件
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Foreground == System.Windows.Media.Brushes.Gray) return;
            
            FilterServices();
        }

        /// <summary>
        /// 搜尋框獲得焦點事件
        /// </summary>
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "搜尋服務...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        /// <summary>
        /// 搜尋框失去焦點事件
        /// </summary>
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "搜尋服務...";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        /// <summary>
        /// 篩選服務
        /// </summary>
        private void FilterServices()
        {
            var searchText = SearchBox.Text?.ToLower() ?? "";
            
            if (string.IsNullOrWhiteSpace(searchText) || searchText == "搜尋服務...")
            {
                _filteredServices = new List<ApiServiceDescriptor>(_allServices);
            }
            else
            {
                _filteredServices = _allServices.Where(s => 
                    s.ServiceName.ToLower().Contains(searchText) ||
                    s.Description.ToLower().Contains(searchText) ||
                    s.ServiceType.Name.ToLower().Contains(searchText)
                ).ToList();
            }
            
            UpdateServicesDisplay();
            UpdateServiceCount();
        }

        /// <summary>
        /// 服務選擇變更事件
        /// </summary>
        private void ServicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedService = ServicesListBox.SelectedItem as ApiServiceDescriptor;
            UpdateServiceDetails();
        }

        /// <summary>
        /// 服務啟用狀態變更事件
        /// </summary>
        private void ServiceEnabled_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is ApiServiceDescriptor service)
            {
                try
                {
                    _apiRegistry.SetServiceEnabled(service.ServiceName, checkBox.IsChecked == true);
                    UpdateStatus($"已{(checkBox.IsChecked == true ? "啟用" : "停用")}服務: {service.ServiceName}");
                }
                catch (Exception ex)
                {
                    UpdateStatus($"變更服務狀態失敗: {ex.Message}");
                    // 恢復原狀態
                    checkBox.IsChecked = !checkBox.IsChecked;
                }
            }
        }

        /// <summary>
        /// 更新服務詳細資訊
        /// </summary>
        private void UpdateServiceDetails()
        {
            if (_selectedService == null)
            {
                ClearServiceDetails();
                return;
            }

            // 更新服務詳情
            ServiceNameText.Text = _selectedService.ServiceName;
            ServiceVersionText.Text = _selectedService.Version;
            ServiceDescriptionText.Text = _selectedService.Description;
            ServiceTypeText.Text = _selectedService.ServiceType.FullName;
            ServiceStatusText.Text = _selectedService.IsEnabled ? "已啟用" : "已停用";
            ServiceStatusText.Foreground = _selectedService.IsEnabled ? 
                System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            ServiceRegisteredText.Text = _selectedService.RegisteredAt.ToString("yyyy-MM-dd HH:mm:ss");

            // 更新方法列表
            MethodsDataGrid.ItemsSource = _selectedService.Methods;

            // 更新屬性列表
            PropertiesDataGrid.ItemsSource = _selectedService.Properties;

            // 生成單一服務的文件
            GenerateServiceDocumentation();
        }

        /// <summary>
        /// 清除服務詳細資訊
        /// </summary>
        private void ClearServiceDetails()
        {
            ServiceNameText.Text = "";
            ServiceVersionText.Text = "";
            ServiceDescriptionText.Text = "";
            ServiceTypeText.Text = "";
            ServiceStatusText.Text = "";
            ServiceRegisteredText.Text = "";
            MethodsDataGrid.ItemsSource = null;
            PropertiesDataGrid.ItemsSource = null;
            DocumentationTextBox.Text = "";
        }

        /// <summary>
        /// 生成單一服務的文件
        /// </summary>
        private void GenerateServiceDocumentation()
        {
            if (_selectedService == null) return;

            try
            {
                var format = GetSelectedDocumentFormat();
                
                // 這裡可以實現單一服務的文件生成
                // 暫時使用完整文件
                var documentation = _apiRegistry.GenerateDocumentation(format);
                DocumentationTextBox.Text = documentation;
            }
            catch (Exception ex)
            {
                DocumentationTextBox.Text = $"生成文件失敗: {ex.Message}";
            }
        }

        /// <summary>
        /// 文件格式變更事件
        /// </summary>
        private void DocumentFormat_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedService != null)
            {
                GenerateServiceDocumentation();
            }
        }

        /// <summary>
        /// 複製文件按鈕點擊事件
        /// </summary>
        private void CopyDocs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(DocumentationTextBox.Text))
                {
                    Clipboard.SetText(DocumentationTextBox.Text);
                    UpdateStatus("已複製API文件到剪貼簿");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"複製失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 獲取選中的文件格式
        /// </summary>
        private DocumentationFormat GetSelectedDocumentFormat()
        {
            return DocumentFormatComboBox.SelectedIndex switch
            {
                0 => DocumentationFormat.Markdown,
                1 => DocumentationFormat.Json,
                2 => DocumentationFormat.Html,
                3 => DocumentationFormat.PlainText,
                _ => DocumentationFormat.Markdown
            };
        }

        /// <summary>
        /// 根據檔案副檔名獲取文件格式
        /// </summary>
        private DocumentationFormat GetDocumentFormatFromExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".md" => DocumentationFormat.Markdown,
                ".json" => DocumentationFormat.Json,
                ".html" => DocumentationFormat.Html,
                ".txt" => DocumentationFormat.PlainText,
                _ => DocumentationFormat.Markdown
            };
        }

        #region 事件處理

        /// <summary>
        /// 服務註冊事件處理
        /// </summary>
        private void OnServiceRegistered(object? sender, ServiceRegisteredEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LoadServices();
                UpdateStatus($"新服務已註冊: {e.ServiceName}");
            });
        }

        /// <summary>
        /// 服務卸載事件處理
        /// </summary>
        private void OnServiceUnregistered(object? sender, ServiceUnregisteredEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LoadServices();
                UpdateStatus($"服務已卸載: {e.ServiceName}");
            });
        }

        /// <summary>
        /// 服務狀態變更事件處理
        /// </summary>
        private void OnServiceStateChanged(object? sender, ServiceStateChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // 更新顯示
                UpdateServicesDisplay();
                if (_selectedService?.ServiceName == e.ServiceName)
                {
                    UpdateServiceDetails();
                }
            });
        }

        #endregion

        /// <summary>
        /// 視窗關閉時清理資源
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // 取消訂閱事件
            _apiRegistry.ServiceRegistered -= OnServiceRegistered;
            _apiRegistry.ServiceUnregistered -= OnServiceUnregistered;
            _apiRegistry.ServiceStateChanged -= OnServiceStateChanged;
            
            base.OnClosed(e);
        }
    }
} 