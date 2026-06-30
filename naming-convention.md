# C# 命名規則

## 1. 大文字・小文字スタイルの種類

| スタイル | 例 |
|---|---|
| PascalCase | `MyClassName` |
| camelCase | `myVariableName` |
| UPPER_SNAKE_CASE（大文字スネークケース） | `MY_CONSTANT` |

---

## 2. 各要素の命名規則

### クラス・構造体・インターフェース・列挙型

```csharp
// クラス: PascalCase
public class CustomerOrder { }

// 構造体: PascalCase
public struct Point { }

// インターフェース: PascalCase + 先頭に "I"
public interface IRepository { }

// 列挙型: PascalCase
public enum OrderStatus
{
    Pending,
    Completed,
    Cancelled
}
```

### メソッド・プロパティ・イベント

```csharp
// メソッド: PascalCase（動詞または動詞句）
public void SaveOrder() { }
public string GetCustomerName() { }

// プロパティ: PascalCase（名詞または名詞句）
public string FirstName { get; set; }
public int TotalCount { get; }

// イベント: PascalCase（動詞または動詞句、過去形も可）
public event EventHandler OrderCompleted;
public event EventHandler<DataEventArgs> DataReceived;
```
### フィールド・変数・パラメーター

```csharp
// privateフィールド: camelCase（アンダースコアなし）
private string customerName;
private readonly IRepository repository;

// publicフィールド: PascalCase（ただし通常はプロパティを使う）
public string Name;

// ローカル変数: camelCase
int itemCount = 0;
string customerName = "John";

// メソッドパラメーター: camelCase
public void ProcessOrder(int orderId, string customerName) { }
```

### 定数・静的読み取り専用フィールド

```csharp
// 定数: 大文字スネークケース
public const int MAX_RETRY_COUNT = 3;
private const string DEFAULT_PREFIX = "USR_";

// static readonly: 大文字スネークケース
public static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(30);
```

### 型パラメーター（ジェネリクス）

```csharp
// 単一の場合は "T"、複数の場合は意味のある名前 + "T" プレフィックス
public class Repository<T> { }
public class KeyValuePair<TKey, TValue> { }
```

### 名前空間

```csharp
// PascalCase、ドット区切り（会社名.プロジェクト名.機能名）
namespace TechC.Services { }
namespace TechC.Data.Repositories { }
```

---

## 3. 命名規則まとめ表

| 種類 | スタイル | プレフィックス | 例 |
|---|---|---|---|
| クラス | PascalCase | なし | `OrderService` |
| インターフェース | PascalCase | `I` | `IOrderService` |
| 構造体 | PascalCase | なし | `ColorRgb` |
| 列挙型 | PascalCase | なし | `HttpMethod` |
| 列挙値 | PascalCase | なし | `HttpMethod.Get` |
| メソッド | PascalCase | なし | `GetById` |
| プロパティ | PascalCase | なし | `FullName` |
| イベント | PascalCase | なし | `DataLoaded` |
| privateフィールド | camelCase | なし | `userCount` |
| publicフィールド | PascalCase | なし | `MaxSize` |
| ローカル変数 | camelCase | なし | `itemCount` |
| パラメーター | camelCase | なし | `orderId` |
| 定数 | 大文字スネークケース | なし | `MAX_VALUE` |
| 型パラメーター | PascalCase | `T` | `TEntity` |
| 名前空間 | PascalCase | なし | `MyApp.Services` |
