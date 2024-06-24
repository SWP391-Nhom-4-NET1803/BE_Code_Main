using ClinicPlatformRepositories.Contracts;
using Moq;

namespace TestProject
{
    [TestFixture]
    public class RepositoryTesting
    {

        private Mock<IUserRepository> userRepository;
        //private Mock<IClinicRepository> clinicRepository;

        [SetUp]
        public void Setup()
        {
            userRepository = new Mock<IUserRepository>();
        }

        

        [TearDown]
        public void Teardown() 
        {
            userRepository.Reset();

        }

        [Test]
        public void Test1()
        {
            if (userRepository.Object != null)
            {
                int row = userRepository.Object.GetAllUser().Count();
                TestContext.WriteLine(row);

                Assert.Pass();
            }
            else { Assert.Fail("Can not create object for testing."); }

            
        }
    }
}