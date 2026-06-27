# C# コード分析パターン集

このドキュメントは、code-analyzer スキルで使用する C# コード解析の技法とパターンをまとめています。

## 1. カバレッジ分析パターン

### Cobertura XML パース

```xml
<coverage>
  <package name="JsonEditor.Core" line-rate="0.6967" branch-rate="0.5127">
    <classes>
      <class name="JsonTreeBuilder" line-rate="0.85" branch-rate="0.72">
        <methods>
          <method name="BuildTree" line-rate="0.92" branch-rate="0.80">
            <!-- テスト対象の行の詳細 -->
          </method>
        </methods>
      </class>
    </classes>
  </package>
</coverage>
```

**解析ポイント**:
```csharp
// 各クラスの line-rate, branch-rate を抽出
double coverage = double.Parse(classElement.Attribute("line-rate")?.Value ?? "0");

// 低カバレッジ基準: < 60%
if (coverage < 0.6)
{
    // リスト対象に追加
}
```

### 未テスト関数の優先順位付け

```
優先度 = テスト必要度スコア × 関数の重要度 × 複雑度

テスト必要度スコア:
  - 100% - (current coverage)
  - 例: 15% coverage → 85 点

関数の重要度:
  - Core 層ロジック: 3.0
  - UI 層のみ: 1.0
  
複雑度:
  - CC <= 5: 1.0
  - CC 5-15: 1.5
  - CC > 15: 2.0

例: JsonTreeBuilder.NormalizeKey (coverage 15%, CC 12)
  = (100-15) × 3.0 × 1.5 = 382.5 (高優先度)
```

## 2. 複雑度分析パターン

### サイクロマティック複雑度（CC）計算

```csharp
// 基本: 1 点から開始、分岐ポイントで +1

public string ExecuteReplace(string input, string pattern, string replacement)
{
    // CC = 1 (開始)
    if (input == null)                      // CC = 2 (if)
        return "";
    
    if (string.IsNullOrEmpty(pattern))      // CC = 3 (if)
        return input;
    
    if (useRegex)                           // CC = 4 (if)
    {
        if (regexCache.Contains(pattern))   // CC = 5 (if)
            return regexCache[pattern].Replace(input, replacement);
        else                                // CC = 6 (else)
            // ...
    }
    else
    {
        switch (mode)                       // CC = 7 (switch)
        {
            case "fast": return FastReplace();    // CC = 8 (case)
            case "safe": return SafeReplace();    // CC = 9 (case)
            default: return input;               // CC = 10 (default)
        }
    }
}
// 最終 CC = 10 (高複雑度、リファクタ推奨)
```

### リファクタ提案パターン

**Before** (CC: 24):
```csharp
public void ProcessJson()
{
    if (IsValid()) {
        if (HasSchema()) {
            if (IsStrict()) {
                // 処理A
            } else {
                // 処理B
            }
        } else {
            // 処理C
        }
    } else {
        if (CanRecover()) {
            // 処理D
        }
    }
}
```

**After** (CC: 6):
```csharp
public void ProcessJson()
{
    // ガード句で早期リターン
    if (!IsValid() && !CanRecover())
        return;
    
    if (IsStrict())
        ProcessStrict();
    else
        ProcessNormal();
}

private void ProcessStrict()
{
    if (HasSchema())
        ProcessWithSchema();  // 処理A
    else
        ProcessWithoutSchema();  // 処理B
}
```

### ネストの深さ測定

```
許容レベル:
  - 1-2 層: ✓ OK（理解容易）
  - 3-4 層: ⚠️ 注意（テストしづらい）
  - 5 層以上: 🔴 NG（リファクタ必須）

測定方法: インデント数をカウント
```

## 3. 依存関係分析パターン

### クラス間依存グラフ構築

```
収集: using, new, プロパティ型 から依存を抽出

MainViewModel
  ├─→ IJsonTreeBuilder
  ├─→ IJsonValidationService
  ├─→ ISearchReplaceService
  ├─→ IAppSettingsStore
  ├─→ IFileDialogService
  └─→ IMessageBoxService

JsonTreeBuilder
  ├─→ JsonTreeNode
  ├─→ List<>
  └─→ (他の Core クラス)
```

### 循環依存検出

```
サイクル検出アルゴリズム:
  1. グラフを構築
  2. DFS (深さ優先探索) で訪問追跡
  3. 訪問中のノードに到達 → 循環検出

例（NG）:
  ServiceA → ServiceB → ServiceC → ServiceA ❌

現プロジェクト:
  UI層 → Core層 → Models層 ✓ (DAG = OK)
```

### 層構造違反検出

```
許容構造:
  ┌─────────────────┐
  │ UI (App)        │  ← ユーザー操作
  └─────────┬───────┘
            │
            ↓
  ┌─────────────────┐
  │ Services (Core) │  ← ビジネスロジック
  └─────────┬───────┘
            │
            ↓
  ┌─────────────────┐
  │ Models (Core)   │  ← データ定義
  └─────────────────┘

違反例:
  ❌ Models が Services を参照
  ❌ Services が App を参照
  ❌ Models が App を参照
```

## 4. 命名規則チェックパターン

### パターンマッチング正規表現

```csharp
// C# 命名規則パターン
Dictionary<string, string> patterns = new()
{
    // クラス、インターフェース, 列挙型
    ["class|interface|enum"] = @"^[A-Z][a-zA-Z0-9]*$",
    
    // メソッド（最初の大文字必須）
    ["method"] = @"^[A-Z][a-zA-Z0-9]*$",
    
    // プロパティ（最初の大文字必須）
    ["property"] = @"^[A-Z][a-zA-Z0-9]*$",
    
    // ローカル変数・パラメータ（小文字開始）
    ["variable|parameter"] = @"^[a-z][a-zA-Z0-9]*$",
    
    // 非公開メンバー（アンダースコア + camelCase）
    ["private_field"] = @"^_[a-z][a-zA-Z0-9]*$",
    
    // 定数（全大文字 または PascalCase）
    ["constant"] = @"^([A-Z][A-Z0-9]*|[A-Z][a-zA-Z0-9]*)$",
    
    // インターフェース（I + PascalCase）
    ["interface"] = @"^I[A-Z][a-zA-Z0-9]*$",
};
```

### 違反パターン例

```csharp
// ❌ NG パターン

private string jsonPath;  // ← underscore 欠落
// 修正: private string _jsonPath;

public string JsonFileName { get; set; }  // OK
public string json_file { get; set; }     // ← snake_case (NG)
// 修正: public string JsonFile { get; set; }

const int max_items = 10;  // ← snake_case (NG)
// 修正: const int MAX_ITEMS = 10; または const int MaxItems = 10;

void process_data() { }  // ← snake_case (NG)
// 修正: void ProcessData() { }

var myList = new List<int>();  // OK
var my_list = new List<int>();  // ← snake_case (NG)
// 修正: var myList = new List<int>();
```

## 5. Null 安全性分析パターン

### Nullability Context 確認

```xml
<!-- .csproj ファイル内 -->
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
  <Nullable>enable</Nullable>  ← ✓ 有効化
  <Nullable>disable</Nullable>  ← ❌ 無効（推奨しない）
</PropertyGroup>
```

### Null チェック漏れパターン

```csharp
// ❌ NG: 型は非null だが値が null で初期化
public string Name { get; set; }  // string は非null ゆえ null 不可
public string Name { get; set; } = null;  // ❌ 型チェッカーエラー

// ✅ OK: null 許容型で宣言
public string? Name { get; set; }  // string? は null 許容

// ❌ NG: 戻り値 null チェック漏れ
var item = GetItemById(id);
Console.WriteLine(item.Id);  // item が null なら NPE

// ✅ OK: null チェック または null コアレッシング
var item = GetItemById(id);
var id = item?.Id ?? 0;  // null 安全

// ✅ OK: throw 式
var item = GetItemById(id) ?? throw new InvalidOperationException("Item not found");
```

### 脆弱性の優先度付け

```
優先度 = 発生確率 × 影響度

発生確率:
  - 高 (常に null 可能性): 3.0
  - 中 (場合による): 2.0
  - 低 (稀): 1.0

影響度:
  - クリティカル (アプリクラッシュ): 3.0
  - 高 (機能不全): 2.0
  - 中 (警告ログ出力): 1.0

例: GetNode()?.Key ?? "" (チェック済み) = 低優先度
例: SelectedNode.Delete() (null チェックなし) = 高優先度
```

## 6. パフォーマンス分析パターン

### ボトルネック検出

```csharp
// ❌ NG: 遅い文字列連結（O(n²)）
string result = "";
foreach (var item in items)
{
    result += item.ToString();  // 毎回、文字列全体をコピー
}

// ✅ OK: StringBuilder（O(n)）
var sb = new StringBuilder();
foreach (var item in items)
{
    sb.Append(item.ToString());
}
string result = sb.ToString();

// ❌ NG: LINQ の非効率な使用
var found = items.Where(x => x.Id == targetId).FirstOrDefault();

// ✅ OK: Dictionary を活用
var dict = items.ToDictionary(x => x.Id);
var found = dict.TryGetValue(targetId, out var item) ? item : null;

// ❌ NG: N+1 クエリ問題
foreach (var parent in parents)
{
    foreach (var child in GetChildren(parent.Id))  // 毎回 DB 問い合わせ
    {
        // ...
    }
}

// ✅ OK: 一度に取得
var childrenMap = GetAllChildren().GroupBy(x => x.ParentId).ToDictionary(...);
foreach (var parent in parents)
{
    if (childrenMap.TryGetValue(parent.Id, out var children))
    {
        foreach (var child in children) { }
    }
}
```

### パフォーマンス計測

```csharp
var sw = Stopwatch.StartNew();
MethodToAnalyze();
sw.Stop();

Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"Operations/sec: {count / (sw.Elapsed.TotalSeconds)}");
```

## 参考資料

- [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Cyclomatic Complexity - Wikipedia](https://en.wikipedia.org/wiki/Cyclomatic_complexity)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [C# Nullable Reference Types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-reference-types)

---

**作成日**: 2026-06-27  
**対象プロジェクト**: JsonEditor (C# .NET 8 WPF)
