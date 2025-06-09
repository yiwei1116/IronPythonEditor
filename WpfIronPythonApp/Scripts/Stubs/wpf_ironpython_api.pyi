# WPF IronPython 應用程式 API 存根檔案
# 符合需求規格書第 IV.C 節關於 IntelliSense 實作策略
# 此檔案定義了需求規格書表1中所有公開的 C# API

"""
WPF IronPython 應用程式 API 類型定義
提供編輯器 IntelliSense 支援
"""

from typing import Optional, Any
from System.Data import DataTable

class IScriptHost:
    """
    主機物件介面
    符合需求規格書表1中的 ApplicationHost API
    """
    
    def log(self, message: str) -> None:
        """
        記錄訊息到應用程式日誌
        
        Args:
            message: 日誌訊息
        """
        ...
    
    @property
    def active_doc(self) -> Optional['DocumentWrapper']:
        """
        取得當前活動文件物件
        
        Returns:
            當前活動的文件物件，如果沒有則返回 None
        """
        ...

class IUIController:
    """
    UI 控制器介面
    符合需求規格書表1中的 UIController API
    """
    
    def show_message(self, message: str, title: str = "訊息") -> None:
        """
        顯示訊息框
        
        Args:
            message: 訊息內容
            title: 訊息標題，預設為 "訊息"
        """
        ...
    
    def status_bar(self, text: str) -> None:
        """
        更新狀態列文字
        
        Args:
            text: 狀態列文字
        """
        ...

class IDataService:
    """
    資料服務介面
    符合需求規格書表1中的 DataService API
    """
    
    def load_csv(self, file_path: str) -> DataTable:
        """
        從 CSV 載入數據
        
        Args:
            file_path: CSV 檔案路徑
            
        Returns:
            載入的資料表
            
        Raises:
            FileNotFoundError: 當檔案不存在時
            Exception: 當載入失敗時
        """
        ...
    
    def process_table(self, table: DataTable) -> DataTable:
        """
        處理數據表
        
        Args:
            table: 要處理的資料表
            
        Returns:
            處理後的資料表
            
        Raises:
            ArgumentNullException: 當 table 為 None 時
        """
        ...

class DocumentWrapper:
    """
    文件包裝器
    符合需求規格書表1中的 Document API
    """
    
    def save(self, path: Optional[str] = None) -> bool:
        """
        儲存文件
        
        Args:
            path: 儲存路徑，如果為 None 則使用現有路徑
            
        Returns:
            是否儲存成功
        """
        ...
    
    @property
    def page_count(self) -> int:
        """
        取得文件頁數
        
        Returns:
            文件的頁數
        """
        ...
    
    @property
    def content(self) -> str:
        """
        文件內容
        """
        ...
    
    @content.setter
    def content(self, value: str) -> None:
        """
        設定文件內容
        """
        ...
    
    @property
    def file_path(self) -> str:
        """
        檔案路徑
        """
        ...
    
    @property
    def is_dirty(self) -> bool:
        """
        是否已修改但未儲存
        """
        ...

# 全域物件實例
# 這些物件在腳本執行時由 C# 主機提供

host: IScriptHost
"""
主機物件，提供應用程式核心功能存取
使用方式：
- host.log('訊息')
- doc = host.active_doc
"""

ui: IUIController
"""
UI 控制器，提供使用者介面操作
使用方式：
- ui.show_message('Hello World!')
- ui.status_bar('狀態更新')
"""

data: IDataService
"""
資料服務，提供資料處理功能
使用方式：
- table = data.load_csv('data.csv')
- processed = data.process_table(table)
"""

doc: Optional[DocumentWrapper]
"""
當前活動文件（當有活動文件時可用）
使用方式：
- if doc:
-     doc.save()
-     print(f'頁數: {doc.page_count}')
"""

# 範例使用案例

def example_basic_usage():
    """
    基本使用範例
    """
    # 記錄日誌
    host.log('腳本開始執行')
    
    # 顯示訊息
    ui.show_message('Hello from IronPython!', '歡迎')
    
    # 更新狀態列
    ui.status_bar('腳本執行中...')
    
    # 檢查活動文件
    if host.active_doc:
        doc = host.active_doc
        host.log(f'當前文件頁數: {doc.page_count}')
        
        # 儲存文件
        if doc.save():
            host.log('文件儲存成功')
    
    print('範例執行完成')

def example_data_processing():
    """
    資料處理範例
    """
    try:
        # 載入 CSV 資料
        table = data.load_csv('sample.csv')
        host.log(f'載入了 {table.Rows.Count} 行資料')
        
        # 處理資料
        processed_table = data.process_table(table)
        host.log(f'處理後有 {processed_table.Rows.Count} 行資料')
        
        # 顯示結果
        ui.show_message(f'資料處理完成\n原始: {table.Rows.Count} 行\n處理後: {processed_table.Rows.Count} 行')
        
    except Exception as e:
        host.log(f'資料處理失敗: {str(e)}')
        ui.show_message(f'錯誤: {str(e)}', '資料處理失敗')

def example_document_manipulation():
    """
    文件操作範例
    """
    current_doc = host.active_doc
    if current_doc:
        # 讀取當前內容
        original_content = current_doc.content
        host.log(f'原始內容長度: {len(original_content)}')
        
        # 修改內容
        current_doc.content += '\n# 由 IronPython 腳本添加'
        
        # 檢查是否需要儲存
        if current_doc.is_dirty:
            host.log('文件已修改，需要儲存')
            if current_doc.save():
                ui.status_bar('文件已儲存')
            else:
                ui.status_bar('儲存失敗')
    else:
        ui.show_message('沒有活動文件可操作', '提示') 