namespace Newsdesk.Infrastructure.Ai
{
    /// <summary>Cấu hình AI tóm tắt, đọc từ mục "Ai" trong configuration (không có bí mật, chỉ địa chỉ và tên model).</summary>
    public sealed class AiOptions
    {
        /// <summary>"Ollama" (model chạy trên máy, mặc định) hoặc "None" (tắt AI: bản tin vẫn tạo, chỉ thiếu phần tóm tắt).</summary>
        public string Provider { get; set; } = "Ollama";

        /// <summary>Ngôn ngữ của bản tóm tắt: "en" hoặc "vi". Model nhỏ viết tiếng Anh tốt hơn tiếng Việt.</summary>
        public string Language { get; set; } = "en";

        public string OllamaBaseUrl { get; set; } = "http://localhost:11434";

        /// <summary>Tên model trong thư viện Ollama. Card 4GB VRAM nên dùng cỡ ~3B (ví dụ llama3.2:3b); 7-8B sẽ tràn bộ nhớ và rất chậm.</summary>
        public string OllamaModel { get; set; } = "llama3.2:3b";

        /// <summary>Thời gian chờ tối đa cho MỘT bài (lần đầu có thể lâu vì phải nạp model vào bộ nhớ).</summary>
        public int TimeoutSeconds { get; set; } = 180;
    }
}
