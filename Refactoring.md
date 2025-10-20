# リファクタリングノート

## 命名規則

### 1. クラス・構造体・インターフェース
- **パスカルケース**（例: `PlayerController`, `EnemyFactory`, `IInputManager`）
- インターフェースは `I` プレフィックス（例: `IWeapon`）
- ScriptableObjectは `SO` サフィックス（例: `WaveConfigSO`)

### 2. メソッド・プロパティ
- **パスカルケース**（例: `TakeDamage()`, `GetWaveForTime()`）
- プロパティは名詞または形容詞（例: `IsAlive`, `CurrentHealth`）
- アクセサは `Get`/`Set` プレフィックスを推奨

### 3. 変数・フィールド
- **キャメルケース**（例: `currentHealth`, `spawnInterval`）

- 定数は全て大文字＋アンダースコア（例: `MAX_ENEMY_COUNT`)

### 4. 名前空間
- プロジェクト名から始める（例: `SGC2025.Enemy`、`SGC2025.Player`）
- Editor用は `.Editor` サフィックス（例: `SGC2025.Editor`)

### 5. ファイル名
- クラス名と一致させる（例: `PlayerController.cs`）
- ScriptableObjectは `SO` サフィックスを付ける

### 6. その他
- シングルトンを使う場合はSingleton.csを継承して使うこと
- テスト用クラスは `Test` サフィックス（例: `PlayerControllerTest`)

---

## コード規約

### 1. 基本ルール
- 1ファイル1クラスを原則とする
- if文のネストは最大4段まで（それ以上はメソッド分割・早期return推奨）
- 1メソッドの行数は最大40行程度を目安
- 1クラスの行数は最大300行程度を目安
- メソッドの引数は3つまで（多い場合はDTO/構造体でまとめる）
- 早期return・ガード句を積極的に使い、ネストを浅く保つ
- マジックナンバー禁止。定数・設定ファイル・SOで管理
- publicフィールド禁止。プロパティまたはprivate+SerializeFieldを使う
- 例外処理はcatchで必ずログ出力

### 2. コメント・ドキュメント
- クラス宣言・publicメソッドにはXMLサマリー（`/// <summary>...</summary>`）を必ず記載
- 重要なロジックには1行コメントを添える

### 3. レイアウト・インデント
- インデントはスペース4つ（タブ禁止）
- ブレース `{}` は必ず改行して書く（K&R（カーニハン・アンド・リッチー）スタイル禁止）
- メソッド間は1行空ける

### 4. 命名・可読性
- 変数名・メソッド名は意味が分かる英語で
- 略語は原則禁止（例: `cnt` → `count`）
- 一時変数は `temp` や `result` など汎用名を避ける

### 5. Unity固有
- MonoBehaviour継承クラスは必ずAwake/Start/OnDestroyを明示的に記載
- Inspectorで設定するフィールドは `[SerializeField] private` を原則
- Update()は必要最小限の処理のみ。重い処理はコルーチンやイベントで分離

---
