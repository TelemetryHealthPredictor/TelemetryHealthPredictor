namespace TelemetryHealthPredictor.Config.Settings
{
    public class BaselineSettings
    {
        public string BaselineFolderPath { get; set; } = string.Empty;
        public string ScalarsFileName { get; set; } = string.Empty;

        // ---- Keys in scalars.json ----
        public string ForgettingFactorKey { get; set; } = string.Empty;
        public string MDOutliersStepSizeKey { get; set; } = string.Empty;
        public string D2WeightKey { get; set; } = string.Empty;
        public string TDecayKey { get; set; } = string.Empty;
        public string HysteresisEntryKey { get; set; } = string.Empty;
        public string HysteresisExitKey { get; set; } = string.Empty;

        // ---- Profile file names inside BaselineFolderPath ----
        public string NameForQc { get; set; } = string.Empty;
        public string NameForR0 { get; set; } = string.Empty;
        public string NameForCov { get; set; } = string.Empty;
        public string NameForCusumSlacks { get; set; } = string.Empty;
        public string NameForCusumThresholds { get; set; } = string.Empty;
        public string NameForMDThresholds { get; set; } = string.Empty;
        public string NameForCusumRawThresholds { get; set; } = string.Empty;
    }
}
