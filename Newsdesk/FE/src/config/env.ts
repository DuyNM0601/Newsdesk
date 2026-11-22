export const apiBaseUrl = import.meta.env.VITE_API_URL ?? "https://localhost:7022";

// Chỉ true khi chạy `npm run dev`; dùng để hiện công cụ thử nghiệm (ví dụ nút gọi thử) không có ở bản build thật.
export const isDev: boolean = import.meta.env.DEV;

export const googleClientId: string = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? "";
