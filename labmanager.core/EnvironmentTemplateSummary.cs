using System;
using System.Collections.Generic;
using System.Text;

namespace labmanager.core
{
    public class EnvironmentTemplateSummary
    {
        public Guid id { get; set; }
        public string Name { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public bool canDelete { get; set; }
        public Guid iconTaskId { get; set; }
        public string environment { get; set; }
        public bool isDeleted { get; set; }
    }
}
