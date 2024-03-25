using TestApp;

namespace TestAppAPI
{
    public interface IStudyGroupRepository
    {
        Task CreateStudyGroup(StudyGroup studyGroup);
        Task<List<StudyGroup>> GetStudyGroups();
        Task<List<StudyGroup>> SearchStudyGroups(string subject);
        Task<List<StudyGroup>> JoinStudyGroup(int studyGroupId, int userId);
        Task LeaveStudyGroup(int studyGroupId, int userId);

        Task DeleteStudyGroup(int studyGroupId);
        Task<StudyGroup> GetStudyGroup(int studyGroupId);
    }
}
