import { request } from "@/lib/api-client";
import type { AuthSession } from "../types";

// idToken do Google Sign-In cấp cho trình duyệt; BE kiểm tra lại rồi đổi lấy JWT của Newsdesk.
export function googleLogin(idToken: string) {
  return request<AuthSession>("/auth/google", {
    method: "POST",
    body: JSON.stringify({ idToken }),
  });
}
