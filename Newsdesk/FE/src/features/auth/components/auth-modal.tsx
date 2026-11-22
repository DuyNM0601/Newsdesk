import { createContext, useCallback, useContext, useState, type ReactNode } from "react";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { LoginForm } from "./login-form";

const AuthModalContext = createContext<{ open: () => void } | null>(null);

export function AuthModalProvider({ children }: { children: ReactNode }) {
  const [isOpen, setIsOpen] = useState(false);
  const [session, setSession] = useState(0);

  const open = useCallback(() => {
    setSession((s) => s + 1); // form mới mỗi lần mở → xoá thông báo của lần trước
    setIsOpen(true);
  }, []);

  return (
    <AuthModalContext.Provider value={{ open }}>
      {children}
      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent className="max-w-sm gap-0 overflow-hidden rounded-md p-0">
          <div className="h-1 bg-rule" aria-hidden="true" />
          <div className="px-8 pb-8 pt-6">
            <DialogHeader className="gap-1 text-left">
              <DialogTitle className="headline text-2xl font-bold">Đăng nhập</DialogTitle>
              <DialogDescription>Đăng nhập bằng Google. Bạn vẫn đọc tin được mà không cần đăng nhập.</DialogDescription>
            </DialogHeader>
            <LoginForm key={session} onSuccess={() => setIsOpen(false)} />
          </div>
        </DialogContent>
      </Dialog>
    </AuthModalContext.Provider>
  );
}

export function useAuthModal() {
  const ctx = useContext(AuthModalContext);
  if (!ctx) throw new Error("useAuthModal phải dùng trong AuthModalProvider");
  return ctx;
}
