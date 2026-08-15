# 開發日誌

## 2026-08-15 09:21 建立開發日誌
### 目前進度:
採用DFS法遞迴嘗試排班，每輪搜尋都只會有一個結果物件以節省空間
<br>Solution分前端Blazor、業務邏輯Core、資料庫物件Data、後端Server四個專案 

Core:
- 基本排班遞迴架構、邏輯完成 未驗證邏輯正確性
- 目前採用每個半時都嘗試所有排班，以獲得所有可能性的排法

Blazor
- MudBlzor套件設定

## 2026-08-15 10:38 
- 建立blazor頁面顏色設定、layout
- 修改Icon、把專案建立時預設css、layout、navbar刪除、改為MudBlazor
- 修正App.razor的設定、刪除bootstrap檔案
### 下次開發預計:
- 思考建立主頁內容、navbar內容、setting頁面

## 2026-08-15 16:40
- 前端設定頁面建立
- 建立專案.Shared 用來放前後端溝通dto
### 下次開發: 補上設定頁面其他設定值