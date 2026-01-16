# .NET8 BreakingChangeLab (WPF)

.NET 6 -> .NET 8 ( = .NET 7 + .NET 8 ) 移行で、WPFアプリで実害が出やすい領域を
**最小構成の検証プロジェクト**としてまとめたソリューションです。

> ポイント
> - .NET 8 の公式 breaking changes 一覧には「WPF」がカテゴリとして出てこないため、公式リスト上は 0件扱いになりがちです。
> - ただし現場では **UI Automation (UIA)**、**WindowsFormsHost**、**DnD(DataObject)** あたりで差分が出ることがあります。

## 使い方

1. `DotNet8BreakingChangeLab.sln` を Visual Studio (17.x or 18.x) で開く
2. 各プロジェクトを **スタートアッププロジェクト**にして F5
3. **.NET 6 と比較したい場合**は、各 *.csproj の `<TargetFramework>` を `net6.0-windows` に変更してビルド/実行
   - 例: `net8.0-windows` -> `net6.0-windows`

> 注意: このリポジトリは Windows 上での実行を前提としています（WPF/WinForms/UIA のため）。

---

## プロジェクト一覧

### 1) DnDDataObjectFormatProbe
**目的:** TextBox などのドラッグ&ドロップで `DataObject` の `Format` がどう見えるかをログ出力。

- 右側の Target にドラッグすると、`DragEnter` / `Drop` で `GetFormats()` を記録します。
- .NET 7 以降で「テキストDnDの DataObject フォーマットが変わった」ケースの調査に使えます。

**見るポイント**
- `UnicodeText`, `Text`, `StringFormat` のどれが present になるか
- 自前の DnD ハンドラが「特定フォーマット前提」になっていないか

---

### 2) UiAutomationTreeProbe
**目的:** `System.Windows.Automation` を使って、このウィンドウの UIA ツリーをダンプ。

- `Dump UIA tree` を押すとツリーがログに出ます。
- UI自動テスト（FlaUI/WinAppDriver 等）やアクセシビリティ検証で、.NET 8 への移行で要素探索が壊れるかを確認する目的。

**見るポイント**
- `AutomationId` / `ControlType` / `Name` が想定通りか
- DataGrid/ListBox/ComboBox で子要素構造が変わっていないか

---

### 3) WindowsFormsHostProbe
**目的:** WPF 内に WinForms を埋め込む `WindowsFormsHost` の描画/安定性確認。

- `WindowsFormsHost` 内に WinForms Button を表示し、背景色切り替えを行います。
- Fluent テーマや特定スタイリングとの組み合わせで問題が出る場合があるため、
  その最小再現ベースとして使います。

**見るポイント**
- 黒い矩形、背景色が反映されない、ちらつき、Z-order の違和感
- テーマ/スタイル適用時に症状が出るか

---

## 追加で作れる検証プロジェクト案

- UIA: ListView/DataGrid の仮想化状態での要素探索（Scroll/Virtualization絡み）
- IME/日本語入力: 編集開始/確定/キャンセルのルーティング差分
- Dpi/PerMonitorV2: 複数モニタ移動時の座標/サイズ差分
- Publish/MSIX: SDK変更によるパッケージ生成の差分検証
