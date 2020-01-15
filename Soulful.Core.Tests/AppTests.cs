using System;
using System.Linq;
using Xunit;

namespace Soulful.Core.Tests
{
    public class AppTests
    {
        [Fact]
        public void TestCreateError()
        {
            const string message = "test";
            Exception generated = App.CreateError<Exception>(message, false);
            Assert.IsType<Exception>(generated);
            Assert.Equal(message, generated.Message);
        }

        [Fact]
        public void TestLogError()
        {
            const string message = "test";
            ArgumentException exception = new ArgumentException();

            Exception generated = App.LogError(message, exception, false);
            Assert.IsType<ArgumentException>(generated);
        }

        [Fact]
        public void TestGetPlatformAppdataPath()
        {
            Assert.False(string.IsNullOrEmpty(App.GetPlatformAppdataPath()));
        }

        [Fact]
        public void TestGetAppdataFilePath()
        {
            const string fileName = "test.txt";
            string path = App.GetAppdataFilePath(fileName);
            Assert.Equal(fileName, path.Split('\\').Last());
        }
    }
}
