import { Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "@/components/layout/layout";
import { HomePage } from "./routes/home";
import { SectionPage } from "./routes/section";

export function AppRouter() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/muc/:slug" element={<SectionPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
