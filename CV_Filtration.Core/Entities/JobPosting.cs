﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV_Filtation_System.Core.Entities
{
    public class JobPosting
    {
        public int JobPostingId { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public string? SalaryRange { get; set; }
        public string? Description { get; set; }
        // Navigation property for related companies
        public List<CompanyJobPosting> CompanyJobPostings { get; set; } = new List<CompanyJobPosting>();
    }

}
