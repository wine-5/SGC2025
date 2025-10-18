# Enemy関連クラス設計

## 既に考えられているクラス

### EnemyBase
敵キャラクターの基底クラス。HP、移動、ダメージ処理などの共通機能を提供

### EnemyFactory (Singleton<EnemyFactory>を継承)
敵の生成・プール管理を行うファクトリークラス

### EnemyController
敵の基本パラメーター（速度、HP、攻撃力など）を管理するクラス

### EnemyType (enum)
敵の種類を定義する列挙型

### EnemySpawner
ランダムな位置に敵を生成するクラス

### EnemyDataSO (ScriptableObject)
敵の出現確率やパラメーターを設定するSO

## 追加で必要になりそうなクラス？

### EnemyAI
敵のAI行動パターン（プレイヤー追従、パトロール等）を管理するクラス

### EnemyMovement
敵の移動処理を専門に扱うクラス

### EnemyHealthSystem
敵のHP管理、ダメージ処理、死亡処理を担当するクラス

### EnemyAnimationController
敵のアニメーション制御を行うクラス

### EnemySpawnPoint
敵の生成ポイントを管理するクラス

### EnemyDeathHandler
敵の死亡時処理（色塗り、エフェクト、スコア等）を担当するクラス


## 実装順序

### Phase 1: 基盤構築
1. **EnemyType (enum)** - 敵の種類定義
2. **EnemyDataSO** - 敵のパラメーター設定用SO
3. **EnemyController** - 基本パラメーター管理
4. **EnemyBase** - 敵の基底クラス

### Phase 2: 基本機能実装
5. **EnemyHealthSystem** - HP管理、ダメージ処理
6. **EnemyMovement** - 移動処理
7. **EnemyAI** - 基本的な追従AI

### Phase 3: 生成・管理システム
8. **EnemyFactory** - プール管理とファクトリー
9. **EnemySpawnPoint** - 生成ポイント管理
10. **EnemySpawner** - ランダム生成機能

### Phase 4: ゲーム機能統合
11. **EnemyDeathHandler** - 死亡処理と色塗り機能
12. **EnemyAnimationController** - アニメーション制御