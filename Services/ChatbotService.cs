namespace CourtBookingAPI.Services
{
    public class ChatbotService
    {
        // Danh sách các từ khóa vi phạm (giả lập AI phân tích ngôn ngữ tự nhiên)
        private readonly List<string> _badWords = new List<string> {"má mày", "thằng điên", "dit bo may", "khùng"};

        public bool AnalyzeReview(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return false;
            
            var lowerComment = comment.ToLower();
            // Nếu có từ ngữ thô tục hoặc spam -> Đánh dấu (Flag)
            return _badWords.Any(word => lowerComment.Contains(word));
        }
    }
}
