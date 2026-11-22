import { NavLink } from "react-router-dom";
import { BriefcaseIcon, ListChecksIcon, NewspaperIcon, SearchIcon, SparklesIcon } from "lucide-react";
import { cn } from "@/lib/utils";

const items = [
  { to: "/", label: "Trang nhất", icon: NewspaperIcon, end: true },
  { to: "/ban-tin", label: "Bản tin", icon: ListChecksIcon, end: false },
  { to: "/muc/ai", label: "AI", icon: SparklesIcon, end: false },
  { to: "/muc/nghe-dev", label: "Nghề dev", icon: BriefcaseIcon, end: false },
  { to: "/muc/tat-ca", label: "Tìm", icon: SearchIcon, end: false },
];

// Thanh dưới chỉ có trên điện thoại (dưới md): bốn việc hay làm nhất, đủ lớn cho ngón cái.
export function BottomNav() {
  return (
    <nav aria-label="Lối tắt" className="fixed inset-x-0 bottom-0 z-30 border-t border-border bg-background/95 pb-[env(safe-area-inset-bottom)] backdrop-blur-sm md:hidden">
      <ul className="mx-auto flex max-w-md">
        {items.map(({ to, label, icon: Icon, end }) => (
          <li key={to} className="flex-1">
            <NavLink
              to={to}
              end={end}
              className={({ isActive }) =>
                cn(
                  "flex min-h-14 flex-col items-center justify-center gap-0.5 text-xs font-semibold transition-colors duration-150 ease-(--ease-out)",
                  isActive ? "text-foreground" : "text-muted-foreground",
                )
              }
            >
              <Icon className="size-5" aria-hidden="true" />
              {label}
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
}
