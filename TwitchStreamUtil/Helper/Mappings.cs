using System;
using System.Collections.Generic;
using System.Text;
using TwitchStreamUtil.EntityFramework;
using TwitchStreamUtil.Models;

namespace TwitchStreamUtil.Helper
{
    public static class Mappings
    {
        public static StreamUtilEntity ToTableEntity(this Title todo)
        {
            return new StreamUtilEntity()
            {
                PartitionKey = "TODO",
                RowKey = todo.Id,
                CreatedTime = todo.CreatedTime,
                IsCompleted = todo.IsCompleted,
                TitleName = todo.TitleName
            };
        }

        public static Title ToTodo(this StreamUtilEntity todo)
        {
            return new Title()
            {
                Id = todo.RowKey,
                CreatedTime = todo.CreatedTime,
                IsCompleted = todo.IsCompleted,
                TitleName = todo.TitleName
            };
        }

    }
}
