WPF IronPython 應用程式軟體需求規格書I. 緒論本節將確立軟體需求規格書 (SRS) 的基礎背景，概述文件的目的、應用程式的範圍及其目標受眾。這將為後續的技術細節設定正式基調。A. 文件目的本文件旨在正式定義一個整合 IronPython 作為巨集腳本引擎的 WPF 應用程式的功能與非功能性需求。它將作為開發、測試和專案管理團隊的全面指南，確保對系統功能和限制有共同的理解。此外，本文件將詳細說明如何從 IronPython 無縫呼叫 C# API，以及如何在嵌入式編輯器中提供智慧程式碼補全 (IntelliSense) 功能。B. WPF IronPython 應用程式範圍此應用程式將提供一個核心的 WPF 使用者介面和業務邏輯。它將包含一個整合環境，供使用者編寫和執行 IronPython 腳本（巨集），以擴展或自動化應用程式功能。其中一個主要功能是，在嵌入式腳本編輯器中提供 IntelliSense，為標準 IronPython 結構和應用程式公開的 C# API 提供自動補全功能。C. 應用程式目標受眾（編寫巨集的使用者）應用程式的目標受眾主要分為兩類：
高階使用者/領域專家： 這些使用者具備領域知識，需要超越標準 UI 操作的自動化或自訂功能，並具備基礎到中級的 Python 腳本編寫技能。
開發人員/整合者： 這些使用者可能需要擴展應用程式的功能，直接存取 C# API 以進行複雜的整合或進階巨集開發。他們將從全面的 IntelliSense 功能中獲益最多。
II. 總體描述本節提供產品的高層次概述，包括其功能、使用者的特徵以及影響其設計和開發的任何一般性限制。A. 產品視角此 WPF IronPython 應用程式被設想為一個獨立的桌面應用程式。它將利用 IronPython 作為嵌入式腳本語言，為核心 C# 應用程式邏輯提供強大的擴展層 1。這種方法允許動態行為和自訂，而無需重新編譯和重新部署主應用程式 4。選擇 IronPython 作為擴展層，而非主要開發語言，是一項策略性決策。這表明核心應用程式受益於 C#/WPF 的穩健性和效能，而 IronPython 則為動態任務和使用者定義邏輯提供了靈活性。這種方法最大限度地降低了純粹動態語言開發相關的風險 1，同時最大限度地提高了靈活性。這種區分隱含著明確的職責分離：核心的、效能關鍵的或安全性敏感的邏輯保留在 C# 中，而使用者導向的、可自訂的或快速變化的邏輯則委託給 IronPython。這種分離直接影響了可維護性，因為更新腳本無需重新編譯，並可能提高安全性，因為腳本只需與受控的 C# 介面互動。這種模式在需要由終端使用者高度自訂或與各種外部系統整合而無需頻繁軟體更新的應用程式中很常見。B. 產品功能
巨集執行： 使用者將能夠編寫、儲存、載入和執行 IronPython 腳本。這些腳本將與 WPF 應用程式的 C# 後端互動，執行資料操作、UI 自動化或自訂業務邏輯等操作 1。
整合式腳本編輯器： 應用程式將包含一個用於 IronPython 腳本的專用程式碼編輯器，具有語法高亮、基本編輯功能，以及關鍵的 IntelliSense 功能。
IntelliSense： 編輯器將為標準 IronPython 語法以及主應用程式公開的 C# API 提供智慧程式碼補全功能 2。
C. 使用者特徵
技術熟練程度： 使用者預期對 Python 語法和程式設計概念有基本了解。他們可能不是 C# 專家，但需要理解公開的 C# API 結構。
目標導向： 使用者主要關心自動化重複任務、自訂工作流程或擴展應用程式功能以滿足特定需求。
學習曲線： IntelliSense 功能對於降低與 C# API 互動的學習曲線至關重要，它能讓使用者有效地發現可用的函數及其參數。
D. 一般限制
.NET Framework/Core 版本： 應用程式必須針對與所選 IronPython 版本相容的.NET 版本（例如，.NET Framework 4.6.2+ 或.NET 6/7/8）4。這對於利用現代.NET 功能和確保長期支援至關重要。
IronPython 版本相容性： 應用程式將使用穩定版的 IronPython，最好是 IronPython 3.4+ 以實現 Python 3 相容性 4。
執行時環境： IronPython 引擎將在與 WPF 應用程式相同的應用程式域 (AppDomain) 中執行，以實現無縫互操作性 3。雖然為了增強安全性，也可以考慮隔離的應用程式域 3。
UI 執行緒安全： IronPython 腳本與 WPF UI 元素之間的互動必須在 UI 執行緒上處理，以防止跨執行緒操作異常。
安全模型： 必須建立一個穩健的安全模型，以控制 IronPython 腳本對底層系統和應用程式資源的存取級別。
III. 特定需求本節詳細說明了 WPF IronPython 應用程式的明確功能和非功能性需求。A. 功能需求1. IronPython 巨集執行

a. 能夠動態載入、解析和執行 IronPython 腳本。應用程式應實例化一個 ScriptEngine（特別是來自 IronPython.Hosting 的 Python.CreateEngine()），以管理 IronPython 執行時環境 1。腳本將作為 ScriptSource 物件從字串或檔案中載入 1。執行將在 ScriptScope 內進行，ScriptScope 作為腳本可存取變數、函數和類別定義的容器 1。
ScriptScope 不僅僅是一個變數容器；它是一個關鍵的隔離邊界。每個巨集都可能在自己的範圍內執行，從而防止不同巨集之間的變數名稱衝突或意外的副作用。這種設計選擇會影響參數傳遞和共享狀態管理的複雜性。最初觀察到 ScriptScope「包含所有您希望『分組』在一起的變數、函數和類別定義」1。對於使用者編寫的巨集，這些巨集是獨立的程式碼片段。如果所有巨集共享一個全域範圍，一個巨集中定義的變數可能會無意中覆蓋或被另一個巨集使用，這將導致不可預測的行為和難以偵錯的惡夢。因此，為不同的巨集或執行上下文使用獨立的 ScriptScope 實例，可以增強模組化並減少耦合，使巨集更穩健且更易於管理。這也支援了多個巨集可能同時執行或需要隔離環境的場景。


b. IronPython 腳本無縫呼叫公共 C# API 和存取 C# 物件的機制。C# 主應用程式應使用 pyScope.SetVariable("variableName", cSharpObjectInstance) 將特定的 C# 物件和實例公開給 IronPython ScriptScope 1。這些公開的物件將可從 IronPython 腳本中直接呼叫。IronPython 腳本應能透過 import clr 後接 clr.AddReference("AssemblyName") 或 clr.AddReferenceToFileAndPath("path/to/assembly.dll") 直接引用.NET 組件 7。這允許存取這些組件中的類型和方法，包括標準.NET Framework/Core 函式庫（例如，用於 MessageBox.Show 的 System.Windows.Forms）7。
clr.AddReference 機制雖然強大，但需要仔細管理。如果組件在執行時由腳本載入，可能會引入意外的依賴或版本衝突。對於生產環境而言，由 C# 主機控制和預先定義一組可存取的組件，是一種更安全的方法。最初觀察到 clr.AddReference 允許載入.NET 組件 16，而使用者希望呼叫 C# API，這些 API 位於組件中。雖然靈活，但允許使用者腳本隨意呼叫 clr.AddReference 可能會導致安全漏洞（例如，載入惡意 DLL）或不穩定性（例如，載入衝突的函式庫版本）。因此，C# 主機應理想地預先載入並僅將必要的應用程式特定組件和一組精選的標準.NET 組件公開給 IronPython 執行時環境。這將創建一個更安全、更穩定的環境，減少攻擊面和潛在的執行時錯誤。
表 1: 公開的 C# API 對應

C# 類別/介面C# 方法/屬性IronPython 名稱 (在範圍內公開)描述參數 (類型, 名稱)回傳類型ApplicationHostLogMessagehost.log記錄訊息到應用程式日誌(string message)voidApplicationHostGetActiveDocumenthost.active_doc取得當前活動文件物件()DocumentDocumentSavedoc.save()儲存文件(string path)boolDocumentGetPageCountdoc.page_count取得文件頁數()intUIControllerShowMessageBoxui.show_message顯示訊息框(string message, string title)voidUIControllerUpdateStatusBarui.status_bar更新狀態列文字(string text)voidDataServiceLoadDatadata.load_csv從 CSV 載入數據(string filePath)DataTableDataServiceProcessDatadata.process_table處理數據表(DataTable table)DataTable

c. 支援 C# 和 IronPython 之間複雜資料類型的傳遞。系統應支援 C# 和 IronPython 之間常用基本類型和物件的隱式轉換 18。透過 SetVariable 傳遞到 ScriptScope 的複雜 C# 物件（例如，自訂類別、集合、WPF UI 元素）應可由 IronPython 腳本存取和操作，包括呼叫方法和存取屬性 1。反之，IronPython 物件（例如，列表、字典、自訂類別）應可在 C# 主機中轉換和使用，可能利用 C# 中的 dynamic 關鍵字實現靈活互動 3。
C# dynamic 關鍵字 5 用於從 C# 消耗 IronPython 結果或與 IronPython 物件互動，是一把雙面刃。雖然它透過繞過靜態類型檢查簡化了互操作性，但它將類型驗證錯誤從編譯時轉移到執行時，這增加了 C#-IronPython 整合層進行健壯單元測試的重要性。最初觀察到 C# dynamic 類型「繞過靜態類型檢查」，並且「編譯器假定動態元素支援任何操作」18。這促進了 C# 和 IronPython 等動態語言之間的「無縫互操作性」2。然而，雖然這對於快速開發和處理來自動態腳本的不可預測類型很方便，但過度依賴 dynamic 可能會掩蓋類型相關的錯誤，直到執行時才被發現，這使得它們在開發過程中更難檢測。因此，dynamic 的優勢（靈活性）是以犧輯時安全性為代價的。這要求更強調執行時驗證、全面的整合測試以及 C# 和 IronPython 之間預期資料契約的清晰文件。


d. 腳本執行失敗的強大錯誤處理和報告。應用程式應以使用者友好的方式捕獲並顯示來自 IronPython 腳本執行的異常 6。錯誤訊息應包含相關詳細資訊，例如行號、錯誤類型和描述性訊息，以幫助使用者偵錯。應實施一種將腳本執行錯誤記錄到內部日誌系統的機制 1。


e. IronPython 腳本範圍和執行時環境的管理。應用程式應管理 ScriptEngine 和 ScriptScope 實例的生命週期，確保正確的初始化和處置 1。應提供重置或重新初始化腳本環境的能力（例如，用於全新執行或腳本更改後）1。IronPython 引擎的額外 Python 模組的搜尋路徑應可配置 3。


f. 從 IronPython 腳本引用外部.NET 組件的能力。使用者應能指定額外的.NET 組件（例如，自訂 DLL 或第三方函式庫），供 IronPython 腳本引用和使用 7。應用程式應提供使用者介面或配置機制，將這些組件引用添加到 IronPython 執行時的搜尋路徑中，或透過 clr.AddReference 明確載入它們。

2. 整合式腳本編輯器與 IntelliSense

a. 用於創建、編輯、儲存和載入 IronPython 巨集的使用者介面。一個專用的 WPF 文字編輯器控制項將整合到應用程式的 UI 中 6。編輯器應支援 IronPython (.py) 腳本檔案的標準檔案操作（新建、開啟、儲存、另存為）。


b. 標準 Python 語法的即時 IntelliSense/自動補全。編輯器應為 Python 關鍵字、內建函數和標準函式庫模組提供自動補全建議 10。這可以透過與外部 Python 語言伺服器整合，或使用模擬 Python 標準函式庫結構的預生成「存根」檔案來實現 10。
在嵌入式 WPF 編輯器中實現標準 Python 語法的 IntelliSense 通常依賴於基於 CPython 的工具（例如，Atom/VS Code 中的 autocomplete-python 或 linter-pylint），這些工具會處理「存根」10。這意味著 IntelliSense 邏輯本身可能不在 IronPython 上運行，而是在一個單獨的 CPython 安裝上運行，該安裝理解 Python 語法並可以使用生成的存根來模擬 IronPython 環境。這為開發時功能引入了對 CPython 安裝的依賴，即使運行時環境僅使用 IronPython。這種設計選擇的影響是，標準 Python IntelliSense 的準確性和完整性將取決於 CPython 工具和生成的存根的質量。這也意味著 IntelliSense 邏輯與實際的 IronPython 運行時解耦，如果存根與 IronPython 版本或其特定行為不完全一致，可能會導致差異。


c. 所有公開 C# API 的 IntelliSense，包括方法簽章和文件。編輯器應為公開給 IronPython ScriptScope 的 C# 類別、物件、方法、屬性和事件提供自動補全功能 1。建議應包含方法簽章（參數類型和名稱）以及可用的文件字串 11。此功能可透過以下方式實現：

存根生成： 創建 Python「存根」檔案（.pyi 或 .py 檔案，定義類別/方法簽章而無實作），以鏡像公開 C# 類型和組件的公共 API 介面 10。這些存根將由編輯器的 IntelliSense 引擎使用。
執行時反射： 動態檢查公開給 ScriptScope 的 C# 物件和已載入組件的類型和成員，並根據此即時資訊填充 IntelliSense 建議。這將需要所選 WPF 編輯器控制項中的自訂補全提供者 20。

在 C# API 的 IntelliSense 方面，選擇存根生成還是執行時反射，是建置時複雜性/準確性與執行時效能/即時性之間的一種權衡。存根生成 11 提供靜態的、預先計算的 IntelliSense，速度更快，但需要單獨的建置步驟，並且可能會錯過動態 API 更改。執行時反射 20 提供即時準確性，但可能計算密集，潛在影響編輯器響應速度，特別是對於大型物件模型或複雜的反射查詢。最初觀察到存根用於 CLR 組件的自動補全 11，而 WPF 語法編輯器支援「自動模式」（來自預建組件）或「自訂模式」（使用者提供列表）的 IntelliSense 20。這意味著存根是靜態表示。如果 C# API 發生變化，必須重新生成並更新存根，這增加了維護開銷。執行時反射雖然實現起來更複雜，但始終會反映 C# API 的當前狀態，這對於動態環境是有益的。因此，該決策會影響開發工作流程（建置時與執行時依賴）、IntelliSense 的準確性（過時的存根與即時反射）以及編輯器的效能特性（靜態存根的快速查找與潛在較慢的動態反射）。混合方法可能是最佳選擇：對常用、穩定的 API 使用存根，對高度動態或使用者定義的物件使用反射。
表 2: IntelliSense 資料來源策略

IntelliSense 目標資料來源機制整合方法關鍵考量 (效能, 準確性, 維護開銷)標準 Python 語法存根檔案 (.pyi/.py)外部語言伺服器 (e.g., CPython-based)效能高，準確性取決於存根更新，需額外 CPython 安裝及維護存根。公開 C# API執行時反射自訂補全提供者 (WPF 編輯器)即時準確，但可能影響編輯器效能，開發複雜度高。.NET 組件 (預載)存根檔案 / 執行時反射自訂補全提供者 / 編輯器原生組件檢查混合模式，兼顧效能與靈活性，需管理存根生成流程。

d. IronPython 程式碼的語法高亮。整合式編輯器應為 Python 關鍵字、字串、註解、數字和其他語言結構提供語法高亮功能 13。這將增強可讀性和程式碼理解。


e. 基本程式碼編輯功能（例如，撤銷/重做、行號、縮排）。編輯器應支援標準文字編輯功能，例如撤銷、重做、剪下、複製、貼上和選取 25。應在程式碼旁顯示行號，以便於導航和錯誤報告 25。應支援自動縮排和反縮排（例如，在按下 Enter 或 Backspace 時），並遵循 Python 的重要空白規則 13。


f. 支援多層次 IntelliSense（例如，object.property.method）。IntelliSense 系統應能夠為巢狀成員提供建議（例如，在輸入 myObject. 後，應建議 myObject 的屬性/方法，然後對於 myObject.Property.，應建議 Property 類型成員）20。這要求 IntelliSense 提供者理解類型關係並遍歷物件圖。

B. 非功能性需求1. 效能：
a. 響應式腳本執行，最小化 UI 凍結。
腳本執行應理想地在後台執行緒上進行，以防止 UI 無響應，並將結果或 UI 更新封送回 UI 執行緒（良好的 WPF 實踐所暗示）。IronPython 引擎本身經過優化，利用 DLR 並生成 MSIL 字節碼，然後由 CLR 轉換為本機程式碼 3。
b. IntelliSense 建議的低延遲。
IntelliSense 建議應在使用者輸入後的幾毫秒內出現，以提供流暢的編碼體驗。這意味著高效的查找機制（例如，預先索引的存根或優化的執行時反射）。
2. 安全性：
a. 防止惡意腳本執行的措施（例如，沙箱、受信任的腳本來源）。
如果應用程式的安全態勢要求，應用程式應實施一種機制來限制 IronPython 腳本的功能，例如限制檔案系統存取或網路操作。從不受信任來源載入的腳本應被標記或在執行前需要明確的使用者批准 26。
b. 受控的 C# API 公開以防止未經授權的存取。
只有明確指定的公共 C# API 和物件應公開給 IronPython 執行時。預設情況下，不應授予對敏感內部應用程式組件或系統資源的直接存取權限。
3. 可用性：
a. 為 Python 開發人員提供直觀且熟悉的編輯器體驗。
整合式編輯器的佈局、快捷方式和功能應與常見的 Python IDE（例如，Visual Studio Code、PyCharm）保持一致，以最大程度地減少使用者的學習曲線 10。
b. 清晰且資訊豐富的腳本問題錯誤訊息。
腳本執行或編譯的錯誤訊息應清晰呈現，指示錯誤的性質、行號，並在可能的情況下建議潛在的解決方案。
4. 可維護性：
a. 模組化設計，便於擴展公開的 C# API。
C# 主應用程式應設計一個清晰的 API 層，專門用於 IronPython 消耗，允許在不對核心應用程式進行重大重構的情況下公開新功能。
b. C# 主機和 IronPython 整合之間職責的明確分離。
IronPython 嵌入邏輯應被封裝，最大程度地減少其對核心 C# 應用程式架構的影響。
5. 相容性：
a. 與指定.NET Framework/.NET Core 版本的相容性。
應用程式必須在選定的.NET 執行時版本上正常運行 4。
b. 與穩定 IronPython 版本的相容性（例如，IronPython 3.4+）。
選定的 IronPython 版本應得到官方支援，並提供巨集執行和 C# 互操作性所需的功能 4。
IV. 架構考量本節概述了滿足指定要求所需的關鍵架構決策和組件。A. DLR 託管 API 使用（ScriptEngine, ScriptScope, ScriptRuntime）

核心組件： IronPython 嵌入的基礎依賴於動態語言運行時 (DLR) 託管 API。

ScriptEngine：用於創建和管理 IronPython 執行環境的主要物件。它透過 Python.CreateEngine() 創建 1。
ScriptScope：表示 Python 程式碼的執行上下文，包含變數、函數和類別定義。它透過 engine.CreateScope() 創建 1。物件可以透過 scope.SetVariable() 公開給腳本 1。
ScriptSource：表示要執行的 Python 程式碼，透過 engine.CreateScriptSourceFromString() 或 engine.CreateScriptSourceFromFile() 從字串或檔案創建 1。
ScriptRuntime：一個可選的更高層次抽象，封裝多個 ScriptEngine 實例並管理共享服務 15。雖然 ScriptEngine 和 ScriptScope 足以滿足基本嵌入需求，但 ScriptRuntime 對於更複雜的託管場景可能很有用。

儘管 ScriptEngine 和 ScriptScope 是基礎，但是否使用 ScriptRuntime 15 取決於腳本環境的規模和複雜性。對於簡單的巨集執行，直接管理 ScriptEngine 和 ScriptScope 就足夠了。然而，如果需要支援多種腳本語言，或者需要在不同的 IronPython 引擎之間共享服務，ScriptRuntime 提供了一個有價值的組織層。這個選擇會影響託管環境的整體複雜性和資源管理。最初觀察到 Python.Hosting 類別「包含用於創建 IronPython 腳本引擎的功能」，並且「大部分功能作為.NET 擴展方法公開，因此它無縫地擴展了正常的 DLR 託管 API 介面」15。這些功能在 ScriptRuntime 或 ScriptEngine 上運行 15。如果應用程式僅使用 IronPython，單個 ScriptEngine 可能就足夠了。如果未來擴展性包括其他 DLR 語言（IronRuby 等），ScriptRuntime 對於管理一個內聚的腳本環境變得更加相關。因此，預先選擇 ScriptRuntime 會增加一層抽象，但為未來的語言整合提供了靈活性。省略它會簡化初始設置，但如果多語言支援成為要求，則可能需要重構。
表 3: 核心 IronPython 託管組件

組件名稱類別/命名空間在託管中的角色關鍵方法/屬性與其他組件的關係ScriptEngineIronPython.Hosting.Python建立和管理 IronPython 執行環境CreateEngine(), CreateScriptSourceFromString(), CreateScope(), Execute()核心，創建 ScriptScope 和 ScriptSourceScriptScopeMicrosoft.Scripting.Hosting腳本執行上下文，包含變數、函數、類別SetVariable(), GetVariable()由 ScriptEngine 創建，執行時的資料容器ScriptSourceMicrosoft.Scripting.Hosting表示要執行的 Python 程式碼Compile(), Execute()由 ScriptEngine 創建，包含腳本內容ScriptRuntimeMicrosoft.Scripting.Hosting封裝多個 ScriptEngine，管理共享服務 (可選)LoadAssembly(), SetSearchPaths()可選，包含 ScriptEngine 實例B. 將 C# 物件和 API 公開給 IronPython 的策略（主機物件，SetVariable，clr.AddReference）

主機物件： 定義一組 C# 類別或介面（例如，如 8 所示的 IScriptContract），這些類別或介面封裝了供 IronPython 腳本使用的功能。此「主機物件」的實例將使用 pyScope.SetVariable("host", hostInstance) 傳遞到 ScriptScope 中 1。這為腳本提供了一個受控且可發現的 API 介面。


直接組件引用： 允許腳本透過 clr.AddReference 直接引用特定的.NET 組件 7。這適用於公開常用.NET 類型（例如，System.IO、System.Collections.Generic）或不屬於核心主機物件的穩定應用程式特定組件。
僅依賴 clr.AddReference 來公開應用程式特定的 C# API 可能會導致腳本與內部 C# 實作之間緊密耦合。而「主機物件」模式 8 提供了一個更健壯且可維護的抽象層，作為腳本環境的門面。這允許 C# 內部重構而不會破壞現有巨集，只要主機物件的介面保持穩定。最初觀察到 clr.AddReference 載入組件 16，而 SetVariable 傳遞物件 1。如果腳本透過 clr.AddReference 直接引用內部 C# 類別，則對這些內部類別的任何更改（例如，重新命名方法、更改命名空間）都會破壞現有腳本。因此，「主機物件」模式為腳本創建了一個穩定、版本化的 API 契約。這將腳本層與 C# 應用程式的內部實作細節解耦，顯著提高了可維護性並減少了內部更改對使用者編寫巨集的影響。

C. IntelliSense 實作策略（存根生成、自訂補全提供者、基於反射的分析）

標準 Python IntelliSense：

存根檔案： 對於標準 Python 函式庫和常見的 IronPython/.NET 組件，可以使用預先生成的「存根」檔案（定義類別/方法簽章而無實作的 .pyi 或 .py 檔案）10。這些存根由標準 Python 語言服務（例如，Atom/VS Code 中的 autocomplete-python）處理，該服務可以與 WPF 編輯器整合。

對「存根」10 的依賴意味著 IntelliSense 引擎通常在 API 的 靜態 表示上運行，而不是即時運行時。這導致如果存根與實際的 IronPython 運行時環境沒有完美同步，或者在存根生成後發生動態 C# API 更改，則可能存在差異。最初觀察到存根是 CPython 自動補全引擎使用的「模擬物件」11，並且存根是透過使用 IronPython「爬取這些函式庫」生成的 11。這表示 IntelliSense 是基於預先生成的檔案。如果底層 C# API 或 IronPython 版本發生變化，這些存根可能會過時，導致建議不準確或缺失。因此，這意味著需要一個健壯的存根生成管道，該管道整合到建置過程中，確保存根始終與公開的 C# API 保持最新。這也表明，對於在運行時生成或修改 C# API 的高度動態場景，純粹基於存根的方法可能不足。


嵌入式編輯器中的 C# API IntelliSense：

自訂補全提供者： 所選的 WPF 文字編輯器組件（例如，AvalonEdit）應支援自訂補全提供者 20。此提供者將：

分析編輯器中的當前文字以確定上下文（例如，myObject.、clr.）。
如果檢測到 C# 物件或 clr 引用，它將查詢即時 IronPython ScriptScope 或使用 C# 反射來檢查公開物件和已載入組件的類型。
動態生成 ICompletionData 項目 21，包括方法名稱、屬性及其簽章/文件，並將它們呈現在補全視窗中。


基於反射的分析： 對於公開的 C# 物件，自訂補全提供者將使用.NET 反射來檢查物件的類型（或其介面）並檢索其公共成員（方法、屬性、事件）。這為即時 C# 物件提供了即時、準確的 IntelliSense 5。

實現使用執行時反射的 C# API 的自訂補全提供者提供了最高的準確性和即時響應性，但這是一項不小的工程任務。它需要對編輯器中 IronPython 程式碼進行複雜的解析，以確定游標處物件的類型，然後有效地執行反射而不會阻塞 UI 執行緒。這種複雜性必須與使用者體驗效益進行權衡。最初觀察到 WPF 語法編輯器支援「自訂模式」IntelliSense，其中使用者「提供項目列表」20，而 AvalonEdit 具有用於自訂補全的 CompletionWindow 和 ICompletionData 21。這意味著可以以程式設計方式生成建議列表。為了對 C# API 執行此操作，編輯器需要知道 IronPython 範圍中可用的 C# 物件/類型及其成員。這意味著編輯器與 IronPython 引擎的當前狀態之間存在橋樑。因此，這種方法允許動態的、上下文感知的 IntelliSense。然而，它需要自訂提供者的大量開發工作，包括 Python 程式碼的詞法分析、類型推斷和高效的反射呼叫。效能是這裡的一個關鍵問題；如果沒有優化，反射可能會很慢，可能導致 UI 延遲。

D. WPF 文字編輯器組件的選擇（評估 AvalonEdit 等開源選項或商業函式庫）
開源選項：AvalonEdit：

優點： 免費、開源 27，高度可自訂 30，提供核心文字編輯功能（語法高亮、行號、撤銷/重做）13。關鍵的是，它透過 CompletionWindow 和 ICompletionData 提供用於自訂程式碼補全的 API 21。
缺點： 需要大量的開發工作才能從頭開始為 IronPython 和 C# API 整合實現語言服務（語法解析、IntelliSense 邏輯）22。編輯器本身不內建偵錯支援（儘管這指的是 VS 整合，而非嵌入式編輯器）13。


商業函式庫：

範例： Syncfusion Essential Edit for WPF 20、Telerik RadSyntaxEditor 25、DevExpress WPF Rich Text Editor 31。
優點： 通常提供更多開箱即用的功能，例如進階語法高亮、程式碼大綱，有時甚至內建語言服務整合。可能為.NET 組件提供「自動模式」IntelliSense 20。
缺點： 授權成本、潛在的供應商鎖定，並且可能仍然需要進行自訂開發，以實現 IronPython 特定的 IntelliSense 或與應用程式公開的 C# API 進行深度整合，而不僅僅是簡單的組件引用。


建議： 對於自訂 IronPython 巨集編輯器，AvalonEdit 提供了靈活而強大的基礎，儘管在 IntelliSense 和語言服務層面需要更高的初始開發投入。商業組件可能會加速基本編輯器功能的實現，但可能無法在沒有大量自訂的情況下完全滿足特定的 IronPython 到 C# IntelliSense 需求。
V. 資料模型本節描述了使用者定義的巨集和 API 配置將如何建構和儲存。A. 儲存使用者定義 IronPython 巨集的結構。巨集應以 .py 副檔名的純文字檔案形式儲存。應用程式的資料夾中應使用專用的目錄結構（例如，AppData\Local\YourApp\Macros）進行組織。巨集的元資料（例如，作者、描述、上次修改日期）可以儲存在腳本檔案中作為文件字串/註解，或儲存在腳本旁邊的單獨清單檔案（例如，JSON/XML）中。B. 動態公開 C# API 的配置。應有一種配置機制（例如，XML、JSON 檔案或內部 C# 程式碼）來定義哪些 C# 類別、方法或屬性公開給 IronPython 執行時。此配置將 C# 類型/成員映射到它們在 IronPython ScriptScope 中的對應名稱（例如，SetVariable("App", new AppHostObject())）。對於 IntelliSense 存根生成，此配置也將作為存根生成工具的輸入，確保只有預期的 API 包含在補全資料中。VI. 未來增強功能（可選）本節概述了在後續開發階段可能添加的潛在功能，以增強應用程式的功能。A. IronPython 腳本的整合式偵錯支援。允許使用者在其 IronPython 腳本中設定斷點。啟用逐步執行、檢查局部變數和呼叫堆疊追蹤 4。這將需要整合 IronPython 的偵錯引擎（例如，Alternet.Studio.Scripter.IronPython 提供 ScriptDebugger 組件 4，或利用 IronPython 的 DLR 偵錯功能建構自訂偵錯整合）。雖然 Visual Studio 為 IronPython 專案提供偵錯功能 13，但在自訂 WPF 應用程式中嵌入功能齊全的偵錯器是一項高度複雜的任務。它涉及與 DLR 的偵錯介面進行深度整合，管理執行緒，並在 UI 中呈現偵錯資訊。這通常是一項重大工程，應在核心巨集執行和 IntelliSense 穩定後才考慮。最初觀察到 Alternet.Studio.Scripter.IronPython 提供 ScriptDebugger 組件 4，且 Visual Studio 具有 IronPython 的偵錯支援 13。使用者希望巨集執行，而偵錯是複雜巨集的自然延伸。從頭開始實作偵錯器極其困難。雖然存在商業組件，但它們會增加成本和依賴性。所需偵錯的級別（例如，基本斷點與完整表達式評估、多執行緒）顯著影響複雜性。因此，是否添加偵錯功能的決定應考慮目標使用者的技術熟練程度以及預期編寫的巨集複雜性。對於簡單巨集，列印語句和基本錯誤報告可能就足夠了。對於複雜的多執行緒巨集，穩健的偵錯器變得至關重要，但同時也是一項重大的開發投資。B. 從腳本進階公開和操作 UI 元素。允許 IronPython 腳本透過將 WPF UI 元素（例如，按鈕、文字框、資料網格）作為 C# 物件公開給腳本範圍來直接存取和操作它們 3。這將使腳本能夠動態更新 UI 內容、響應 UI 事件，甚至生成簡單的 UI 組件。C. 巨集的版本控制整合。直接從編輯器提供與原始碼控制系統（例如，Git）的基本整合，允許使用者提交、拉取和推送巨集更改。VII. 結論與建議本報告詳細闡述了在 WPF 應用程式中整合 IronPython 作為巨集腳本引擎的軟體需求，並特別強調了 C# API 呼叫能力和 IntelliSense 支援。核心結論：
IronPython 作為擴展層的價值： 將 IronPython 嵌入 WPF 應用程式，提供了一個強大且靈活的擴展層，允許使用者自訂和自動化工作流程，而無需重新編譯核心 C# 應用程式。這種架構模式將核心業務邏輯的穩健性與腳本層的敏捷性結合起來，降低了開發風險並提高了應用程式的適應性。
C# API 互操作性的關鍵： 成功實現巨集功能的關鍵在於 C# 主機應用程式如何有效地將其 API 公開給 IronPython。建議採用「主機物件」模式，透過 ScriptScope.SetVariable 1 提供一個受控且解耦的 API 介面。這優於腳本直接透過 clr.AddReference 16 引用內部組件，因為前者能更好地維護 C# 程式碼的內部結構，並減少腳本因 C# 內部變更而失效的風險。
IntelliSense 實作的複雜性與重要性： 提供全面的 IntelliSense 是降低使用者學習曲線和提高巨集開發效率的基石。對於標準 Python 語法，依賴預生成的「存根」檔案 10 並與外部 CPython 語言服務整合是一種可行的方法，但需要仔細管理存根的生成和同步。對於 C# API 的 IntelliSense，實現一個基於執行時反射的自訂補全提供者 20 將提供最高的準確性和即時性，儘管這在技術上更具挑戰性，需要對效能進行細緻的優化。
技術選型與權衡： 在 WPF 文字編輯器組件的選擇上，像 AvalonEdit 這樣的開源選項 27 提供了極高的靈活性和自訂潛力，但需要投入大量的開發工作來構建語言服務。商業函式庫雖然可能提供更多開箱即用的功能，但仍可能需要為特定的 IronPython-C# 互操作性 IntelliSense 進行客製化。
行動建議：
優先構建穩健的 C# API 暴露層： 在開發初期，應專注於設計和實作一個清晰、模組化的 C# API 層，並透過「主機物件」模式安全地將其公開給 IronPython。這將為所有巨集互動奠定堅實的基礎。
分階段實施 IntelliSense：

第一階段： 實施標準 Python 語法的 IntelliSense，利用現有的 CPython 存根生成工具和編輯器整合。
第二階段： 開發自訂補全提供者，利用 C# 反射機制為公開的 C# API 提供即時 IntelliSense。應特別關注效能優化，以確保編輯器響應迅速。


建立自動化存根生成流程： 如果採用存根來支援 IntelliSense，應將存根生成整合到建置流程中，以確保存根始終與 C# API 的最新版本保持同步，避免 IntelliSense 資訊過時。
嚴格的錯誤處理和測試： 由於 IronPython 的動態特性以及 C# dynamic 關鍵字的使用可能將類型錯誤推遲到執行時，因此必須實施全面的錯誤處理機制和徹底的整合測試，以確保巨集的穩定性和可靠性。
考慮長期可維護性： 應用程式的設計應確保 C# 主機和 IronPython 整合之間有明確的職責分離，以便於未來的擴展和維護。
透過遵循這些建議，該 WPF IronPython 應用程式將能夠提供一個強大、使用者友好且可擴展的平台，滿足使用者透過巨集呼叫 C# API 並享受智慧程式碼補全的需求。