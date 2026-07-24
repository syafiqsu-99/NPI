namespace NPI.Server.Helpers
{
    public static class NotificationTypes
    {
        // Task-scoped — navigate to the task
        public const string Handover = "handover";
        public const string TaskAssigned = "task_assigned";
        public const string TaskComment = "task_comment";
        public const string DateRevised = "date_revised";
        public const string Overdue = "overdue";
        public const string DueSoon = "due_soon";
        public const string ApprovalStalled = "approval_stalled";

        // Project-scoped — navigate to the project
        public const string StageComplete = "stage_complete";
        public const string ProjectLaunch = "project_launch";
        public const string PlanningStuck = "planning_stuck";
        public const string ProjectPlanning = "project_planning";
        public const string ProjectActive = "project_active";
        public const string ProjectOnHold = "project_on_hold";
        public const string ProjectComplete = "project_complete";
        public const string ProjectCancelled = "project_cancelled";
        public const string ProjectStatusChanged = "project_status_changed";

        // Enquiry-scoped — navigate to the enquiry
        public const string EnquiryReview = "enquiry_review";
        public const string EnquirySubmitted = "enquiry_submitted";
    }
}
