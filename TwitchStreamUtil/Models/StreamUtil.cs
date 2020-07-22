using System;

namespace TwitchStreamUtil.Models
{
    public class Title
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string TitleName { get; set; }
        public bool IsCompleted { get; set; }
    }
}
