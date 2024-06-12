using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.SharedKernel;

public class LocationShould
{
     [Fact]
    public void BeCorrectWhenParamsIsCorrectOnCreated()
    {
       //Arrange

        //Act
        var location = Location.Create(10,10);

        //Assert
        location.IsSuccess.Should().BeTrue();
        location.Value.Value.Should().Be(10,10);
    }    
}    