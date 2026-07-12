namespace OnlineJobRecruitmentSystem.Application.Interfaces
{
    public enum JobOperationResult { Success, EmployerNotFound, JobNotFound }
    public enum JobApplicationResult { Success, ProfileNotFound, JobNotFound, AlreadyApplied }
    public enum SaveJobResult { Success, ProfileNotFound, JobNotFound, AlreadySaved }
    public enum RemoveSavedJobResult { Success, ProfileNotFound, SavedJobNotFound }
    public enum ContractResultStatus { Success, EmployerNotFound, JobNotFound }
    public enum ContractAccessStatus { Success, NotFound, Forbidden }
    public enum ContractUpdateStatus { Success, EmployerNotFound, ContractNotFound }
    public enum RegisterResult { Success, EmailExists, UsernameExists }
    public enum LoginResult { Success, InvalidCredentials, EmailNotVerified }
}