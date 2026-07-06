using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPI.Server.DTOs;
using NPI.Server.Helpers;
using NPI.Server.Models;
using NPI.Server.Services;
using System.Security.Claims;

namespace NPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            try
            {
                var tasks = await _taskService.GetAllTasksAsync();
                return Ok(new { success = true, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("my-tasks")]
        public async Task<IActionResult> GetMyTasks()
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

                var tasks = await _taskService.GetTasksByProjectTeamsAsync(userId, userRole);
                return Ok(new { success = true, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTask(int taskId)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(taskId);
                if (task == null)
                {
                    return NotFound(new { success = false, message = "Task not found" });
                }
                return Ok(new { success = true, data = task });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            try
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                }
                var (success, message, taskId) = await _taskService.CreateTaskAsync(dto, userId);

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new { success = true, message, data = new { task_id = taskId } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateTaskDto dto)
        {
            try
            {
                var userDeptId = RbacHelper.GetDepartmentId(User);

                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                }
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

                var (success, message) = await _taskService.UpdateTaskAsync(taskId, dto, userId, userRole, userDeptId);

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            try
            {
                var userDeptId = RbacHelper.GetDepartmentId(User);

                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

                var result = await _taskService.DeleteTaskAsync(taskId, userId, userRole, userDeptId);

                if (result.success)
                {
                    return Ok(new { success = true, message = result.message });
                }

                return BadRequest(new { success = false, message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{taskId}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, [FromBody] UpdateTaskStatusDto dto)
        {
            try
            {
                var userDeptId = RbacHelper.GetDepartmentId(User);

                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                }
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

                var result = await _taskService.UpdateTaskStatusAsync(taskId, dto.status, userId, userRole, userDeptId);

                if (result.success)
                {
                    return Ok(new { success = true, message = result.message });
                }

                return BadRequest(new { success = false, message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{taskId}/progress")]
        public async Task<IActionResult> UpdateTaskProgress(int taskId, [FromBody] UpdateTaskProgressDto dto)
        {
            try
            {
                var userDeptId = RbacHelper.GetDepartmentId(User);

                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

                var result = await _taskService.UpdateTaskProgressAsync(taskId, dto.per_complete, userId, userRole, userDeptId);

                if (result.success)
                {
                    return Ok(new { success = true, message = result.message });
                }

                return BadRequest(new { success = false, message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{taskId}/planned-dates")]
        public async Task<IActionResult> UpdatePlannedDates(int taskId, [FromBody] UpdatePlannedDatesDto dto)
        {
            try
            {
                var userDeptId = RbacHelper.GetDepartmentId(User);

                var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                    return Unauthorized(new { success = false, message = "Invalid user identity claim." });
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

                var result = await _taskService.UpdatePlannedDatesAsync(taskId, dto.new_start_date, dto.new_end_date, dto.note, userId, userRole, userDeptId);

                if (result.success)
                {
                    return Ok(new { success = true, message = result.message });
                }
                return BadRequest(new { success = false, message = result.message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(int projectId)
        {
            try
            {
                var tasks = await _taskService.GetTasksByProjectAsync(projectId);
                return Ok(new { success = true, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetTasksByUser(int userId)
        {
            try
            {
                var tasks = await _taskService.GetTasksByUserAsync(userId);
                return Ok(new { success = true, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("by-department/{deptId}")]
        public async Task<IActionResult> GetTasksByDepartment(int deptId)
        {
            try
            {
                var tasks = await _taskService.GetTasksByDepartmentAsync(deptId);
                return Ok(new { success = true, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}