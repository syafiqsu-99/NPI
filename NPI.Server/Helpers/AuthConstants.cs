namespace NPI.Server.Helpers
{
    public static class SystemRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Member = "Member";
    }

    public static class ProjectRoleNames
    {
        public const string TeamLead = "Team Lead";
        public const string Member = "Member";
        public const string Viewer = "Viewer";
    }

    public static class EnquiryStatus
    {
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
    }

    public static class FileStatus
    {
        public const string Active = "Active";
        public const string Deleted = "Deleted";
    }

    public static class TasksStatus
    {
        public const string NotStarted  = "Not Started";
        public const string InProgress  = "In Progress";
        public const string OnHold      = "On Hold";
        public const string Completed   = "Completed";
        public const string Cancelled   = "Cancelled";
    }

    public static class ProjectsStatus
    {
        public const string Planning    = "Planning";
        public const string NotStarted  = "Not Started";
        public const string InProgress  = "In Progress";
        public const string OnHold      = "On Hold";
        public const string Completed   = "Completed";
        public const string Cancelled   = "Cancelled";
    }
    public static class DeptCodes
    {
        public const string Sales = "SLS";
        public const string Technical = "TEC";
        public const string Purchaser = "PUR";
        public const string Production = "PRD";
        public const string QA = "QA";
        public const string Management = "MGT";
    }
}
