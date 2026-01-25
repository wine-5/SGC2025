/**
 * メインJavaScript
 * 初期化処理とイベント管理を担当
 */

const ANIMATION_DELAY_MS = 100;

/**
 * ページロード時のアニメーション
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
 * スムーススクロールの実装
 */
function initSmoothScroll() {
    const links = document.querySelectorAll('a[href^="#"]');
    
    links.forEach(link => {
        link.addEventListener('click', (event) => {
            const targetId = link.getAttribute('href');
            
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
 * アプリケーションの初期化
 */
function initApp() {
    initPageAnimation();
    initSmoothScroll();
    
    if (typeof initScrollEffects === 'function') {
        initScrollEffects();
    }
}

// DOMContentLoadedイベントでアプリケーションを初期化
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initApp);
} else {
    initApp();
}