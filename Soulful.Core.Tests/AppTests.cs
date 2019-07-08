using System;
using Xunit;

namespace Soulful.Core.Tests
{
    public class AppTests
    {
        [Fact]
        public void TestGenerateError()
        {
            const string message = "test";
            ArgumentException exception = new ArgumentException();

            Exception generated = App.CreateError<Exception>(message, false);
            Assert.IsType<Exception>(generated);
            Assert.Equal(message, generated.Message);

            generated = App.LogError(message, exception, false);
            Assert.IsType<ArgumentException>(generated);
        }
    }
}
