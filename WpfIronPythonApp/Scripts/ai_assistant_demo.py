# AI 助手功能示範腳本
# 這個腳本展示如何使用右鍵選單的AI助手功能

# 1. 選取下面的 print 語句，右鍵選擇 "AI 助手" > "獲取程式碼建議"
print("Hello World")

# 2. 選取下面的迴圈，右鍵選擇 "AI 助手" > "優化程式碼"
for i in range(10):
    print(i)

# 3. 選取下面的函數，右鍵選擇 "AI 助手" > "解釋程式碼"
def calculate_sum(a, b):
    return a + b

# 4. 選取下面有錯誤的程式碼，右鍵選擇 "AI 助手" > "修復程式碼"
# 故意的語法錯誤示例：
# print("Missing closing quote

# 5. 測試步驟：
# - 選取任何程式碼片段
# - 右鍵點擊
# - 選擇 "AI 助手" 下的任一選項
# - 等待AI處理（會顯示灰色建議文字）
# - 按 Tab 鍵應用建議，或按 Esc 鍵取消

# 使用可用的API
host.log('AI助手示範開始')
ui.show_message('歡迎使用AI助手功能！', '提示')

# 嘗試選取這行並請求AI建議
result = data.load_csv('sample.csv') 