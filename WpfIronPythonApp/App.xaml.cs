using System;
using System.Windows;

namespace WpfIronPythonApp
{
    /// <summary>
    /// WPF IronPython 應用程式主類別
    /// 符合需求規格書第 I 節 - 緒論中定義的應用程式範圍
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                
                // 設定全域異常處理
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                
                // 初始化應用程式日誌
                Services.LoggingService.Instance.LogInfo("應用程式啟動");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"應用程式啟動失敗: {ex.Message}", "錯誤", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown(1);
            }
        }

        private void App_DispatcherUnhandledException(object sender, 
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Services.LoggingService.Instance.LogError($"UI 執行緒異常: {e.Exception}");
            
            MessageBox.Show($"發生未處理的錯誤:\n{e.Exception.Message}", "錯誤", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Services.LoggingService.Instance.LogError($"應用程式域異常: {ex}");
            
            MessageBox.Show($"發生嚴重錯誤:\n{ex?.Message}", "嚴重錯誤", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Services.LoggingService.Instance.LogInfo("應用程式關閉");
            base.OnExit(e);
        }
    }
} 