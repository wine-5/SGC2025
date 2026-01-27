/**
 * ナビゲーション機能
 * ハンバーガーメニューとモバイルメニューの制御
 */

document.addEventListener('DOMContentLoaded', () => {
  const hamburger = document.getElementById('hamburger');
  const mobileNav = document.getElementById('mobile-nav');
  const mobileNavLinks = document.querySelectorAll('.mobile-nav-link');

  // ハンバーガーメニューの開閉
  if (hamburger) {
    hamburger.addEventListener('click', () => {
      mobileNav.classList.toggle('active');
      hamburger.classList.toggle('active');
    });
  }

  // モバイル用ナビゲーションリンクのクリック処理
  mobileNavLinks.forEach((link) => {
    link.addEventListener('click', () => {
      // メニューを閉じる
      mobileNav.classList.remove('active');
      if (hamburger) {
        hamburger.classList.remove('active');
      }
    });
  });

  // ウィンドウリサイズでモバイルメニューを閉じる
  window.addEventListener('resize', () => {
    if (window.innerWidth > 768) {
      mobileNav.classList.remove('active');
      if (hamburger) {
        hamburger.classList.remove('active');
      }
    }
  });
});
