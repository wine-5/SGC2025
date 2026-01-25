// 現在の言語を管理
let currentLanguage = localStorage.getItem('language') || 'ja';

// JSON設定データを読み込む
let contentData = {};

// JSONからコンテンツを読み込む
async function loadContent() {
  try {
    const response = await fetch('web/data/content.json');
    contentData = await response.json();
    updateUI();
  } catch (error) {
    console.error('JSONの読み込みに失敗しました:', error);
  }
}

// データ属性を持つ要素を更新する関数
function updateUI() {
  document.querySelectorAll('[data-i18n]').forEach((element) => {
    const keys = element.getAttribute('data-i18n').split('.');
    let value = contentData[currentLanguage];
    
    for (const key of keys) {
      if (value && typeof value === 'object') {
        value = value[key];
      } else {
        value = null;
        break;
      }
    }
    
    if (value) {
      if (element.tagName === 'A') {
        // リンク要素は通常のテキスト更新
        element.textContent = value;
      } else if (element.tagName === 'INPUT' || element.tagName === 'BUTTON') {
        // フォーム要素
        element.value = value;
        element.textContent = value;
      } else {
        // その他の要素
        element.textContent = value;
      }
    }
  });

  // 言語表示を更新
  const langIndicator = document.querySelector('.lang-indicator');
  if (langIndicator) {
    langIndicator.textContent = currentLanguage === 'ja' ? '日本語' : 'English';
  }

  // HTMLの言語属性を更新
  document.documentElement.lang = currentLanguage;

  // ナビゲーション言語切り替え
  updateNavigation();
}

// ナビゲーションメニューの言語を更新
function updateNavigation() {
  // PC用ナビゲーション
  document.querySelectorAll('.nav-link').forEach((link) => {
    const section = link.getAttribute('data-section');
    if (section && contentData[currentLanguage] && contentData[currentLanguage].navigation) {
      const navText = contentData[currentLanguage].navigation[section];
      if (navText) {
        link.textContent = navText;
      }
    }
  });

  // モバイル用ナビゲーション
  const mobileNavLinks = document.querySelectorAll('.mobile-nav-link');
  const sections = ['home', 'sapporo_camp', 'download', 'game_overview', 'controls', 'developers'];
  mobileNavLinks.forEach((link, index) => {
    const sectionKey = sections[index];
    if (contentData[currentLanguage] && contentData[currentLanguage].navigation && contentData[currentLanguage].navigation[sectionKey]) {
      link.textContent = contentData[currentLanguage].navigation[sectionKey];
    }
  });
}

// 言語切り替えボタンの処理
document.addEventListener('DOMContentLoaded', () => {
  loadContent();

  const languageToggle = document.getElementById('language-toggle');
  if (languageToggle) {
    languageToggle.addEventListener('click', () => {
      currentLanguage = currentLanguage === 'ja' ? 'en' : 'ja';
      localStorage.setItem('language', currentLanguage);
      updateUI();
    });
  }
});
