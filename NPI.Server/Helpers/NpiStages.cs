namespace NPI.Server.Helpers
{
    public static class NpiStages
    {
        public const string Enquiry         = "0.0";
        public const string ProjectStart    = "1.0";
        public const string PilotMould      = "2.0";
        public const string MachinePurchase = "3.0";
        public const string ProductionMould = "4.0";
        public const string TrialRun        = "5.0";

        public static readonly IReadOnlyDictionary<string, string> Names =
            new Dictionary<string, string>
            {
                [Enquiry]           = "Enquiry",
                [ProjectStart]      = "Project Start",
                [PilotMould]        = "Pilot Mould Fabrication",
                [MachinePurchase]   = "New Machine Purchase",
                [ProductionMould]   = "Production Mould Fabrication",
                [TrialRun]          = "Trial Run at JJ"
            };

        public static readonly IReadOnlySet<string> Required =
            new HashSet<string> { Enquiry, ProjectStart, ProductionMould, TrialRun };

        public static readonly IReadOnlySet<string> AutoComplete =
            new HashSet<string> { Enquiry };

        public static bool IsValid(string stageId) => Names.ContainsKey(stageId);
    }
}