namespace Newsdesk.Application.Common
{
    public interface ICronJob
    {
        /// <summary>Ném exception nếu thất bại: bộ chạy sẽ ghi lỗi vào bảng CronJobs và vẫn chạy tiếp các job khác.</summary>
        Task RunAsync(CancellationToken cancellationToken);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CronScheduleAttribute(string expression) : Attribute
    {
        public string Expression { get; } = expression;
    }
}
