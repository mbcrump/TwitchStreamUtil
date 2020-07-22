using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchStreamUtil.Helper
{
    public class StreamUtilEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string TitleName { get; set; }
        public bool IsCompleted { get; set; }
    }
}
