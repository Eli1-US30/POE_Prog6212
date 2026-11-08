namespace RaceDay.MVC.Helpers
{
    public static class Format
    {
        public static string AgeText(int? min, int? max) =>
            min == null && max == null ? "All ages" :
            max == null ? $"{min} and over" :
            min == null ? $"Up to {max}" :
            $"{min} to {max}";

        public static string Distance(decimal? km) =>
            km == null ? "-" : km.Value.ToString("0.##") + " km";

        // 1st, 2nd, 3rd, 4th ... 11th, 12th, 13th ... 47th
        public static string Ordinal(int n)
        {
            if (n % 100 is 11 or 12 or 13) return n + "th";
            return (n % 10) switch { 1 => n + "st", 2 => n + "nd", 3 => n + "rd", _ => n + "th" };
        }
    }
}