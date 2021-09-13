using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web1Api.Models;

namespace Web1Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
#if Test_Logging
        private const int TodoItemsControllerLoggingEventID = 2;
        private readonly ILogger<TodoItemsController> _logger;
#endif
        private readonly DatabaseContext _context;

        public TodoItemsController(DatabaseContext context
                                   #if Test_Logging
                                   ,
                                   ILogger<TodoItemsController> logger
                                   #endif
                                   )
        {
            _context = context;
            #if Test_Logging
            this._logger = logger;
            #endif
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems(string UserName = "No Name")
        {
            #if Test_Logging
            // below code is needed to get User name for Log             
            Serilog.Context.LogContext.PushProperty("UserName", UserName); //Push user in LogContext;  
            this._logger.LogWarning(TodoItemsControllerLoggingEventID, "Alle data læst af : " + UserName);
            #endif
            return await _context.TodoItems.ToListAsync();
        }

        // GET: api/TodoItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(long id, string UserName ="No Name")
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

#if Test_Logging
            // below code is needed to get User name for Log             
            Serilog.Context.LogContext.PushProperty("UserName", UserName); //Push user in LogContext;  
            this._logger.LogWarning(TodoItemsControllerLoggingEventID, "Data med ID : " + id.ToString() + " læst af : " + UserName);
#endif
            return todoItem;
        }

        // PUT: api/TodoItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem, string UserName = "No Name")
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                #if Test_Logging
                // below code is needed to get User name for Log             
                Serilog.Context.LogContext.PushProperty("UserName", UserName); //Push user in LogContext;  
                this._logger.LogWarning(TodoItemsControllerLoggingEventID, "Data med ID : " + id.ToString() + " ændret af : " + UserName);
                #endif
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TodoItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem, string UserName = "No Name")
        {
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            #if Test_Logging
            // below code is needed to get User name for Log             
            Serilog.Context.LogContext.PushProperty("UserName", UserName); //Push user in LogContext;  
            this._logger.LogWarning(TodoItemsControllerLoggingEventID, "Data oprettet af : " + UserName);
            #endif

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id, string UserName = "No Name")
        {
            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();

            #if Test_Logging
            // below code is needed to get User name for Log             
            Serilog.Context.LogContext.PushProperty("UserName", UserName); //Push user in LogContext;  
            this._logger.LogWarning(TodoItemsControllerLoggingEventID, "Data med ID : " + id.ToString() + " slettet af : " + UserName);
            #endif

            return NoContent();
        }

        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
