import { Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "@/components/layout/layout";
import { DigestPage } from "./routes/digest";
import { GitHubPage } from "./routes/github";
import { HomePage } from "./routes/home";
import { SectionPage } from "./routes/section";

export function AppRouter() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/ban-tin" element={<DigestPage />} />
        <Route path="/ban-tin/:date" element={<DigestPage />} />
        <Route path="/github" element={<GitHubPage />} />
        <Route path="/muc/:slug" element={<SectionPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
