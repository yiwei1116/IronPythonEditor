# WPF IronPython 巨集範例
# 此腳本展示如何使用需求規格書表1中定義的 C# API

"""
範例巨集：展示 WPF IronPython 應用程式的核心功能
符合需求規格書第 III.A.1 節中定義的功能要求
"""

import sys
import os
from datetime import datetime

def main():
    """主函數"""
    try:
        # 1. 記錄日誌 (符合需求規格書表1中的 host.log API)
        host.log('範例巨集開始執行')
        
        # 2. 顯示歡迎訊息 (符合需求規格書表1中的 ui.show_message API)
        ui.show_message('歡迎使用 WPF IronPython 巨集系統!', '歡迎')
        
        # 3. 更新狀態列 (符合需求規格書表1中的 ui.status_bar API)
        ui.status_bar('正在執行範例巨集...')
        
        # 4. 檢查活動文件 (符合需求規格書表1中的 host.active_doc API)
        active_doc = host.active_doc
        if active_doc:
            host.log(f'發現活動文件：{active_doc.file_path}')
            host.log(f'文件頁數：{active_doc.page_count}')
            
            # 修改文件內容示例
            if hasattr(active_doc, 'content'):
                original_content = active_doc.content
                active_doc.content += f'\n# 巨集於 {datetime.now()} 添加此註解'
                host.log('已修改文件內容')
        else:
            host.log('目前沒有活動文件')
        
        # 5. 資料處理示例 (符合需求規格書表1中的 data API)
        demonstrate_data_processing()
        
        # 6. 系統資訊收集
        collect_system_info()
        
        # 7. 完成
        ui.status_bar('範例巨集執行完成')
        host.log('範例巨集執行完成')
        
        print('✅ 所有操作完成成功!')
        
    except Exception as e:
        error_msg = f'執行範例巨集時發生錯誤: {str(e)}'
        host.log(error_msg)
        ui.show_message(error_msg, '錯誤')
        ui.status_bar('巨集執行失敗')
        raise

def demonstrate_data_processing():
    """展示資料處理功能"""
    try:
        host.log('開始資料處理示例')
        
        # 創建範例 CSV 檔案
        sample_csv_path = create_sample_csv()
        
        if sample_csv_path and os.path.exists(sample_csv_path):
            # 載入 CSV 資料 (符合需求規格書表1中的 data.load_csv API)
            data_table = data.load_csv(sample_csv_path)
            host.log(f'成功載入 CSV 資料，共 {data_table.Rows.Count} 行')
            
            # 處理資料表 (符合需求規格書表1中的 data.process_table API)
            processed_table = data.process_table(data_table)
            host.log(f'資料處理完成，處理後共 {processed_table.Rows.Count} 行')
            
            # 顯示資料摘要
            display_data_summary(processed_table)
        else:
            host.log('無法建立範例 CSV 檔案')
            
    except Exception as e:
        host.log(f'資料處理示例失敗: {str(e)}')

def create_sample_csv():
    """建立範例 CSV 檔案"""
    try:
        # 使用應用程式資料目錄
        import tempfile
        temp_dir = tempfile.gettempdir()
        csv_path = os.path.join(temp_dir, 'sample_data.csv')
        
        # 建立範例資料
        sample_data = """Name,Age,Department,Salary
張三,30,技術部,50000
李四,25,行銷部,45000
王五,35,人事部,55000
趙六,28,技術部,48000
錢七,32,行銷部,52000
"""
        
        with open(csv_path, 'w', encoding='utf-8') as f:
            f.write(sample_data)
        
        host.log(f'已建立範例 CSV 檔案: {csv_path}')
        return csv_path
        
    except Exception as e:
        host.log(f'建立範例 CSV 檔案失敗: {str(e)}')
        return None

def display_data_summary(data_table):
    """顯示資料摘要"""
    try:
        if data_table and data_table.Rows.Count > 0:
            summary = f"資料表摘要：\n"
            summary += f"- 總行數：{data_table.Rows.Count}\n"
            summary += f"- 總列數：{data_table.Columns.Count}\n"
            
            # 列出欄位名稱
            column_names = [col.ColumnName for col in data_table.Columns]
            summary += f"- 欄位：{', '.join(column_names)}\n"
            
            ui.show_message(summary, "資料摘要")
            host.log("已顯示資料摘要")
        else:
            host.log("資料表為空或無效")
            
    except Exception as e:
        host.log(f'顯示資料摘要失敗: {str(e)}')

def collect_system_info():
    """收集系統資訊"""
    try:
        host.log('收集系統資訊')
        
        # Python 版本資訊
        python_version = f"{sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro}"
        host.log(f'Python 版本: {python_version}')
        
        # 系統路徑資訊
        host.log(f'當前工作目錄: {os.getcwd()}')
        host.log(f'Python 路徑數量: {len(sys.path)}')
        
        # 顯示可用的內建模組
        builtin_modules = list(sys.builtin_module_names)[:10]  # 只顯示前10個
        host.log(f'部分內建模組: {", ".join(builtin_modules)}')
        
        print(f'🐍 Python {python_version} 執行環境正常')
        print(f'📁 工作目錄: {os.getcwd()}')
        print(f'📦 可用內建模組: {len(sys.builtin_module_names)} 個')
        
    except Exception as e:
        host.log(f'收集系統資訊失敗: {str(e)}')

# 如果直接執行此腳本，則呼叫主函數
if __name__ == '__main__':
    main() 