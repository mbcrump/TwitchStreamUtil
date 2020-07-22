using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TwitchStreamUtil.Models;
using System.IO;
using Newtonsoft.Json;
using TwitchStreamUtil.Helper;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Linq;

namespace TwitchStreamUtil
{
    public static class TodoApiTableStorage1
    {
        private const string Route = "title";
        private const string TableName = "Streaminfo";

        [FunctionName("Table_CreateTitle")]
        public static async Task<IActionResult> CreateTitle(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req,
            [Table(TableName, Connection = "TableConnectionString")] IAsyncCollector<StreamUtilEntity> todoTable,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TitleCreateModel>(requestBody);

            var todo = new Title() { TitleName = input.TitleName };
            await todoTable.AddAsync(todo.ToTableEntity());
            return new OkObjectResult(todo);
        }

        [FunctionName("Table_GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequest req,
            // unfortunately IQueryable<StreamUtilEntity> binding not supported in functions v2
            [Table(TableName, Connection = "TableConnectionString")] CloudTable todoTable,
            ILogger log)
        {
            log.LogInformation("Getting todo list items");
            var query = new TableQuery<StreamUtilEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToTodo));
        }

        [FunctionName("Table_GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequest req,
            [Table(TableName, "TODO", "{id}", Connection = "TableConnectionString")] StreamUtilEntity todo,
            ILogger log, string id)
        {
            log.LogInformation("Getting todo item by id");
            if (todo == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(todo.ToTodo());
        }

        [FunctionName("Table_UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")] HttpRequest req,
            [Table(TableName, Connection = "TableConnectionString")] CloudTable todoTable,
            ILogger log, string id)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TitleUpdateModel>(requestBody);
            var findOperation = TableOperation.Retrieve<StreamUtilEntity>("TODO", id);
            var findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }
            var existingRow = (StreamUtilEntity)findResult.Result;

            existingRow.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TitleName))
            {
                existingRow.TitleName = updated.TitleName;
            }

            var replaceOperation = TableOperation.Replace(existingRow);
            await todoTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.ToTodo());
        }

        [FunctionName("Table_DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequest req,
            [Table(TableName, Connection = "TableConnectionString")] CloudTable todoTable,
            ILogger log, string id)
        {
            var deleteEntity = new TableEntity { PartitionKey = "TODO", RowKey = id, ETag = "*" };
            var deleteOperation = TableOperation.Delete(deleteEntity);
            try
            {
                await todoTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }
            return new OkResult();
        }
    }

}
