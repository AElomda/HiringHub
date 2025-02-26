﻿using CV.Filtation.System.API.DTO;
using CV.Filtation.System.API.Helpers;
using CV_Filtation_System.Core.Entities;
using CV_Filtation_System.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CV.Filtation.System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPostingsController : ControllerBase
    {
        private readonly IJobPostingService _jobPostingService;
        private readonly AppDbContext _context;

        public JobPostingsController(IJobPostingService jobPostingService, AppDbContext context)
        {
            _jobPostingService = jobPostingService;
            _context = context;
        }


        // POST api/jobpostings
        [HttpPost]
        public async Task<IActionResult> CreateJobPosting([FromForm] DTO.CreateJobPostingDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid data.");
            }
            var companyExists = await _context.Companies
            .AnyAsync(c => c.CompanyId == dto.CompanyId);

            if (!companyExists)
            {
                return BadRequest($"Company with ID {dto.CompanyId} does not exist.");
            }

            var jobPosting = new JobPosting
            {
                Title = dto.Title,
                Location = dto.Location,
                SalaryRange = dto.SalaryRange,
                Description = dto.Description,
                CompanyId = dto.CompanyId,
                JopType = dto.JopType,
                WorkMode = dto.WorkMode
            };

            if (dto.JobImageUrl != null)
            {
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "job_images");

                // Use the helper function
                string fileName = await FileUploadHelper.SaveUploadedFileAsync(
                    dto.JobImageUrl,
                    uploadFolder
                );

                jobPosting.JobImageUrl = "/job_images/"+fileName;
            }

            _context.JobPostings.Add(jobPosting);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetJobPostingById), new { id = jobPosting.JobPostingId }, jobPosting);
        }

        // GET api/jobpostings/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobPostingById(int id)
        {
            var jobPosting = await _context.JobPostings
                .Include(jp => jp.Company) // Directly include the Company
                .FirstOrDefaultAsync(jp => jp.JobPostingId == id);

            if (jobPosting == null) 
            {
                return NotFound();
            }

            return Ok(jobPosting);
        }

        // GET api/jobpostings
        [HttpGet]
        public async Task<IActionResult> GetAllJobPostings([FromQuery] string? title, [FromQuery] string? location)
        {
            var jobPostingsQuery = _context.JobPostings.AsQueryable();

            if (!string.IsNullOrEmpty(title))
                jobPostingsQuery = jobPostingsQuery.Where(jp => jp.Title.ToLower().Contains(title.ToLower()));

            if (!string.IsNullOrEmpty(location))
                jobPostingsQuery = jobPostingsQuery.Where(jp => jp.Location.ToLower().Contains(location.ToLower()));

            var jobPostings = await jobPostingsQuery.ToListAsync();

            return Ok(jobPostings);
        }

        // PUT api/jobpostings/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobPosting(int id, [FromBody] UpdateJobPostingDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid data.");
            }

            var jobPosting = await _context.JobPostings.FindAsync(id);
            if (jobPosting == null)
            {
                return NotFound();
            }

            // Update job posting details
            jobPosting.Title = dto.Title ?? jobPosting.Title;
            jobPosting.Location = dto.Location ?? jobPosting.Location;
            jobPosting.SalaryRange = dto.SalaryRange ?? jobPosting.SalaryRange;
            jobPosting.Description = dto.Description ?? jobPosting.Description;
            jobPosting.WorkMode = dto.WorkMode ?? jobPosting.WorkMode;
            jobPosting.JopType = dto.JopType ?? jobPosting.JopType;

            _context.JobPostings.Update(jobPosting);
            await _context.SaveChangesAsync();

            return Ok(jobPosting);
        }

        // DELETE api/jobpostings/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobPosting(int id)
        {
            var jobPosting = await _context.JobPostings.FindAsync(id);
            if (jobPosting == null)
            {
                return NotFound();
            }

            _context.JobPostings.Remove(jobPosting);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        // GET api/jobpostings/company/{companyId}
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetJobPostingsByCompany(int companyId)
        {
            var jobPostings = await _context.JobPostings
                .Where(jp => jp.CompanyId == companyId) 
                .ToListAsync();

            if (jobPostings == null || !jobPostings.Any())
            {
                return NotFound();
            }

            return Ok(jobPostings);
        }

    }
}
