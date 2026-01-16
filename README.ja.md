# .NET 6 → 8 移行時の WPF 影響チェックリスト

> 対象：
> - WPF / Desktop アプリ
> - .NET 6 → .NET 8（= .NET 7 + .NET 8 の変更をまとめて踏む）
>
> 判定基準：
> - **A** = 実害が出やすく、必ず確認すべき
> - **B** = 条件次第で影響あり。該当するなら確認
> - **C** = 基本はそのままでOK（念のため確認）

---

## 影響ポイント一覧（優先度順）

| 優先 | 影響ポイント | 何が起きる？（代表的な症状） | 該当しやすい条件 | 最短チェック方法 | 対策の方向性 |
|---|---|---|---|---|---|
| **A** | **UI Automation / 自動UIテスト / アクセシビリティ** | UIAツリー構造が変わり、要素が見つからない／自動テストが失敗 | FlaUI / WinAppDriver / Appium / Accessibility Insights を使用 | .NET6版と.NET8版で同画面を Accessibility Insights で比較 | `AutomationProperties.*` 明示、必要に応じて AutomationPeer 調整 |
| **A** | **WindowsFormsHost + Fluent/テーマ** | WinFormsHost が黒くなる、描画崩れ、色がおかしい | WPF に WinForms コントロールを埋め込んでいる | Fluent テーマ ON/OFF で該当画面を目視確認 | テーマ回避、Host 分離、コンポーネント更新 |
| **A** | **ビルド環境（VS / MSBuild / SDK）** | ローカルは通るが CI だけ失敗／一部 PC でビルド不可 | CI や別 PC が古い BuildTools を使用 | CI で `dotnet --info` / BuildTools バージョン確認 | BuildTools 更新、SDK 固定、CI イメージ更新 |
| **B** | **TextBox / RichTextBox の Drag & Drop（.NET7由来）** | DnD の DataObject フォーマット前提コードが動かない | TextBox 等の DnD をフックして中身を解析 | TextBox 間 DnD + 自前ハンドラ画面を検証 | Format 前提を廃止、複数フォーマット許容 |
| **B** | **サードパーティ WPF UI ライブラリ** | テーマ・入力・自動化・仮想化の細部が変わる | WPF-UI / MahApps / Telerik 等を使用 | 主要画面の回帰テスト | ライブラリ更新、既知問題回避、スタイル調整 |
| **B** | **MSIX / Store 配布パイプライン** | publish / 署名 / 生成物で詰まる | Store 配布、MSIX 生成を行っている | `dotnet publish` + MSIX をローカル/CI両方で確認 | Packaging プロジェクト更新、手順固定化 |
| **B** | **SingleFile / Trim（使用時のみ）** | 起動失敗、XAML ロード失敗 | single-file 化や trimming を使用 | 該当 publish 設定で起動テスト | WPF は原則 Trim 非推奨、例外指定 |
| **C** | **WPF 本体の一般挙動** | 体感が少し滑らかになる程度 | 通常の WPF | スクロール・選択・編集の簡易回帰 | 問題が出た箇所のみ局所対応 |
| **C** | **パフォーマンス / GC / JIT 改善** | フリーズや引っ掛かりが減る方向 | 大量データ・仮想化使用 | 同条件で体感＋簡易計測 | 改善前提にしすぎない（副作用は少ない） |

---

## FastExplorer / 類似アプリ向け 最優先確認トップ3

1. **UI Automation（自動UIテスト・アクセシビリティ）を使っているか**
2. **WindowsFormsHost を使用しているか**
3. **CI / 別PCビルド環境が存在するか（BuildTools 世代差）**

---

## 結論

- **.NET 8 の WPF 公式破壊的変更は 0 件**
- ただし実務では  
  **UIA / WinFormsHost / ビルド環境** が主な地雷
- WPF 本体そのものは **.NET 6 → 8 で非常に安定**

👉 新規開発・基盤ライブラリは **.NET 8 LTS 推奨**
