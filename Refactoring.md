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
- 200~300行くらいのクラスには`#region`を使用すること

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

## SOLID原則（設計指針）

### 1. 単一責任の原則（Single Responsibility Principle）
- クラス・メソッドは「1つの責務（役割）」だけを持つ
- 例：Playerクラスはプレイヤーの状態管理のみ、入力処理はInputManagerに分離

### 2. オープン/クローズドの原則（Open/Closed Principle）
- クラス・関数は「拡張に対して開かれ、修正に対して閉じている」
- 例：新しい敵タイプ追加時は既存クラスを修正せず、継承やインターフェースで拡張

### 3. リスコフの置換原則（Liskov Substitution Principle）
- 派生クラスは親クラスと置き換えても正しく動作する
- 例：EnemyBaseを継承した全ての敵クラスはEnemyBase型として扱える

### 4. インターフェース分離の原則（Interface Segregation Principle）
- 不要な機能を持つ大きなインターフェースを避け、役割ごとに分割
- 例：IDamageable, IMovableなど、必要な機能だけを持つインターフェース

### 5. 依存性逆転の原則（Dependency Inversion Principle）
- 具体的な実装ではなく、抽象（インターフェースや抽象クラス）に依存する
- 例：WeaponSystemはIWeaponインターフェースに依存し、具体的な武器実装は注入

---
