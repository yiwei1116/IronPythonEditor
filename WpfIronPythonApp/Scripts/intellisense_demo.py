# -*- coding: utf-8 -*-
"""
IntelliSense 功能演示腳本
展示如何使用 WPF IronPython 應用程式的代碼補全功能

使用說明:
1. 輸入 'host.' 會顯示主機物件的可用方法和屬性
2. 輸入 'ui.' 會顯示 UI 控制器的可用方法
3. 輸入 'data.' 會顯示資料服務的可用方法
4. 輸入 'doc.' (當有活動文件時) 會顯示文件物件的方法和屬性
5. 按 Ctrl+Space 手動觸發代碼補全
6. 使用 Python 關鍵字也會有自動補全

作者: WPF IronPython 應用程式
版本: 1.0
"""

# 導入必要的模組 (在這裡輸入 'im' 會自動建議 'import')


def main():
    """主函數 - 演示所有可用的 API"""
    
    # === 日誌記錄功能 ===
    # 輸入 'host.' 會顯示可用的方法
    
    
    # === UI 控制功能 ===
    # 輸入 'ui.' 會顯示可用的方法
    
    
    # === 資料處理功能 ===
    # 輸入 'data.' 會顯示可用的方法
    
    
    # === 文件操作功能 ===
    # 輸入 'doc = host.' 然後選擇 active_doc
    doc = None  # 這裡會在實際使用時設為 host.active_doc
    
    # 如果有活動文件，輸入 'doc.' 會顯示文件的方法和屬性
    
    
    # === Python 內建功能演示 ===
    # 輸入以下關鍵字的開頭會有自動補全:
    # def, class, if, for, while, try, except, print, len, str, int, etc.
    
    
    # === 流程控制結構 ===
    # 輸入 'if' 會自動補全條件判斷語法
    
    
    # 輸入 'for' 會自動補全迴圈語法
    

def error_handling_example():
    """錯誤處理示例 - 輸入 'try' 會自動補全異常處理結構"""
    

def data_processing_example():
    """資料處理示例"""
    # 在這個函數中嘗試以下操作:
    # 1. 輸入 'data.load_csv' 查看參數說明
    # 2. 輸入 'data.process_table' 查看方法說明
    

def ui_interaction_example():
    """UI 互動示例"""
    # 在這個函數中嘗試以下操作:
    # 1. 輸入 'ui.show_message' 查看參數說明
    # 2. 輸入 'ui.status_bar' 查看使用方法
    

def document_operations_example():
    """文件操作示例"""
    # 在這個函數中嘗試以下操作:
    # 1. 獲取活動文件: doc = host.active_doc
    # 2. 檢查文件屬性: doc.page_count, doc.is_dirty, doc.file_path
    # 3. 文件操作: doc.save(), doc.content
    

# === 常用變數類型示例 ===
sample_string = ""      # 字串類型
sample_number = 0       # 數字類型
sample_list = []        # 列表類型
sample_dict = {}        # 字典類型
sample_boolean = True   # 布林類型


if __name__ == "__main__":
    """
    程式入口點
    執行此腳本來測試 IntelliSense 功能
    """
    main()

# === 使用提示 ===
"""
IntelliSense 使用技巧:

1. 自動觸發: 輸入 '.' 後會自動顯示可用的方法和屬性
2. 手動觸發: 按 Ctrl+Space 手動顯示補全列表
3. 方法補全: 選擇方法後會自動添加括號，游標移到括號內
4. 詳細說明: 每個 API 都有詳細的參數說明和使用範例
5. 分類顯示: API 按重要程度排序，優先顯示最相關的建議

支援的 API 物件:
- host: 主機物件，提供 log() 和 active_doc
- ui: UI 控制器，提供 show_message() 和 status_bar()
- data: 資料服務，提供 load_csv() 和 process_table()  
- doc: 活動文件物件 (當有文件開啟時)

支援的 Python 關鍵字:
def, class, if, elif, else, for, while, try, except, finally,
import, from, return, print, len, str, int, float, bool,
list, dict, True, False, None, and, or, not, in, is
""" 