namespace SupportManagement.Domain.Constants;

public static class AppConstants
{
    public const int MaxAttachmentSizeMb = 10;
    public const int MaxAttachmentSizeBytes = MaxAttachmentSizeMb * 1024 * 1024;
    public static readonly string[] AllowedAttachmentTypes = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip", ".log" };
    
    public const int JwtExpiryMinutes = 60;
    public const int RefreshTokenExpiryDays = 7;
    
    public const string TicketNoPrefix = "TKT";
    
    public static class SlaDefaults
    {
        public const int CriticalResponseMinutes = 15;
        public const int CriticalResolutionMinutes = 240;
        public const int HighResponseMinutes = 60;
        public const int HighResolutionMinutes = 480;
        public const int MediumResponseMinutes = 240;
        public const int MediumResolutionMinutes = 2880;
        public const int LowResponseMinutes = 1440;
        public const int LowResolutionMinutes = 4320;
    }
}
