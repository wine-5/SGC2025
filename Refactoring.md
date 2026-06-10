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
- 使用していない名前空間は削除する
- 不要な改行の削除

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

## Enemy クラス設計リファクタリング方針

### 現状の問題

1つのEnemyプレハブに `EnemyController` / `EnemyMovement` / `EnemyAutoReturn` の3つのMonoBehaviourが横並びになっており、上位層（ファサード）が不明確。  
外部クラス（`EnemyFactory` / `EnemySpawner`）が複数の `GetComponent` を呼ぶ必要があり、依存が分散している。

### 目標とする構造

```
GameObject "Enemy"
 └── EnemyController（MonoBehaviour・唯一の窓口）
       ├── EnemyMovement（plain C#・EnemyControllerが所有・Tick()で駆動）
       └── ライフタイム管理（EnemyAutoReturnのロジックをUpdate()に統合）
```

### 各クラスの責任

| クラス | 種別 | 責任 |
|--------|------|------|
| `EnemyController` | MonoBehaviour | HP・状態・Updateループ・外部への唯一の窓口 |
| `EnemyMovement` | plain C# | 移動計算（戦略パターン維持） |
| `EnemyDataSO` | ScriptableObject | 純粋なデータのみ（`InitializeController()`は削除） |
| `EnemyFactory` | Singleton | `EnemyController` 1つだけに触れる |
| `EnemySpawner` | MonoBehaviour | `EnemyFactory.CreateEnemy()` を呼ぶだけ |
| `EnemySpawnPositionManager` | plain C# | インターフェースなしで直接使用 |

### 変更内容

#### 削除するもの
- `EnemyAutoReturn.cs` → `EnemyController` に統合
- `ISpawnPositionProvider.cs` → 実装クラスが1つしかないため不要
- `EnemyDataSO.InitializeController()` → SOが Controllerを知るのは責任の逆転

#### 変更しなくていいもの
- 移動戦略パターン（`IMovementStrategy` + 各 Strategy クラス）
- `EnemySpawnConfigSO` / `WaveDataSO`（データ設計として問題なし）
- `EnemySpawnConfigManager`

### 実装の順番

1. `EnemyMovement` を plain C# 化 → `EnemyController` が所有・駆動
2. `EnemyAutoReturn` のロジックを `EnemyController` に統合して削除
3. `EnemyDataSO.InitializeController()` を削除
4. `EnemyFactory` / `EnemySpawner` を整理（`GetComponent` を1回に削減）
5. `ISpawnPositionProvider` を削除し `EnemySpawnPositionManager` を直接使用

---

## フォルダ構造リファクタリング方針

### 現状の問題

```
Scripts/
 ├── Manager/
 │    └── WaveManager.cs          ← 管理クラスはここにある
 └── InGame/
      ├── Enemy/
      │    └── Wave/
      │         └── WaveDataSO.cs ← データだけEnemyの中にある（不整合）
      └── Ground/
           ├── GameManager.cs     ← Groundと無関係なのにGroundフォルダにある
           └── GroundDataSO.cs
```

**問題1:** `WaveDataSO` が `Enemy/Wave/` にある  
WaveはEnemyだけの概念ではなく、`WaveManager` はすでに `Scripts/Manager/` にある。データだけがEnemy配下に置かれており一貫性がない。

**問題2:** `GameManager` が `Ground/` にある  
`GameManager` は地面とは無関係のクラスで、`GroundDataSO` と同列に置く理由がない。

**問題3:** `Enemy/` 内の `Interface/` フォルダが不要  
1ファイルのためだけに `Interface/` フォルダを作るのは逆に可読性を下げる。

### 目標とする構造

```
Scripts/
 ├── Manager/
 │    ├── WaveManager.cs
 │    ├── WaveDataSO.cs           ← Enemy/Wave/ から移動
 │    ├── InGameManager.cs
 │    ├── GroundManager.cs
 │    └── GameManager.cs          ← InGame/Ground/ から移動
 └── InGame/
      ├── Enemy/
      │    ├── Core/
      │    ├── Movement/
      │    │    ├── IMovementStrategy.cs   ← Interface/ フォルダを廃止して直下に
      │    │    └── Strategy/
      │    └── Spawning/           ← Interface/ フォルダ削除（ISpawnPositionProvider自体も削除）
      └── Ground/
           └── GroundDataSO.cs    ← データのみ残す
```

### 注意事項
- **ファイル移動は必ずUnityエディタのProjectウィンドウ上で行うこと**
- ExplorerやターミナルでのファイルコピーはNG（`.meta` ファイルの紐付けが壊れてプレハブ参照が全て切れる）

---

## Item クラス設計リファクタリング方針

### 現状の問題

**問題1: `ItemDataSO` に不要なメソッドがある**  
`AddItem()` / `RemoveItem()` はどこからも呼ばれておらず、ScriptableObjectのデータをランタイムで書き換えるのは設計的にも誤り。削除する。

**問題2: `ItemController` に責任が混在している**  
`ItemController` はフィールド上のアイテムオブジェクトの制御が責任だが、デバッグ用のアイテム生成機能（`SpawnDebugItemNearPlayer` / `IsDebugKeyPressed`）が混入している。これは `ItemManager` の責任範囲であり、Steam リリース前に削除すべき。

**問題3: `ItemFactory` が不要にシングルトンになっている**  
`ItemFactory` への生成呼び出しは `ItemManager` からのみ。`ItemManager` が `[SerializeField]` で直接参照を持てばシングルトン不要。

**問題4: `ItemController` が `ItemFactory` を直接知っている**  
`ReturnToPool()` 内で `ItemFactory.I.ReturnItem()` を直接呼んでいる。返却の窓口は `ItemManager` に統一し、`ItemController` は `ItemManager.I.ReturnItem()` を呼ぶようにする。

### 各クラスの責任（目標）

| クラス | 種別 | 責任 |
|--------|------|------|
| `ItemManager` | Singleton | 生成タイミング・効果の適用と管理・返却の窓口 |
| `ItemFactory` | MonoBehaviour（非Singleton） | ObjectPoolのラッパー。ItemManagerが直接参照を持つ |
| `ItemController` | MonoBehaviour | フィールドアイテムの動作（回転・ライフタイム・当たり判定）のみ |
| `ItemDataSO` | ScriptableObject | 純粋なデータのみ（`AddItem`/`RemoveItem` 削除） |

### 変更内容

#### 削除するもの
- `ItemDataSO.AddItem()` / `RemoveItem()`
- `ItemController` 内のデバッグ関連フィールド・メソッド一式（`enableDebugSpawn`, `debugSpawnKey`, `debugSpawnDistance`, `testItemObj`, `SpawnDebugItemNearPlayer()`, `IsDebugKeyPressed()`）

#### 変更するもの
- `ItemFactory` から `Singleton` 継承を外し、`ItemManager` が `[SerializeField]` で参照を持つ
- `ItemController.ReturnToPool()` を `ItemFactory.I` から `ItemManager.I.ReturnItem()` に変更
- `ItemManager.ReturnItem(GameObject)` を public に追加

#### 変更しなくていいもの
- `ItemSpawnSelector`（重み付き抽選の設計として問題なし）
- `ItemManager` の効果管理ロジック

### 追加指摘事項

**問題5: `ItemData` と `ItemDataSO` が同一ファイルに同居している**  
コード規約「1ファイル1クラス」に違反。`ItemData.cs` として分離する。

**問題6: `ApplyEffect()` の `switch` に `throw new NotImplementedException` がある**  
新しい `ItemType` を追加したとき更新し忘れると本番で例外クラッシュが発生する。  
`default` は `Debug.LogWarning` + `break` に変更する。  
また機能追加が増える場合は、`switch` 自体を廃止して `ItemData` 側に視覚エフェクトの種類を持たせるデータドリブン方式を検討する。

**問題7: `ItemController.lifeTime` と `ItemData.Duration` が二重になっている**  
`Initialize(ItemData data)` 呼び出し時に `lifeTime` が `data.Duration` で上書きされず、インスペクターの固定値が使われてしまう。  
`Initialize()` 内で `lifeTime = data.Duration` を設定するか、フィールドを削除してSOの値を直接参照する。

**問題8: `ItemManager.SpawnItem()` の戻り値を捨てている**  
`GameObject item = ItemFactory.I.SpawnItem(...)` の戻り値を変数に入れているが何もしていない。  
`void` に統一して変数代入を削除するか、将来の拡張に備えて `return GameObject` にするか統一する。
