import { useEffect } from "react";
import { Outlet, useLocation } from "react-router-dom";
import { BottomNav } from "./bottom-nav";
import { SiteFooter } from "./site-footer";
import { Masthead, SectionNav } from "./site-header";

// Chiều cao thanh chuyên mục dính (min-h-12 = 48px).
const STICKY_NAV_HEIGHT = 48;

export function Layout() {
  const { pathname } = useLocation();

  // Đổi trang: chỉ cuộn khi người đọc đang ở sâu hơn đầu nội dung, và dừng NGAY DƯỚI thanh chuyên mục dính thay vì
  // nhảy lên đầu trang (bỏ cả tên báo). Đang ở đầu trang thì giữ nguyên, không giật.
  useEffect(() => {
    const main = document.getElementById("main");
    if (!main) return;
    const top = main.getBoundingClientRect().top + window.scrollY - STICKY_NAV_HEIGHT;
    if (window.scrollY > top) window.scrollTo({ top: Math.max(top, 0), behavior: "auto" });
  }, [pathname]);

  return (
    <div className="flex min-h-screen flex-col">
      <a href="#main" className="sr-only focus:not-sr-only focus:fixed focus:left-3 focus:top-3 focus:z-50 focus:rounded focus:bg-foreground focus:px-4 focus:py-2 focus:text-background">
        Bỏ qua, tới nội dung chính
      </a>
      <Masthead />
      <SectionNav />
      {/* pb-16: chừa chỗ cho BottomNav (chỉ có trên điện thoại) để không che cuối trang. */}
      <main id="main" className="flex flex-1 flex-col pb-16 md:pb-0">
        <Outlet />
      </main>
      <SiteFooter />
      <BottomNav />
    </div>
  );
}
