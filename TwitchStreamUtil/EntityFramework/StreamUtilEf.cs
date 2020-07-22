using System;

namespace TwitchStreamUtil.EntityFramework
{
    public class TitleEf
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string TitleName { get; set; }
        public bool IsCompleted { get; set; }
    }
}