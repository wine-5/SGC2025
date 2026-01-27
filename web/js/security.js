/**
 * セキュリティ管理
 * 開発者ツール・右クリック制禁・その他セキュリティ設定の制御
 */

/**
 * セキュリティ設定
 * DEBUG_MODE = trueの場合、全ての制限が無効になる
 */
const SECURITY_CONFIG = {
    DEBUG_MODE: false,                    // デバッグモード（有効化で全制限が無効）
    DISABLE_DEV_TOOLS: true,              // 開発者ツール禁止
    DISABLE_RIGHT_CLICK: true,            // 右クリック禁止
    DISABLE_F12: true,                    // F12キー禁止
    DISABLE_CONSOLE_SHORTCUTS: true,      // Ctrl+Shift+I/J/C禁止
    WARN_ON_DEV_TOOLS_OPEN: false        // 開発者ツール開放時に警告表示
};

/**
 * セキュリティマネージャークラス
 */
class SecurityManager {
    constructor(config = {}) {
        this.config = { ...SECURITY_CONFIG, ...config };
        this.devToolsDetected = false;
        this.isInitialized = false;
    }

    /**
     * セキュリティの初期化
     */
    init() {
        if (this.isInitialized) return;
        
        if (!this.config.DEBUG_MODE) {
            this._setupEventListeners();
            this._detectDevTools();
        }
        
        this.isInitialized = true;
    }

    /**
     * イベントリスナーの設定
     */
    _setupEventListeners() {
        // キーボードイベント
        document.addEventListener('keydown', (e) => this._handleKeyDown(e));
        
        // 右クリック禁止
        if (this.config.DISABLE_RIGHT_CLICK) {
            document.addEventListener('contextmenu', (e) => e.preventDefault());
        }
        
        // 長押し禁止（開発者ツール開放対策）
        document.addEventListener('touchstart', (e) => {
            if (e.touches.length > 1) {
                e.preventDefault();
            }
        }, { passive: false });
    }

    /**
     * キーボード入力の処理
     */
    _handleKeyDown(event) {
        const { key, ctrlKey, shiftKey } = event;

        // F12禁止
        if (this.config.DISABLE_F12 && key === 'F12') {
            event.preventDefault();
            this._logSecurityEvent('F12キーが押されました');
            return;
        }

        // 開発者ツールショートカット禁止
        if (this.config.DISABLE_CONSOLE_SHORTCUTS) {
            // Ctrl+Shift+I (DevTools)
            if (ctrlKey && shiftKey && key === 'I') {
                event.preventDefault();
                this._logSecurityEvent('Ctrl+Shift+Iが押されました');
                return;
            }

            // Ctrl+Shift+J (Console)
            if (ctrlKey && shiftKey && key === 'J') {
                event.preventDefault();
                this._logSecurityEvent('Ctrl+Shift+Jが押されました');
                return;
            }

            // Ctrl+Shift+C (Element Inspector)
            if (ctrlKey && shiftKey && key === 'C') {
                event.preventDefault();
                this._logSecurityEvent('Ctrl+Shift+Cが押されました');
                return;
            }
        }
    }

    /**
     * 開発者ツールの検出
     */
    _detectDevTools() {
        if (!this.config.WARN_ON_DEV_TOOLS_OPEN) return;

        const checkDevTools = () => {
            const start = performance.now();
            debugger;
            const end = performance.now();

            // debuggerが実行されるのに時間がかかる場合は開発者ツールが開いている可能性
            if (end - start > 100) {
                this.devToolsDetected = true;
                this._logSecurityEvent('開発者ツールが開放されています', 'warn');
            }
        };

        // 定期的にチェック
        setInterval(checkDevTools, 1000);
    }

    /**
     * セキュリティイベントのログ出力
     */
    _logSecurityEvent(message, level = 'info') {
        if (this.config.DEBUG_MODE) {
            console[level](`[Security] ${message}`);
        }
    }

    /**
     * 設定を更新（デバッグモードの切り替えなど）
     */
    updateConfig(newConfig) {
        this.config = { ...this.config, ...newConfig };
        
        if (this.config.DEBUG_MODE) {
            console.log('[Security] デバッグモード：有効 - 全ての制限が無効化されました');
        } else {
            console.log('[Security] デバッグモード：無効 - 全てのセキュリティ制限が有効です');
        }
    }

    /**
     * デバッグモードの切り替え
     */
    toggleDebugMode() {
        this.config.DEBUG_MODE = !this.config.DEBUG_MODE;
        return this.config.DEBUG_MODE;
    }

    /**
     * 現在の設定を取得
     */
    getConfig() {
        return { ...this.config };
    }

    /**
     * セキュリティ状態をリセット
     */
    reset() {
        this.config = { ...SECURITY_CONFIG };
        this.devToolsDetected = false;
    }
}

// グローバルインスタンスの作成
const securityManager = new SecurityManager();

// グローバルスコープに公開（デバッグ時のアクセス用）
window.securityManager = securityManager;

// ページロード時に初期化
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        securityManager.init();
    });
} else {
    securityManager.init();
}
