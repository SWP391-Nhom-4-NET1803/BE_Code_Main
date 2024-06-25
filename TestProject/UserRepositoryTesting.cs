using ClinicPlatformDTOs.UserModels;
using ClinicPlatformRepositories;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace TestProject
{
    [TestFixture]
    public class UserRepositoryTesting
    {
        private Mock<IUserRepository> userRepository;

        public UserRepositoryTesting()
        {
        }

        [SetUp]
        public void SetUpTest()
        {
            List<UserInfoModel> MockDatabase = new List<UserInfoModel>()
            { 
                new UserInfoModel() {Id = 1, Username = "Example1", PasswordHash="3JCFCWE33", Salt="Uok334fe", Email="G@gmail.com" },
                new UserInfoModel() {Id = 2, Username = "Example2", PasswordHash="Haerufe2424gt", Salt="Uwwe23ke", Email="F@gmail.com" },
                new UserInfoModel() {Id = 3, Username = "Example3", PasswordHash="134CFawef2CW424477", Salt="sdwvw", Email="M@gmail.com" },
                new UserInfoModel() {Id = 4, Username = "Example4", PasswordHash="bbEC42rfff34342", Salt="56keGG", Email="D@gmail.com" },
            };

            userRepository = new Mock<IUserRepository>();

            userRepository.Setup(x => x.GetAllUser(true, true))
                .Returns(MockDatabase);

            userRepository.Setup(x => x.GetUser(It.IsAny<int>()))
                .Returns<int>((int id) => { return MockDatabase.Where(x => x.Id == id).FirstOrDefault(); });

                userRepository.Setup(x => x.GetUserWithEmail(It.IsAny<string>()))
                .Returns<string>((string id) => { return MockDatabase.Where(x => x.Email == id).FirstOrDefault(); });

            userRepository.Setup(x => x.AddUser(It.IsAny<UserInfoModel>()))
                .Returns<UserInfoModel>((x) => { x.Id = MockDatabase.Count() + 1; MockDatabase.Add(x); return x; });
        }

        [TearDown]
        public void CleanTest()
        {
            userRepository.Reset();
        }

        [Test]
        public void AddNewUser()
        {
            UserService userService = new UserService(userRepository.Object);

            UserInfoModel? user = new UserInfoModel()
            {
                Username = "J0shTheBestManager",
                PasswordHash = "Hang0ver4tTh3Party",
                Email = "Joshua@gmail.com",
                Role = "Customer",
            };
            user = userService.RegisterAccount(user, "Customer", out var message);

            TestContext.WriteLine($"Message: {message}");

            if (user != null)
            {
                var afterUser = userService.GetUserWithUserId(user.Id);

                Assert.IsTrue(afterUser != null);
            }
            else
            {
                Assert.Fail("User was not created");
            }
        }

        [Test]
        public void GetUser()
        {
            UserService userService = new UserService(userRepository.Object);

            UserInfoModel? user = userService.GetUserWithUserId(3);

            if (user != null)
            {
                Assert.IsTrue(user.Username == "Example3");
            }
            else
            {
                Assert.Fail("User was not found");
            }
        }

        [Test]
        public void FindUserWithEmail()
        {
            UserService userService = new UserService(userRepository.Object);

            UserInfoModel? user = userService.GetUserWithEmail("M@gmail.com");

            Assert.NotNull(user);
        }
    }
}