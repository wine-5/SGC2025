/**
 * メインJavaScript
 * ページ初期化処理を担当
 */

const ANIMATION_DELAY_MS = 100; // ページロードアニメーション遅延時間（ミリ秒）

/**
 * ページロード時のアニメーション初期化
 */
function initPageAnimation() {
    const heroContent = document.querySelector('.hero-content');
    
    if (heroContent) {
        setTimeout(() => {
            heroContent.style.opacity = '1';
            heroContent.style.transform = 'translateY(0)';
        }, ANIMATION_DELAY_MS);
    }
}

/**
 * アプリケーションの初期化
 */
function initApp() {
    initPageAnimation();
    
    // スクロールエフェクト初期化（scroll-effects.jsから提供）
    if (typeof initScrollEffects === 'function') {
        initScrollEffects();
    }
}

// DOMContentLoadedイベント、または既にロード済みの場合に初期化実行
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initApp);
} else {
    initApp();
}