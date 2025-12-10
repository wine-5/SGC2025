/**
 * メインJavaScript
 * 初期化処理とイベント管理を担当
 */

// 定数定義
const ANIMATION_DELAY_MS = 100;
const SCROLL_THROTTLE_MS = 100;

// DOM要素の参照
const elements = {
    hero: null,
    sections: [],
    animateOnScroll: []
};

/**
 * DOM要素の初期化
 */
function initializeElements() {
    elements.hero = document.getElementById('hero');
    elements.sections = Array.from(document.querySelectorAll('.section'));
    elements.animateOnScroll = Array.from(document.querySelectorAll('.animate-on-scroll'));
}

/**
 * ページロード時のアニメーション
 */
function initPageAnimation() {
    const heroContent = document.querySelector('.hero-content');
    
    if (heroContent) {
        // ヒーローコンテンツをフェードイン
        setTimeout(() => {
            heroContent.style.opacity = '1';
            heroContent.style.transform = 'translateY(0)';
        }, ANIMATION_DELAY_MS);
    }
}

/**
 * スムーススクロールの実装
 */
function initSmoothScroll() {
    const links = document.querySelectorAll('a[href^="#"]');
    
    links.forEach(link => {
        link.addEventListener('click', (event) => {
            const targetId = link.getAttribute('href');
            
            // ハッシュのみの場合はスキップ
            if (targetId === '#') {
                return;
            }
            
            const targetElement = document.querySelector(targetId);
            
            if (targetElement) {
                event.preventDefault();
                targetElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}

/**
 * デバウンス関数
 * 指定されたミリ秒間、関数の実行を遅延させる
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * ページのトップに戻る機能の初期化
 */
function initScrollToTop() {
    // ページトップへのスクロールボタンを追加する場合はここに実装
    // 現在の仕様では不要だが、将来的な拡張のために残しておく
}

/**
 * エラーハンドリング
 */
function handleError(error, context) {
    // 本番環境ではエラーログを送信するなどの処理を行う
    // 開発環境ではコンソールに出力
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
        console.error(`エラーが発生しました [${context}]:`, error);
    }
}

/**
 * アプリケーションの初期化
 */
function initApp() {
    try {
        // DOM要素の初期化
        initializeElements();
        
        // ページアニメーションの初期化
        initPageAnimation();
        
        // スムーススクロールの初期化
        initSmoothScroll();
        
        // スクロールエフェクトの初期化（scroll-effects.jsで定義）
        if (typeof initScrollEffects === 'function') {
            initScrollEffects();
        }
        
        // アニメーションコントローラーの初期化（animation-controller.jsで定義）
        if (typeof initAnimationController === 'function') {
            initAnimationController();
        }
        
        // スクロールトップ機能の初期化
        initScrollToTop();
        
    } catch (error) {
        handleError(error, 'アプリケーション初期化');
    }
}

// DOMContentLoadedイベントでアプリケーションを初期化
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initApp);
} else {
    // DOMが既に読み込まれている場合は即座に実行
    initApp();
}

// ページ離脱時のクリーンアップ
window.addEventListener('beforeunload', () => {
    // 必要に応じてクリーンアップ処理を追加
});