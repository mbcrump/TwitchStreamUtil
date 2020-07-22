using System;
using System.IO;
using System.Threading.Tasks;
using TwitchStreamUtil.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitchStreamUtil.EntityFramework;

namespace AzureFunctionsTodo.EntityFramework
{

    public class TodoApiEntityFramework
    {
        private const string Route = "eftitle";
        private readonly TitleContext titleContext;

        public TodoApiEntityFramework(TitleContext titleContext)
        {
            this.titleContext = titleContext;
        }

        [FunctionName("EntityFramework_CreateTodo")]
        public async Task<IActionResult>CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)]HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating a new todo list item");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TitleCreateModel>(requestBody);
            var todo = new TitleEf { TitleName = input.TitleName };
            await this.titleContext.Titles.AddAsync(todo);
            await this.titleContext.SaveChangesAsync();
            return new OkObjectResult(todo);
        }

        [FunctionName("EntityFramework_GetTodos")]
        public async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)]HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting todo list items");
            var todos = await this.titleContext.Titles.ToListAsync();
            return new OkObjectResult(todos);
        }

        [FunctionName("EntityFramework_GetTodoById")]
        public async Task<IActionResult> GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")]HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation("Getting todo item by id");
            var todo = await this.titleContext.Titles.FindAsync(Guid.Parse(id));
            if (todo == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(todo);
        }

        [FunctionName("EntityFramework_UpdateTodo")]
        public async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")]HttpRequest req,
            ILogger log, string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TitleUpdateModel>(requestBody);
            var todo = await this.titleContext.Titles.FindAsync(Guid.Parse(id));
            if (todo == null)
            {
                log.LogWarning($"Item {id} not found");
                return new NotFoundResult();
            }

            todo.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TitleName))
            {
                todo.TitleName = updated.TitleName;
            }

            await this.titleContext.SaveChangesAsync();

            return new OkObjectResult(todo);

        }

        [FunctionName("EntityFramework_DeleteTodo")]
        public async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")]HttpRequest req,
            ILogger log, string id)
        {
            var todo = await this.titleContext.Titles.FindAsync(Guid.Parse(id));
            if (todo == null)
            {
                log.LogWarning($"Item {id} not found");
                return new NotFoundResult();
            }

            this.titleContext.Titles.Remove(todo);
            await this.titleContext.SaveChangesAsync();
            return new OkResult();
        }
    }
}
