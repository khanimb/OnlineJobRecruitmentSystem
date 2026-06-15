using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OnlineJobRecruitmentSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetJobs()
        {
            // Logic to retrieve job listings from the database
            return Ok(new { Message = "List of jobs" });
        }
        [HttpPost]
        public IActionResult CreateJob([FromBody] Job job)
        {
            // Logic to create a new job listing in the database
            return Ok(new { Message = "Job created successfully" });
        }
        [HttpPost]
        [Route("apply")]
        public IActionResult ApplyForJob([FromBody] JobApplication application)
        {
            // Logic to apply for a job and save the application in the database
            return Ok(new { Message = "Applied for job successfully" });
        }
    }
}
