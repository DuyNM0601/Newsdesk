import { useCallback, useEffect, useLayoutEffect, useRef, useState, type FormEvent } from "react";
import { Link, NavLink, useLocation, useNavigate } from "react-router-dom";
import { SearchIcon } from "lucide-react";
import { SignOutButton, useAuth, useAuthModal } from "@/features/auth";
import { prefetchArticles } from "@/features/news/api/news";
import { sectionQuery, sections } from "@/features/news/lib/sections";
import { ThemeToggle } from "@/components/theme-toggle";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

/** Đầu trang kiểu báo: tên báo chữ có chân, vạch mực đậm bên dưới, tìm kiếm + đăng nhập bên phải. */
export function Masthead() {
  const { open } = useAuthModal();
  const { session } = useAuth();
  const navigate = useNavigate();
  const [q, setQ] = useState("");

  function search(e: FormEvent) {
    e.preventDefault();
    if (q.trim()) navigate(`/muc/tat-ca?q=${encodeURIComponent(q.trim())}`);
  }

  return (
    <header className="border-b-4 border-rule">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 px-4 py-4 sm:px-6 sm:py-5">
        <Link to="/" className="headline text-3xl font-bold leading-none tracking-tight sm:text-[2.5rem]">
          Newsdesk
        </Link>
        <div className="flex items-center gap-2 sm:gap-3">
          <form onSubmit={search} role="search" className="relative hidden md:block">
            <label htmlFor="site-search" className="sr-only">
              Tìm tin theo tiêu đề
            </label>
            <SearchIcon className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
            <input
              id="site-search"
              type="search"
              value={q}
              onChange={(e) => setQ(e.target.value)}
              placeholder="Tìm tin..."
              className="h-11 w-52 rounded-md border border-input bg-card pl-9 pr-3 text-sm placeholder:text-muted-foreground"
            />
          </form>
          {session ? (
            <>
              <span className="hidden max-w-40 truncate text-sm font-semibold sm:inline">{session.displayName}</span>
              <SignOutButton />
            </>
          ) : (
            <Button type="button" onClick={open} className="h-11 px-5 text-sm font-semibold">
              Đăng nhập
            </Button>
          )}
          <ThemeToggle />
        </div>
      </div>
    </header>
  );
}

const navLink =
  "relative flex min-h-12 shrink-0 items-center px-3 text-[15px] font-semibold text-muted-foreground transition-colors duration-150 ease-(--ease-out) hover:text-foreground aria-[current=page]:text-foreground";

/**
 * Thanh chuyên mục dính khi cuộn; trên điện thoại cuộn ngang trong chính nó, trang không cuộn ngang.
 * Gạch chân active là MỘT phần tử trượt giữa các tab (chỉ transform, ~260ms), không phải mỗi tab một viền nhảy tức thì.
 * Rê chuột / chạm / focus vào tab thì tải sẵn danh sách của mục đó để bấm xong là có bài.
 */
export function SectionNav() {
  const { pathname } = useLocation();
  const trackRef = useRef<HTMLDivElement>(null);
  const [bar, setBar] = useState<{ x: number; w: number } | null>(null);
  const [instant, setInstant] = useState(true); // lần đo đầu: đặt thẳng vào chỗ, không trượt từ góc trái

  const measure = useCallback(() => {
    const active = trackRef.current?.querySelector<HTMLElement>('[aria-current="page"]');
    setBar(active ? { x: active.offsetLeft, w: active.offsetWidth } : null);
    return active;
  }, []);

  // Đổi trang: đo lại, và đưa tab active vào giữa khung (điện thoại cuộn ngang) bằng cuộn mượt.
  useLayoutEffect(() => {
    const active = measure();
    if (!active) return;
    const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    active.scrollIntoView({ inline: "center", block: "nearest", behavior: instant || reduce ? "auto" : "smooth" });
  }, [pathname, measure, instant]);

  useEffect(() => {
    const id = requestAnimationFrame(() => setInstant(false));
    // Font nạp xong hoặc đổi cỡ cửa sổ làm tab đổi bề rộng: đo lại, không trượt.
    const ro = new ResizeObserver(() => measure());
    if (trackRef.current) ro.observe(trackRef.current);
    document.fonts?.ready.then(measure);
    return () => {
      cancelAnimationFrame(id);
      ro.disconnect();
    };
  }, [measure]);

  return (
    <nav aria-label="Chuyên mục" className="sticky top-0 z-20 border-b border-border bg-background/95 backdrop-blur-sm">
      <div className="mx-auto w-full max-w-6xl overflow-x-auto px-2 sm:px-4 [scrollbar-width:none]">
        <div ref={trackRef} className="relative flex w-max min-w-full gap-1">
          <NavLink to="/" end className={navLink}>
            Trang nhất
          </NavLink>
          {sections.map((s) => {
            const warm = () => prefetchArticles({ ...sectionQuery(s), page: 1 });
            return (
              <NavLink key={s.slug} to={`/muc/${s.slug}`} className={navLink} onPointerEnter={warm} onFocus={warm} onTouchStart={warm}>
                {s.label}
              </NavLink>
            );
          })}
          {bar && (
            <span
              aria-hidden="true"
              data-instant={instant || undefined}
              className={cn("nav-bar pointer-events-none absolute bottom-0 left-0 h-[3px] w-px origin-left bg-foreground")}
              style={{ transform: `translateX(${bar.x}px) scaleX(${bar.w})` }}
            />
          )}
        </div>
      </div>
    </nav>
  );
}
