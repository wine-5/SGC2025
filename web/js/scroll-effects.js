/**
 * スクロールエフェクト
 * スクロールに連動した動的な効果を実装
 */

// スクロール設定
const SCROLL_CONFIG = {
    ACTIVE_THRESHOLD: 100,  // スクロール検知の閾値（ピクセル）
    SCROLL_END_DELAY: 150   // スクロール終了判定の遅延（ミリ秒）
};

// 状態管理
const scrollState = {
    lastScrollY: 0,
    ticking: false,
    isScrolling: false,
    scrollTimeout: null,
    resizeTimeout: null
};

/**
 * 現在のスクロール位置を取得
 */
function getScrollPosition() {
    return window.pageYOffset || document.documentElement.scrollTop;
}

/**
 * スクロール方向を判定
 */
function getScrollDirection(currentScrollY) {
    if (currentScrollY > scrollState.lastScrollY) {
        return 'down';
    } else if (currentScrollY < scrollState.lastScrollY) {
        return 'up';
    }
    return 'none';
}

/**
 * セクションの可視性を更新
 */
function updateSectionVisibility() {
    const sections = document.querySelectorAll('.section');
    const windowHeight = window.innerHeight;
    const scrollY = getScrollPosition();
    
    sections.forEach(section => {
        const rect = section.getBoundingClientRect();
        const sectionTop = rect.top + scrollY;
        const sectionHeight = rect.height;
        
        // セクションが画面内にあるかチェック
        const isVisible = (
            scrollY + windowHeight > sectionTop + (sectionHeight * 0.2) &&
            scrollY < sectionTop + sectionHeight
        );
        
        if (isVisible) {
            section.classList.add('in-viewport');
        } else {
            section.classList.remove('in-viewport');
        }
    });
}

/**
 * スクロールインジケーターの制御
 */
function updateScrollIndicator() {
    const scrollIndicator = document.querySelector('.scroll-indicator');
    if (!scrollIndicator) return;
    
    const scrollY = getScrollPosition();
    
    // 一定以上スクロールしたらインジケーターを非表示
    if (scrollY > SCROLL_CONFIG.activeThreshold) {
        scrollIndicator.style.opacity = '0';
        scrollIndicator.style.pointerEvents = 'none';
    } else {
        scrollIndicator.style.opacity = '1';
        scrollIndicator.style.pointerEvents = 'auto';
    }
}

/**
 * スクロールイベントの処理
 */
function handleScrollUpdate() {
    const currentScrollY = getScrollPosition();
    const direction = getScrollDirection(currentScrollY);
    
    // スクロール方向に応じた処理
    if (direction !== 'none') {
        document.body.setAttribute('data-scroll-direction', direction);
    }
    
    // 各種更新処理
    updateSectionVisibility();
    updateScrollIndicator();
    
    // 最後のスクロール位置を保存
    scrollState.lastScrollY = currentScrollY;
    scrollState.ticking = false;
}

/**
 * スクロール中の状態を管理
 */
function handleScrollStart() {
    scrollState.isScrolling = true;
    document.body.classList.add('is-scrolling');
    
    // 既存のタイムアウトをクリア
    if (scrollState.scrollTimeout) {
        clearTimeout(scrollState.scrollTimeout);
    }
}

/**
 * スクロール終了の検知
 */
function handleScrollEnd() {
    scrollState.scrollTimeout = setTimeout(() => {
        scrollState.isScrolling = false;
        document.body.classList.remove('is-scrolling');
    }, 150);
}

/**
 * スロットル付きスクロールハンドラー
 */
function throttledScrollHandler() {
    handleScrollStart();
    
    if (!scrollState.ticking) {
        window.requestAnimationFrame(() => {
            handleScrollUpdate();
        });
        scrollState.ticking = true;
    }
    
    handleScrollEnd();
}

/**
 * デバウンス付きリサイズハンドラー
 */
function throttledResizeHandler() {
    if (scrollState.resizeTimeout) {
        clearTimeout(scrollState.resizeTimeout);
    }
    
    scrollState.resizeTimeout = setTimeout(() => {
        updateSectionVisibility();
    }, 200);
}

/**
 * スクロールエフェクトの初期化
 */
function initScrollEffects() {
    try {
        // 初期状態の設定
        scrollState.lastScrollY = getScrollPosition();
        
        // 初期表示の更新
        handleScrollUpdate();
        
        // スクロールイベントのリスナー登録
        window.addEventListener('scroll', throttledScrollHandler, { passive: true });
        
        // リサイズイベントのリスナー登録
        window.addEventListener('resize', throttledResizeHandler, { passive: true });
        
    } catch (error) {
        // エラーが発生しても基本機能は動作するようにする
        console.error('スクロールエフェクトの初期化に失敗しました:', error);
    }
}

/**
 * クリーンアップ処理
 */
function cleanupScrollEffects() {
    window.removeEventListener('scroll', throttledScrollHandler);
    window.removeEventListener('resize', throttledResizeHandler);
    
    if (scrollState.scrollTimeout) {
        clearTimeout(scrollState.scrollTimeout);
    }
    if (scrollState.resizeTimeout) {
        clearTimeout(scrollState.resizeTimeout);
    }
}

// グローバルスコープに関数を公開
window.initScrollEffects = initScrollEffects;
window.cleanupScrollEffects = cleanupScrollEffects;