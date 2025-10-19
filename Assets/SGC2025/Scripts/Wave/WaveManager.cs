// using UnityEngine;
// using TechC;
// using SGC2025.Enemy;

// namespace SGC2025
// {
//     /// <summary>
//     /// Wave進行を管理するマネージャー
//     /// 時間経過によるWave変更とEnemySpawnerとの連携を行う
//     /// </summary>
//     public class WaveManager : Singleton<WaveManager>
//     {
//         [Header("Wave設定")]
//         [SerializeField] private WaveConfigSO waveConfig;
        
//         [Header("連携するスポーナー")]
//         [SerializeField] private EnemySpawner enemySpawner;
        
//         [Header("デバッグ設定")]
//         [SerializeField] private bool enableDebugLog = false;
        
//         // 現在の状態
//         private WaveConfigSO.WaveData currentWave;
//         private float gameStartTime;
//         private bool isWaveSystemActive = false;
        
//         // イベント
//         public static event System.Action<WaveConfigSO.WaveData> OnWaveChanged;
//         public static event System.Action<int> OnWaveLevelChanged;
        
//         // プロパティ
//         public WaveConfigSO.WaveData CurrentWave => currentWave;
//         public int CurrentWaveLevel => currentWave?.waveLevel ?? 1;
//         public float ElapsedTime => Time.time - gameStartTime;
//         public bool IsActive => isWaveSystemActive;
        
//         protected override void Init()
//         {
//             base.Init();
            
//             if (waveConfig == null)
//             {
//                 Debug.LogError("[WaveManager] WaveConfigSOが設定されていません！");
//                 return;
//             }
            
//             // EnemySpawnerが設定されていない場合は自動で検索
//             if (enemySpawner == null)
//             {
//                 enemySpawner = FindFirstObjectByType<EnemySpawner>();
//                 if (enemySpawner == null)
//                 {
//                     Debug.LogError("[WaveManager] EnemySpawnerが見つかりません！");
//                     return;
//                 }
//             }
            
//             // ゲーム開始時間を記録
//             gameStartTime = Time.time;
            
//             // 最初のWaveを設定
//             currentWave = waveConfig.GetFirstWaveOrNull();
            
//             if (currentWave != null)
//             {
//                 isWaveSystemActive = true;
//                 ApplyCurrentWave();
                
//                 if (enableDebugLog)
//                 {
//                     Debug.Log($"[WaveManager] Wave システム開始 - 初期Wave: {currentWave.waveName}");
//                 }
//             }
//             else
//             {
//                 Debug.LogWarning("[WaveManager] 有効なWaveが設定されていません");
//             }
//         }
        
//         private void Update()
//         {
//             if (!isWaveSystemActive || waveConfig == null) return;
            
//             CheckForWaveTransition();
//         }
        
//         /// <summary>
//         /// Wave遷移の確認
//         /// </summary>
//         private void CheckForWaveTransition()
//         {
//             float currentTime = ElapsedTime;
//             WaveConfigSO.WaveData newWave = waveConfig.GetWaveForTime(currentTime);
            
//             // 新しいWaveに遷移すべきか確認
//             if (newWave != null && newWave != currentWave)
//             {
//                 ChangeToWave(newWave);
//             }
//         }
        
//         /// <summary>
//         /// 指定されたWaveに変更
//         /// </summary>
//         private void ChangeToWave(WaveConfigSO.WaveData newWave)
//         {
//             if (newWave == null) return;
            
//             WaveConfigSO.WaveData previousWave = currentWave;
//             currentWave = newWave;
            
//             ApplyCurrentWave();
            
//             // イベント通知
//             OnWaveChanged?.Invoke(currentWave);
//             OnWaveLevelChanged?.Invoke(currentWave.waveLevel);
            
//             if (enableDebugLog || newWave.enableDebugLog)
//             {
//                 Debug.Log($"[WaveManager] Wave変更: {previousWave?.waveName ?? "None"} → {newWave.waveName} " +
//                          $"(Level: {newWave.waveLevel}, Time: {ElapsedTime:F1}s)");
//             }
//         }
        
//         /// <summary>
//         /// 現在のWave設定をEnemySpawnerに適用
//         /// </summary>
//         private void ApplyCurrentWave()
//         {
//             if (currentWave == null || enemySpawner == null) return;
            
//             // スポーン間隔を更新
//             enemySpawner.SetSpawnInterval(currentWave.spawnInterval);
            
//             // Waveレベルを更新
//             enemySpawner.SetWaveLevel(currentWave.waveLevel);
            
//             // 敵設定を更新（実装が必要）
//             ApplyEnemyConfigs();
            
//             if (enableDebugLog || currentWave.enableDebugLog)
//             {
//                 Debug.Log($"[WaveManager] Wave設定適用完了 - " +
//                          $"間隔: {currentWave.spawnInterval}s, レベル: {currentWave.waveLevel}");
//             }
//         }
        
//         /// <summary>
//         /// 敵設定を適用（将来的にEnemySpawnerの拡張が必要）
//         /// </summary>
//         private void ApplyEnemyConfigs()
//         {
//             // TODO: EnemySpawnerに特定のEnemySpawnConfigSOを使用する機能を追加する必要がある
//             // 現在は基本的なWaveレベルのみ適用
            
//             if (currentWave.enableDebugLog)
//             {
//                 Debug.Log($"[WaveManager] 敵設定数: {currentWave.enemyConfigs.Count}個");
//             }
//         }
        
//         /// <summary>
//         /// Wave システムを停止
//         /// </summary>
//         public void StopWaveSystem()
//         {
//             isWaveSystemActive = false;
            
//             if (enableDebugLog)
//             {
//                 Debug.Log("[WaveManager] Wave システムを停止しました");
//             }
//         }
        
//         /// <summary>
//         /// Wave システムを再開
//         /// </summary>
//         public void ResumeWaveSystem()
//         {
//             if (waveConfig != null && currentWave != null)
//             {
//                 isWaveSystemActive = true;
                
//                 if (enableDebugLog)
//                 {
//                     Debug.Log("[WaveManager] Wave システムを再開しました");
//                 }
//             }
//         }
        
//         /// <summary>
//         /// 強制的に次のWaveに変更（デバッグ用）
//         /// </summary>
//         [ContextMenu("Force Next Wave")]
//         public void ForceNextWave()
//         {
//             if (waveConfig == null || currentWave == null) return;
            
//             WaveConfigSO.WaveData nextWave = waveConfig.GetNextWave(currentWave);
//             if (nextWave != null)
//             {
//                 ChangeToWave(nextWave);
//             }
//             else
//             {
//                 Debug.Log("[WaveManager] 次のWaveがありません");
//             }
//         }
        
//         /// <summary>
//         /// 現在のWave情報を取得（UI表示用）
//         /// </summary>
//         public string GetCurrentWaveInfo()
//         {
//             if (currentWave == null) return "Wave なし";
            
//             return $"{currentWave.waveName} (Level {currentWave.waveLevel}) - {ElapsedTime:F0}s";
//         }
//     }
// }