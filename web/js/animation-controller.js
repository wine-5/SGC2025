/**
 * アニメーションコントローラー
 * 各要素の出現アニメーションを制御
 */

// アニメーション設定
const ANIMATION_CONFIG = {
    threshold: 0.15,           // 要素が15%表示されたらアニメーション開始
    rootMargin: '0px 0px -50px 0px',  // 下部50ピクセル分早めに検知
    staggerDelay: 150          // スタッガーアニメーションの遅延（ミリ秒）
};

// 監視対象の要素セレクタ
const ANIMATED_SELECTORS = [
    '.section-header',
    '.theme-card',
    '.theme-kanji-reveal',
    '.character-card',
    '.gameplay-card',
    '.key',
    '.download-card',
    '.developer-card'
];

/**
 * Intersection Observerのインスタンス
 */
let observer = null;

/**
 * 要素が画面内に入った時の処理
 */
function handleIntersection(entries) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            // 表示状態のクラスを追加
            entry.target.classList.add('is-visible');
            
            // 一度表示したら監視を解除（再度非表示にしない）
            if (observer) {
                observer.unobserve(entry.target);
            }
        }
    });
}

/**
 * スタッガーアニメーションの適用
 * グループ内の要素に順番にアニメーションを適用
 */
function applyStaggerAnimation(elements) {
    elements.forEach((element, index) => {
        // 遅延時間を計算
        const delay = index * ANIMATION_CONFIG.staggerDelay;
        element.style.transitionDelay = `${delay}ms`;
    });
}

/**
 * 特定のグループに対してスタッガーアニメーションを設定
 */
function initStaggerGroups() {
    // キャラクターカード
    const characterCards = document.querySelectorAll('.character-card');
    if (characterCards.length > 0) {
        applyStaggerAnimation(characterCards);
    }
    
    // ゲームプレイカード
    const gameplayCards = document.querySelectorAll('.gameplay-card');
    if (gameplayCards.length > 0) {
        applyStaggerAnimation(gameplayCards);
    }
    
    // コントロールキー
    const keys = document.querySelectorAll('.key');
    if (keys.length > 0) {
        applyStaggerAnimation(keys);
    }
}

/**
 * テーマセクションの特別なアニメーション
 */
function initThemeAnimation() {
    const themeCard = document.querySelector('.theme-card');
    const kanjiReveal = document.querySelector('.theme-kanji-reveal');
    
    if (themeCard && kanjiReveal) {
        // テーマカードとは別に漢字リビールを監視
        const kanjiObserver = new IntersectionObserver(
            (entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        // 漢字リビールのアニメーション
                        setTimeout(() => {
                            kanjiReveal.classList.add('is-visible');
                        }, 400);
                        kanjiObserver.unobserve(entry.target);
                    }
                });
            },
            {
                threshold: ANIMATION_CONFIG.threshold,
                rootMargin: ANIMATION_CONFIG.rootMargin
            }
        );
        
        kanjiObserver.observe(themeCard);
    }
}

/**
 * ヒーローセクションの初期アニメーション
 */
function initHeroAnimation() {
    const heroContent = document.querySelector('.hero-content');
    
    if (heroContent) {
        // 初期状態を設定
        heroContent.style.opacity = '0';
        heroContent.style.transform = 'translateY(30px)';
        heroContent.style.transition = 'opacity 1s ease, transform 1s ease';
    }
}

/**
 * パララックス効果の初期化
 */
function initParallaxEffect() {
    const butterflyParticles = document.querySelectorAll('.butterfly-particle');
    
    if (butterflyParticles.length === 0) return;
    
    window.addEventListener('scroll', () => {
        const scrollY = window.pageYOffset;
        
        butterflyParticles.forEach((particle, index) => {
            // 各蝶々に異なる速度でパララックス効果を適用
            const speed = 0.3 + (index * 0.1);
            const yPos = -(scrollY * speed);
            particle.style.transform = `translateY(${yPos}px)`;
        });
    }, { passive: true });
}

/**
 * アニメーションコントローラーの初期化
 */
function initAnimationController() {
    try {
        // ヒーローアニメーションの初期化
        initHeroAnimation();
        
        // スタッガーグループの初期化
        initStaggerGroups();
        
        // テーマアニメーションの初期化
        initThemeAnimation();
        
        // パララックス効果の初期化
        initParallaxEffect();
        
        // 交差監視オブザーバーの設定
        const observerOptions = {
            threshold: ANIMATION_CONFIG.threshold,
            rootMargin: ANIMATION_CONFIG.rootMargin
        };
        
        observer = new IntersectionObserver(handleIntersection, observerOptions);
        
        // 監視対象の要素を登録
        ANIMATED_SELECTORS.forEach(selector => {
            const elements = document.querySelectorAll(selector);
            elements.forEach(element => {
                observer.observe(element);
            });
        });
        
    } catch (error) {
        // エラーが発生してもアニメーションなしで表示
        const allAnimatedElements = document.querySelectorAll(ANIMATED_SELECTORS.join(','));
        allAnimatedElements.forEach(element => {
            element.classList.add('is-visible');
        });
    }
}

// グローバルスコープに関数を公開
window.initAnimationController = initAnimationController;