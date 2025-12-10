/**
 * スクロールエフェクト
 * スクロールに連動した動的な効果を実装
 */

// スクロール設定
const SCROLL_CONFIG = {
    throttleDelay: 16,         // 約60フレーム・パー・セカンド
    activeThreshold: 100,      // スクロール検知の闾値（ピクセル）
    headerTransitionPoint: 300 // ヘッダーの背景変更ポイント（ピクセル）
};

// 状態管理
const scrollState = {
    lastScrollY: 0,
    ticking: false,
    isScrolling: false,
    scrollTimeout: null
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
 * スクロール進捗率を計算
 */
function calculateScrollProgress() {
    const windowHeight = window.innerHeight;
    const documentHeight = document.documentElement.scrollHeight;
    const scrollTop = getScrollPosition();
    
    // 0から1の範囲で進捗を返す
    return scrollTop / (documentHeight - windowHeight);
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
 * 背景のパララックス効果
 */
function updateBackgroundParallax() {
    const scrollY = getScrollPosition();
    const sections = document.querySelectorAll('.section');
    
    sections.forEach(section => {
        const speed = 0.5;
        const yPos = -(scrollY * speed);
        
        // セクションの背景に対してパララックス効果を適用
        const sectionRect = section.getBoundingClientRect();
        if (sectionRect.top < window.innerHeight && sectionRect.bottom > 0) {
            section.style.backgroundPosition = `center ${yPos}px`;
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
    updateBackgroundParallax();
    
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
 * リサイズイベントの処理
 */
function handleResize() {
    // リサイズ時に必要な再計算
    updateSectionVisibility();
}

/**
 * デバウンス付きリサイズハンドラー
 */
let resizeTimeout;
function throttledResizeHandler() {
    if (resizeTimeout) {
        clearTimeout(resizeTimeout);
    }
    
    resizeTimeout = setTimeout(() => {
        handleResize();
    }, 200);
}

/**
 * キーボードによるナビゲーション
 */
function initKeyboardNavigation() {
    document.addEventListener('keydown', (event) => {
        // スペースキーでスクロール
        if (event.code === 'Space' && event.target === document.body) {
            event.preventDefault();
            const windowHeight = window.innerHeight;
            window.scrollBy({
                top: windowHeight * 0.8,
                behavior: 'smooth'
            });
        }
        
        // Homeキーでページトップへ移動
        if (event.code === 'Home') {
            event.preventDefault();
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        }
        
        // Endキーでページ最下部へ移動
        if (event.code === 'End') {
            event.preventDefault();
            window.scrollTo({
                top: document.documentElement.scrollHeight,
                behavior: 'smooth'
            });
        }
        
        // スペースキーで弾発射（ゲーム機能）
        if (event.code === 'Space') {
            event.preventDefault();
            shootBullet();
        }
    });
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
        
        // キーボードナビゲーションの初期化
        initKeyboardNavigation();
        
    } catch (error) {
        // エラーが発生しても基本機能は動作するようにする
        console.error('スクロールエフェクトの初期化に失敗しました:', error);
    }
}

/**
 * 弾発射システム
 */
function shootBullet() {
    // 弾のエレメントを作成
    const bullet = document.createElement('div');
    bullet.className = 'bullet';
    bullet.innerHTML = '💫'; // 弾のビジュアル（蝶の魔法弾をイメージ）
    
    // 弾の初期位置設定（画面中央下部から発射）
    const startX = window.innerWidth / 2;
    const startY = window.innerHeight - 100;
    
    bullet.style.cssText = `
        position: fixed;
        left: ${startX}px;
        top: ${startY}px;
        font-size: 24px;
        z-index: 1000;
        pointer-events: none;
        transition: transform 0.8s ease-out;
        text-shadow: 0 0 10px rgba(255, 215, 0, 0.8);
    `;
    
    document.body.appendChild(bullet);
    
    // 弾の移動アニメーション
    requestAnimationFrame(() => {
        bullet.style.transform = `translateY(-${window.innerHeight + 200}px) rotate(360deg)`;
    });
    
    // 敵（人間）との当たり判定をチェック
    checkBulletCollision(bullet);
    
    // 弾を0.8秒後に削除
    setTimeout(() => {
        if (bullet.parentNode) {
            bullet.parentNode.removeChild(bullet);
        }
    }, 800);
    
    // 発射エフェクト
    createShootEffect(startX, startY);
}

/**
 * 弾と敵の当たり判定
 */
function checkBulletCollision(bullet) {
    const enemies = document.querySelectorAll('.human-enemy');
    
    const checkCollision = () => {
        const bulletRect = bullet.getBoundingClientRect();
        
        enemies.forEach(enemy => {
            if (!enemy.classList.contains('defeated')) {
                const enemyRect = enemy.getBoundingClientRect();
                
                // 当たり判定（簡易版）
                if (bulletRect.left < enemyRect.right &&
                    bulletRect.right > enemyRect.left &&
                    bulletRect.top < enemyRect.bottom &&
                    bulletRect.bottom > enemyRect.top) {
                    
                    // 敵を倒す
                    defeatEnemy(enemy);
                    
                    // 弾を消去
                    if (bullet.parentNode) {
                        bullet.parentNode.removeChild(bullet);
                    }
                }
            }
        });
        
        // 弾がまだ存在する場合は継続してチェック
        if (bullet.parentNode) {
            requestAnimationFrame(checkCollision);
        }
    };
    
    requestAnimationFrame(checkCollision);
}

/**
 * 敵を倒す処理
 */
function defeatEnemy(enemy) {
    enemy.classList.add('defeated');
    enemy.style.cssText += `
        transform: scale(0.5) rotate(180deg);
        opacity: 0.3;
        filter: grayscale(1);
        transition: all 0.5s ease-out;
    `;
    
    // 撃破エフェクト
    createDefeatEffect(enemy);
    
    // スコア更新（もしスコア要素があれば）
    updateScore();
}

/**
 * 発射エフェクト
 */
function createShootEffect(x, y) {
    const effect = document.createElement('div');
    effect.innerHTML = '✨';
    effect.style.cssText = `
        position: fixed;
        left: ${x}px;
        top: ${y}px;
        font-size: 20px;
        z-index: 999;
        pointer-events: none;
        animation: shootEffect 0.3s ease-out forwards;
    `;
    
    document.body.appendChild(effect);
    
    setTimeout(() => {
        if (effect.parentNode) {
            effect.parentNode.removeChild(effect);
        }
    }, 300);
}

/**
 * 撃破エフェクト
 */
function createDefeatEffect(enemy) {
    const rect = enemy.getBoundingClientRect();
    const effect = document.createElement('div');
    effect.innerHTML = '💥';
    effect.style.cssText = `
        position: fixed;
        left: ${rect.left + rect.width / 2}px;
        top: ${rect.top + rect.height / 2}px;
        font-size: 30px;
        z-index: 1001;
        pointer-events: none;
        animation: defeatEffect 0.6s ease-out forwards;
    `;
    
    document.body.appendChild(effect);
    
    setTimeout(() => {
        if (effect.parentNode) {
            effect.parentNode.removeChild(effect);
        }
    }, 600);
}

/**
 * スコア更新
 */
function updateScore() {
    const scoreElement = document.getElementById('game-score');
    if (scoreElement) {
        const currentScore = parseInt(scoreElement.textContent) || 0;
        scoreElement.textContent = currentScore + 100;
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
}

// グローバルスコープに関数を公開
window.initScrollEffects = initScrollEffects;
window.cleanupScrollEffects = cleanupScrollEffects;