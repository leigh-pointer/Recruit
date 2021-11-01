﻿namespace Recruit.Shared.ViewModels
{
    public class JobDetailsModel
    {
        public int JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? Location { get; set; }
        public string? Department { get; set; }
        public string? JobType { get; set; }
        public List<Stage> Stages { get; set; }
        public List<Applicant> Applicants { get; set; }

        public JobDetailsModel()
        {
            Stages = new List<Stage>();
            Applicants = new List<Applicant>();
        }

    }
}