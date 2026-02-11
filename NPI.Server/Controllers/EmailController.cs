using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NPI.Server.Models;
using NPI.Server.Services;
using System.ComponentModel.DataAnnotations;

namespace NPI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailController> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailController(
        IEmailSender emailSender,
        ILogger<EmailController> logger,
        IOptions<EmailSettings> emailSettings)
    {
        _emailSender = emailSender;
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid email data",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });
        }

        try
        {
            // Parse multiple recipients
            var recipients = request.ReceiverEmail
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToList();

            if (!recipients.Any())
            {
                return BadRequest(new { success = false, message = "No valid recipient email addresses" });
            }

            // Format the email body (use sender from config)
            string emailBody = request.IsHtml
                ? request.Body
                : FormatAsHtmlEmail(request.Subject, request.Body);

            // Send email to each recipient
            var sendTasks = recipients.Select(recipient =>
                _emailSender.SendEmailAsync(recipient, request.Subject, emailBody)
            );

            await Task.WhenAll(sendTasks);

            _logger.LogInformation(
                "Email sent successfully. From: {SenderName} <{SenderEmail}>, To: {Recipients}, Subject: {Subject}",
                _emailSettings.SenderName,
                _emailSettings.SenderEmail,
                string.Join(", ", recipients),
                request.Subject
            );

            return Ok(new
            {
                success = true,
                message = $"Email sent successfully to {recipients.Count} recipient(s)",
                recipientCount = recipients.Count,
                recipients = recipients,
                sentFrom = _emailSettings.SenderEmail
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email. Subject: {Subject}", request.Subject);

            return StatusCode(500, new
            {
                success = false,
                message = "Failed to send email. Please check the server logs for details.",
                error = ex.Message
            });
        }
    }

    [HttpPost("send-template")]
    public async Task<IActionResult> SendTemplateEmail([FromBody] TemplateEmailRequest request)
    {
        try
        {
            string htmlBody = request.TemplateName switch
            {
                "npi_created" => GenerateNPICreatedTemplate(request.TemplateData),
                "task_assigned" => GenerateTaskAssignedTemplate(request.TemplateData),
                "deadline_reminder" => GenerateDeadlineReminderTemplate(request.TemplateData),
                "workflow_completed" => GenerateWorkflowCompletedTemplate(request.TemplateData),
                _ => throw new ArgumentException($"Unknown template: {request.TemplateName}")
            };

            await _emailSender.SendEmailAsync(request.ReceiverEmail, request.Subject, htmlBody);

            return Ok(new { success = true, message = "Template email sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send template email: {TemplateName}", request.TemplateName);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new
        {
            message = "Email controller is working!",
            timestamp = DateTime.Now,
            smtpServer = _emailSettings.SmtpServer,
            smtpPort = _emailSettings.SmtpPort,
            senderEmail = _emailSettings.SenderEmail,
            senderName = _emailSettings.SenderName,
            authEnabled = _emailSettings.UseAuthentication
        });
    }

    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        return Ok(new
        {
            senderEmail = _emailSettings.SenderEmail,
            senderName = _emailSettings.SenderName,
            smtpServer = _emailSettings.SmtpServer,
            smtpPort = _emailSettings.SmtpPort
        });
    }

    private string FormatAsHtmlEmail(string subject, string body)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .email-container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .email-header {{ background-color: #3498db; color: white; padding: 20px; text-align: center; }}
                    .email-body {{ background-color: #f9f9f9; padding: 20px; white-space: pre-wrap; }}
                    .email-footer {{ background-color: #ecf0f1; padding: 15px; text-align: center; font-size: 12px; color: #7f8c8d; }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='email-header'>
                        <h2>{_emailSettings.SenderName}</h2>
                        <p>{subject}</p>
                    </div>
                    <div class='email-body'>
                        {System.Net.WebUtility.HtmlEncode(body)}
                    </div>
                    <div class='email-footer'>
                        <p>This email was sent from {_emailSettings.SenderName} ({_emailSettings.SenderEmail})</p>
                        <p>NPI Tracking System - {DateTime.Now:yyyy}</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    #region Email Templates

    private string GenerateNPICreatedTemplate(Dictionary<string, string> data)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2c3e50;'>New NPI Project Created</h2>
                    <p>A new NPI project has been created and requires your attention.</p>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr>
                            <td style='padding: 10px; border: 1px solid #ddd;'><strong>Project Name:</strong></td>
                            <td style='padding: 10px; border: 1px solid #ddd;'>{data.GetValueOrDefault("projectName", "N/A")}</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; border: 1px solid #ddd;'><strong>Project ID:</strong></td>
                            <td style='padding: 10px; border: 1px solid #ddd;'>{data.GetValueOrDefault("projectId", "N/A")}</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; border: 1px solid #ddd;'><strong>Created By:</strong></td>
                            <td style='padding: 10px; border: 1px solid #ddd;'>{data.GetValueOrDefault("createdBy", "N/A")}</td>
                        </tr>
                        <tr>
                            <td style='padding: 10px; border: 1px solid #ddd;'><strong>Due Date:</strong></td>
                            <td style='padding: 10px; border: 1px solid #ddd;'>{data.GetValueOrDefault("dueDate", "N/A")}</td>
                        </tr>
                    </table>
                    <p style='margin-top: 20px;'>
                        <a href='{data.GetValueOrDefault("projectUrl", "#")}' 
                            style='background-color: #3498db; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                            View Project
                        </a>
                    </p>
                </div>
            </body>
            </html>";
    }

    private string GenerateTaskAssignedTemplate(Dictionary<string, string> data)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #e74c3c;'>Task Assigned to You</h2>
                    <p>You have been assigned a new task in the NPI system.</p>
                    <div style='background-color: #f9f9f9; padding: 15px; border-left: 4px solid #e74c3c;'>
                        <h3>{data.GetValueOrDefault("taskName", "N/A")}</h3>
                        <p><strong>Priority:</strong> {data.GetValueOrDefault("priority", "Normal")}</p>
                        <p><strong>Due Date:</strong> {data.GetValueOrDefault("dueDate", "N/A")}</p>
                        <p><strong>Description:</strong></p>
                        <p>{data.GetValueOrDefault("description", "No description provided")}</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private string GenerateDeadlineReminderTemplate(Dictionary<string, string> data)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #f39c12;'>⚠️ Deadline Reminder</h2>
                    <p>This is a reminder that the following task is approaching its deadline.</p>
                    <div style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #f39c12;'>
                        <h3>{data.GetValueOrDefault("taskName", "N/A")}</h3>
                        <p><strong>Due Date:</strong> {data.GetValueOrDefault("dueDate", "N/A")}</p>
                        <p><strong>Days Remaining:</strong> {data.GetValueOrDefault("daysRemaining", "N/A")}</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    private string GenerateWorkflowCompletedTemplate(Dictionary<string, string> data)
    {
        return $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #27ae60;'>✓ Workflow Completed</h2>
                    <p>The following workflow has been successfully completed.</p>
                    <div style='background-color: #d4edda; padding: 15px; border-left: 4px solid #27ae60;'>
                        <h3>{data.GetValueOrDefault("workflowName", "N/A")}</h3>
                        <p><strong>Completed By:</strong> {data.GetValueOrDefault("completedBy", "N/A")}</p>
                        <p><strong>Completion Date:</strong> {data.GetValueOrDefault("completionDate", "N/A")}</p>
                    </div>
                </div>
            </body>
            </html>";
    }

    #endregion
}

#region Request Models

public class EmailRequest
{

    [Required(ErrorMessage = "Receiver email is required")]
    public string ReceiverEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject is required")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email body is required")]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = false;
}

public class TemplateEmailRequest
{
    [Required]
    public string TemplateName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ReceiverEmail { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    public Dictionary<string, string> TemplateData { get; set; } = new();
}

#endregion