﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruit.Server.Data;
using Recruit.Shared;
using Recruit.Shared.Extensions;
using Recruit.Shared.ViewModels;

namespace Recruit.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public JobsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IEnumerable<Job>> Get()
        {
            var jobs = await _db.Jobs
                .Include(j => j.Applicants)
                .Include(j => j.Department)
                .OrderByDescending(j => j.PostDate)
                .ToListAsync();

            return jobs;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var job = _db.Jobs.Find(id);
            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpGet("Due")]
        public async Task<IEnumerable<Job>> GetDueJobs()
        {
            var jobs = await _db.Jobs
                .OrderBy(j => j.Expires)
                .Take(10)
                .ToListAsync();

            return jobs;
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _db.Jobs
                .Where(j => j.Id == id)
                .Include(j => j.Applicants)
                .Include(j => j.Stages)
                .Include(j => j.Department)
                .FirstOrDefaultAsync();

            if (job == null)
                return NotFound();

            var model = new JobDetailsViewModel
            {
                JobId = job.Id,
                JobTitle = job.Title,
                Location = $"{job.City}, {job.Country}",
                Department = job.Department?.Name,
                JobType = job.JobType.GetDisplayName(),
                Applicants = job.Applicants?.ToList() ?? new List<Applicant>(),
                Stages = job.Stages?.ToList() ?? new List<Stage>()
            };

            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Job job)
        {
            var department = _db.Departments.Find(job.DepartmentId);

            var newJob = new Job()
            {
                Title = job.Title,
                Description = job.Description,
                Department = department,
                Country = job.Country,
                City = job.City,
                ContactPhone = job.ContactPhone,
                ContactEmail = job.ContactEmail,
                Manager = job.Manager,
                JobType = job.JobType,
                JobExperience = job.JobExperience,
                RequiredSkills = job.RequiredSkills,
                PostDate = DateTime.Now,
                Expires = job.Expires,
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                Published = job.Published
            };

            var stages = new List<Stage>()
            {
                new Stage() { Name = "Screen" },
                new Stage() { Name = "Interview" },
                new Stage() { Name = "Offer" },
                new Stage() { Name = "Hire" }
            };

            newJob.Stages = stages;

            _db.Jobs.Add(newJob);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] Job job)
        {
            var jobToEdit = await _db.Jobs.FindAsync(id);
            if (jobToEdit == null)
                return NotFound();

            var department = _db.Departments.Find(job.DepartmentId);

            jobToEdit.Title = job.Title;
            jobToEdit.Description = job.Description;
            jobToEdit.DepartmentId = department?.Id;
            jobToEdit.Country = job.Country;
            jobToEdit.City = job.City;
            jobToEdit.ContactPhone = job.ContactPhone;
            jobToEdit.ContactEmail = job.ContactEmail;
            jobToEdit.Manager = job.Manager;
            jobToEdit.JobType = job.JobType;
            jobToEdit.JobExperience = job.JobExperience;
            jobToEdit.RequiredSkills = job.RequiredSkills;
            jobToEdit.Expires = job.Expires;
            jobToEdit.SalaryFrom = job.SalaryFrom;
            jobToEdit.SalaryTo = job.SalaryTo;
            jobToEdit.Published = job.Published;

            _db.Jobs.Update(jobToEdit);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{id}/Publish")]
        public async Task<IActionResult> Publish(int id)
        {
            var job = await _db.Jobs.FindAsync(id);
            if (job == null)
                return NotFound();

            job.Published = true;

            _db.Jobs.Update(job);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{id}/Unpublish")]
        public async Task<IActionResult> Unpublish(int id)
        {
            var job = await _db.Jobs.FindAsync(id);
            if (job == null)
                return NotFound();

            job.Published = false;

            _db.Jobs.Update(job);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{id}/Clone")]
        public async Task<IActionResult> Clone(int id)
        {
            var job = await _db.Jobs
                .Where(j => j.Id == id)
                .Include(j => j.Stages)
                .Include(j => j.Department)
                .FirstOrDefaultAsync();

            if (job == null)
                return NotFound();

            var newJob = new Job()
            {
                Title = $"Copy of {job.Title}",
                Description = job.Description,
                Department = job.Department,
                Country = job.Country,
                City = job.City,
                ContactPhone = job.ContactPhone,
                ContactEmail = job.ContactEmail,
                Manager = job.Manager,
                JobType = job.JobType,
                JobExperience = job.JobExperience,
                RequiredSkills = job.RequiredSkills,
                PostDate = DateTime.Now,
                Expires = job.Expires,
                SalaryFrom = job.SalaryFrom,
                SalaryTo = job.SalaryTo,
                Published = false,
                Applicants = new List<Applicant>(),
                Stages = new List<Stage>()
            };

            foreach (var stage in job.Stages ?? Enumerable.Empty<Stage>())
            {
                newJob?.Stages?.Add(new Stage() { Name = stage.Name });
            }

            _db.Jobs.Add(newJob);
            await _db.SaveChangesAsync();

            return Ok(newJob);
        }

        [HttpPost("{id}/Invite")]
        public IActionResult Invite(int id, InviteViewModel model)
        {
            Console.WriteLine("TODO: Send invitation email");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var job = await _db.Jobs
                .Include(j => j.Applicants)
                .Include(j => j.Stages)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
                return NotFound();

            _db.Jobs.Remove(job);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
