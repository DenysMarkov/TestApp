using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NUnit.Framework;
using TestApp;

namespace TestAppAPI.Tests
{
    public class StudyGroupControllerTests
    {
        private static IStudyGroupRepository studyGroupRepository;
        private IUserRepository userRepository;
        private StudyGroupController studyGroupController = new StudyGroupController(studyGroupRepository);
        private List<StudyGroup> studyGroups = new List<StudyGroup>(3);
        private Random random = new Random();
        private Subject[] subjectValues = Enum.GetValues(typeof(Subject)).Cast<Subject>().ToArray();
        private int freeSubjectId = 2;
        private List<User> users = new List<User>(2);
        private int userIdAllowedToJoin;
        private int userIdAllowedToLeave = 2;
        private int studyGroupIdAllowedToLeaveUser = 2;

        [OneTimeSetUp]
        public void OneTimeTestSetup()
        {
            var dateTime = DateTime.UtcNow;

            for (int i = 0; i < studyGroups.Capacity - 1; i++)
            {
                var studyGroupId = i + 1;
                var subject = subjectValues[i];
                var name = GenerateStudyGroupName(subject);
                dateTime = dateTime.AddSeconds(-1);

                var user = GenerateUser();
                var newUsers = new List<User> { user };
                users.Add(user);
                userRepository.CreateUser(user);

                var studyGroup = new StudyGroup(studyGroupId, name, subject, dateTime, newUsers);
                studyGroups.Add(studyGroup);
                studyGroupRepository.CreateStudyGroup(studyGroup);
            }

            var oneMoreUser = GenerateUser();
            users.Add(oneMoreUser);
            userRepository.CreateUser(oneMoreUser);
            userIdAllowedToJoin = oneMoreUser.UserId;
        }

        [Test]
        public void CreatingOneStudyGroupForSingleSubjectTest_StudyGroupCreated()
        {
            // Arrange
            var expectedStudyGroup = GenerateStudyGroup(DateTime.UtcNow);
            var expectedStatusCode = new CreatedResult(string.Empty, null).StatusCode;

            // Act
            var response = studyGroupController.CreateStudyGroup(expectedStudyGroup).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(expectedStudyGroup.StudyGroupId).Result;
            studyGroups.Add(expectedStudyGroup);

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            Assert.NotNull(actualStudyGroup);
            CompareStudyGroups(expectedStudyGroup, actualStudyGroup);
        }

        [Test]
        public void CreatingMoreThanOneStudyGroupForSingleSubjectTest_SecondStudyGroupNotCreated()
        {
            // Arrange
            var studyGroup1 = GenerateStudyGroup(DateTime.UtcNow);
            var studyGroup2 = GenerateStudyGroup(DateTime.UtcNow);
            var expectedStatusCode = new BadRequestResult().StatusCode;

            // Act
            _ = studyGroupController.CreateStudyGroup(studyGroup1);
            var response = studyGroupController.CreateStudyGroup(studyGroup2).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(studyGroup2.StudyGroupId).Result;
            studyGroups.Add(studyGroup1);

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            Assert.IsNull(actualStudyGroup);
        }

        [Test]
        public void CreatingStudyGroupWithNameSizeOfLessThan5CharactersTest_StudyGroupNotCreated()
        {
            // Arrange
            var name = GenerateStudyGroupName(subjectValues[freeSubjectId]);
            name = name.Remove(2);
            var studyGroup = GenerateStudyGroup(DateTime.UtcNow, name);
            var expectedStatusCode = new BadRequestResult().StatusCode;

            // Act
            var response = studyGroupController.CreateStudyGroup(studyGroup).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(studyGroup.StudyGroupId).Result;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            Assert.IsNull(actualStudyGroup);
        }

        [Test]
        public void CreatingStudyGroupWithNameSizeOfMoreThan30CharactersTest_StudyGroupNotCreated()
        {
            // Arrange
            var name = GenerateStudyGroupName(subjectValues[freeSubjectId]);
            for (int i = 0; i < 30; i++)
            {
                name += 't';
            }
            var studyGroup = GenerateStudyGroup(DateTime.UtcNow, name);
            var expectedStatusCode = new BadRequestResult().StatusCode;

            // Act
            var response = studyGroupController.CreateStudyGroup(studyGroup).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(studyGroup.StudyGroupId).Result;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            Assert.IsNull(actualStudyGroup);
        }

        [Test]
        public void CreatingStudyGroupWithCreateDateEarlierThan12HoursFromCurrentTimeTest_StudyGroupNotCreated()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddHours(-14);
            var studyGroup = GenerateStudyGroup(dateTime);
            var expectedStatusCode = new BadRequestResult().StatusCode;

            // Act
            var response = studyGroupController.CreateStudyGroup(studyGroup).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(studyGroup.StudyGroupId).Result;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            Assert.IsNull(actualStudyGroup);
        }

        [Test]
        public void CreatingStudyGroupWithCreateDateLaterThan12HoursFromCurrentTimeTest_StudyGroupNotCreated()
        {
            // Arrange
            var dateTime = DateTime.UtcNow.AddHours(14);
            var studyGroup = GenerateStudyGroup(dateTime);
            var expectedStatusCode = new BadRequestResult().StatusCode;

            // Act
            var response = studyGroupController.CreateStudyGroup(studyGroup).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(studyGroup.StudyGroupId).Result;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            Assert.IsNull(actualStudyGroup);
        }

        [Test]
        public void JoiningUserToStudyGroupsForDifferentSubjectsTest_StudyGroupsUpdated()
        {
            // Arrange
            var user = users.First(u => u.UserId == userIdAllowedToJoin);
            var expectedStudyGroup1 = studyGroups[0];
            var expectedStudyGroup2 = studyGroups[1];
            expectedStudyGroup1.AddUser(user);
            expectedStudyGroup2.AddUser(user);
            var expectedStatusCode = new OkResult().StatusCode;

            // Act
            var response = studyGroupController.JoinStudyGroup(expectedStudyGroup1.StudyGroupId, userIdAllowedToJoin).Result;
            var actualStatusCode1 = ((IStatusCodeActionResult)response).StatusCode;
            response = studyGroupController.JoinStudyGroup(expectedStudyGroup2.StudyGroupId, userIdAllowedToJoin).Result;
            var actualStatusCode2 = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup1 = studyGroupRepository.GetStudyGroup(expectedStudyGroup1.StudyGroupId).Result;
            var actualStudyGroup2 = studyGroupRepository.GetStudyGroup(expectedStudyGroup2.StudyGroupId).Result;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode1);
            Assert.AreEqual(expectedStatusCode, actualStatusCode2);
            CompareStudyGroups(expectedStudyGroup1, actualStudyGroup1);
            CompareStudyGroups(expectedStudyGroup2, actualStudyGroup2);
        }

        [Test]
        public void JoiningUserToStudyGroupWhichHeIsAlreadyInTest_StudyGroupNotUpdated()
        {
            // Arrange
            var user = users[0];
            var expectedStudyGroup = studyGroups[0];
            var expectedStatusCode = new BadRequestResult().StatusCode;

            // Act
            var response = studyGroupController.JoinStudyGroup(expectedStudyGroup.StudyGroupId, user.UserId).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(expectedStudyGroup.StudyGroupId).Result;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            CompareStudyGroups(expectedStudyGroup, actualStudyGroup);
        }

        [Test]
        public void JoiningUserToStudyGroupThatDoesNotExistTest_StudyGroupNotFound()
        {
            // Arrange
            var user = users[random.Next(users.Count - 1)];
            var studyGroup = GenerateStudyGroup(DateTime.UtcNow);
            var expectedStatusCode = new NotFoundResult().StatusCode;

            // Act
            var response = studyGroupController.JoinStudyGroup(studyGroup.StudyGroupId, user.UserId).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(studyGroup.StudyGroupId).Result;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            Assert.IsNull(actualStudyGroup);
        }

        [Test]
        public void JoiningNonExistentUserToStudyGroupTest_UserNotFound()
        {
            // Arrange
            var user = GenerateUser();
            var expectedStudyGroup = studyGroups[random.Next(studyGroups.Count - 1)];
            var expectedStatusCode = new NotFoundResult().StatusCode;

            // Act
            var response = studyGroupController.JoinStudyGroup(expectedStudyGroup.StudyGroupId, user.UserId).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(expectedStudyGroup.StudyGroupId).Result;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            CompareStudyGroups(expectedStudyGroup, actualStudyGroup);
        }

        [Test]
        public void CheckingListOfAllExistingStudyGroupsTest_StudyGroupsDownloaded()
        {
            // Arrange
            var expectedStudyGroups = studyGroups;
            var expectedStatusCode = new OkObjectResult(expectedStudyGroups).StatusCode;

            // Act
            var response = studyGroupController.GetStudyGroups().Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var responseValue = ((OkObjectResult)response).Value;
            List<StudyGroup> actualStudyGroups = responseValue == null ? null : responseValue as List<StudyGroup>;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            Assert.IsNotNull(actualStudyGroups);
            Assert.AreEqual(expectedStudyGroups.Count, actualStudyGroups.Count);
            for (int i = 0; i < expectedStudyGroups.Count; i++)
            {
                CompareStudyGroups(expectedStudyGroups[i], actualStudyGroups[i]);
            }
        }

        [Test]
        public void FiltersStudyGroupsByGivenSubjectTest_FilteredStudyGroupsDownloaded()
        {
            // Arrange
            var expectedStudyGroups = studyGroups.Where(sg => sg.Subject == studyGroups[0].Subject).ToList();
            var expectedStatusCode = new OkObjectResult(expectedStudyGroups).StatusCode;
            var subjectExpectedStudyGroups = studyGroups[0].Subject.ToString();

            // Act
            var response = studyGroupController.SearchStudyGroups(subjectExpectedStudyGroups).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var responseValue = ((OkObjectResult)response).Value;
            List<StudyGroup> actualStudyGroups = responseValue == null ? null : responseValue as List<StudyGroup>;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            Assert.IsNotNull(actualStudyGroups);
            Assert.AreEqual(expectedStudyGroups.Count, actualStudyGroups.Count);
            for (int i = 0; i < expectedStudyGroups.Count; i++)
            {
                CompareStudyGroups(expectedStudyGroups[i], actualStudyGroups[i]);
            }
        }

        [Test]
        public void LeavingUserFromStudyGroupTheyJoinedTest_StudyGroupUpdated()
        {
            // Arrange
            var user = users.First(u => u.UserId == userIdAllowedToLeave);
            var expectedStudyGroup = studyGroups.First(sg => sg.StudyGroupId == studyGroupIdAllowedToLeaveUser);
            expectedStudyGroup.RemoveUser(user);
            var expectedStatusCode = new OkResult().StatusCode;

            // Act
            var response = studyGroupController.LeaveStudyGroup(studyGroupIdAllowedToLeaveUser, userIdAllowedToLeave).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;
            var actualStudyGroup = studyGroupRepository.GetStudyGroup(expectedStudyGroup.StudyGroupId).Result;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
            CompareStudyGroups(expectedStudyGroup, actualStudyGroup);
        }

        [Test]
        public void LeavingUserFromStudyGroupThatTheyDidNotJoinTest_NotFoundResponse()
        {
            // Arrange
            var user = users.First();
            var expectedStudyGroup = studyGroups.Last();
            var expectedStatusCode = new NotFoundResult().StatusCode;

            // Act
            var response = studyGroupController.LeaveStudyGroup(expectedStudyGroup.StudyGroupId, user.UserId).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
        }

        [Test]
        public void LeavingUserFromStudyGroupThatDoesNotExistTest_BadRequestResponse()
        {
            // Arrange
            var user = users.First();
            var expectedStudyGroup = GenerateStudyGroup(DateTime.UtcNow);
            var expectedStatusCode = new BadRequestResult().StatusCode;

            // Act
            var response = studyGroupController.LeaveStudyGroup(expectedStudyGroup.StudyGroupId, user.UserId).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
        }

        [Test]
        public void LeavingNonExistentUserFromStudyGroupTest_BadRequestResponse()
        {
            // Arrange
            var user = GenerateUser();
            var expectedStudyGroup = studyGroups.First();
            var expectedStatusCode = new BadRequestResult().StatusCode;

            // Act
            var response = studyGroupController.LeaveStudyGroup(expectedStudyGroup.StudyGroupId, user.UserId).Result;
            var actualStatusCode = ((IStatusCodeActionResult)response).StatusCode;

            // Assert
            Assert.AreEqual(expectedStatusCode, actualStatusCode);
        }

        #region Support methods

        /// <summary>
        /// Getting next free Study Group Id.
        /// </summary>
        /// <returns>Id.</returns>
        private int GetNextStudyGroupId()
        {
            return studyGroups.Last().StudyGroupId + 1;
        }

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
        /// <param name="dateTime">Creation date and time.</param>
        /// <param name="name">Name of Study Group.</param>
        /// <param name="users">Users that are in Study Group.</param>
        /// <returns><see cref="StudyGroup"/> object.</returns>
        private StudyGroup GenerateStudyGroup(DateTime dateTime, string name = null, List<User> users = null)
        {
            var studyGroupId = GetNextStudyGroupId();
            var subject = subjectValues[freeSubjectId];
            name = name == null ? GenerateStudyGroupName(subject) : name;
            return new StudyGroup(studyGroupId, name, subject, dateTime, users);
        }

        /// <summary>
        /// Generating a User object.
        /// </summary>
        /// <returns><see cref="User"/> object.</returns>
        private User GenerateUser()
        {
            var userId = users.Last().UserId + 1;
            var names = new List<string> { "John", "Miguel", "Mike", "Manuel", "Anton" };
            var userName = names[random.Next(names.Count)];

            return new User(userId, userName);
        }

        /// <summary>
        /// Full comparison of two StudyGroup objects.
        /// </summary>
        /// <param name="expectedStudyGroup">First StudyGroup object.</param>
        /// <param name="actualStudyGroup">Second StudyGroup object.</param>
        private void CompareStudyGroups(StudyGroup expectedStudyGroup, StudyGroup actualStudyGroup)
        {
            Assert.AreEqual(expectedStudyGroup.StudyGroupId, actualStudyGroup.StudyGroupId);
            Assert.AreEqual(expectedStudyGroup.Name, actualStudyGroup.Name);
            Assert.AreEqual(expectedStudyGroup.Subject, actualStudyGroup.Subject);
            Assert.AreEqual(expectedStudyGroup.CreateDate, actualStudyGroup.CreateDate);

            if (expectedStudyGroup.Users != null)
            {
                Assert.AreEqual(expectedStudyGroup.Users.Count, actualStudyGroup.Users.Count);

                for (int i = 0; i < expectedStudyGroup.Users.Count; i++)
                {
                    Assert.AreEqual(expectedStudyGroup.Users[i], actualStudyGroup.Users[i]);
                }
            }
        }
        #endregion

        [TearDown]
        public void TearDown()
        {
            if (studyGroups.Last().Subject == subjectValues[freeSubjectId])
            {
                studyGroupRepository.DeleteStudyGroup(studyGroups.Last().StudyGroupId);
                studyGroups.Remove(studyGroups.Last());
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            for (int i = 0; i < studyGroups.Count; i++)
            {
                studyGroupRepository.DeleteStudyGroup(studyGroups[i].StudyGroupId);
            }
            for (int i = 0; i < users.Count; i++)
            {
                userRepository.DeleteUser(users[i].UserId);
            }
        }
    }
}