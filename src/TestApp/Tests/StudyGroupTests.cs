using NUnit.Framework;

namespace TestApp.Tests
{
    public class StudyGroupTests
    {
        private StudyGroup studyGroup;
        private Random random = new Random();
        private Subject[] subjectValues = Enum.GetValues(typeof(Subject)).Cast<Subject>().ToArray();
        private int countUsers = 0;

        [SetUp]
        public void TestSetup()
        {
            var user = GenerateUser();
            var users = new List<User> { user };
            studyGroup = GenerateStudyGroup(users);
        }

        [Test]
        public void AddUserTest_User_UserAddedToStudyGroup()
        {
            // Arrange
            var user1 = studyGroup.Users.First();
            var user2 = GenerateUser();
            var users = new List<User> { user1, user2 };
            var expectedStudyGroup = GenerateStudyGroup(users);

            // Act
            studyGroup.AddUser(user2);

            // Assert
            Assert.AreEqual(expectedStudyGroup.Users.Count, studyGroup.Users.Count);
            Assert.AreEqual(expectedStudyGroup.Users.Last(), studyGroup.Users.Last());
        }

        [Test]
        public void RemoveUserTest_User_UserRemovedFromStudyGroup()
        {
            // Arrange
            var user1 = studyGroup.Users.First();
            var users = new List<User>();
            var expectedStudyGroup = GenerateStudyGroup(users);

            // Act
            studyGroup.RemoveUser(user1);

            // Assert
            Assert.AreEqual(expectedStudyGroup.Users.Count, studyGroup.Users.Count);
        }

        #region Support methods

        /// <summary>
        /// Generating Study Group name.
        /// </summary>
        /// <param name="subject">Subject of Study Group.</param>
        /// <returns>Study Group name.</returns>
        private string GenerateStudyGroupName(Subject subject)
        {
            return subject.ToString().Substring(0, 4) + "-" + random.Next(100, 900);
        }

        /// <summary>
        /// Generating a Study Group object.
        /// </summary>
        /// <param name="users">Users that are in Study Group.</param>
        /// <returns><see cref="StudyGroup"/> object.</returns>
        private StudyGroup GenerateStudyGroup(List<User> users)
        {
            var studyGroupId = 1;
            var subject = subjectValues[random.Next(subjectValues.Length - 1)];
            var name = GenerateStudyGroupName(subject);
            return new StudyGroup(studyGroupId, name, subject, DateTime.UtcNow, users);
        }

        /// <summary>
        /// Generating a User object.
        /// </summary>
        /// <returns><see cref="User"/> object.</returns>
        private User GenerateUser()
        {
            var userId = countUsers++;
            var names = new List<string> { "John", "Miguel", "Mike", "Manuel", "Anton" };
            var userName = names[random.Next(names.Count)];

            return new User(userId, userName);
        }

        #endregion
    }
}