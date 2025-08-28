using DataLogicLibrary.DTO;
using DataLogicLibrary.Services;
using DataLogicLibrary.Infrastructure.Enums;
using DataLogicLibrary.DirectionStrategies.Interfaces;
using Moq;

namespace DataLogicLibrary.Tests;

public class SimulationLogicService_Tests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void PerformAction_ShouldCallCorrectStrategy(int input)
    {
        // Arrange
        var context = new Mock<IDirectionContext>();
        var left = new Mock<IDirectionStrategy>();
        var right = new Mock<IDirectionStrategy>();
        var forward = new Mock<IDirectionStrategy>();
        var backward = new Mock<IDirectionStrategy>();

        left.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
        right.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
        forward.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
        backward.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);

        SimulationLogicService.DirectionStrategyResolver resolver = action =>
            action switch
            {
                MovementAction.Left => left.Object,
                MovementAction.Right => right.Object,
                MovementAction.Forward => forward.Object,
                MovementAction.Backward => backward.Object,
                _ => throw new ArgumentOutOfRangeException(nameof(action), $"Not expected direction value: {action}"),
            };
        var logicService = new SimulationLogicService(context.Object, resolver);

        var status = new StatusDTO
        {
            CardinalDirection = CardinalDirection.North,
            EnergyValue = 10,
            GasValue = 10,
        };
        // Act
        logicService.PerformAction(input, status);

        // Assert
        switch(input) 
        {
            case 1: context.Verify(c => c.SetStrategy(left.Object), Times.Once); break;
            case 2: context.Verify(c => c.SetStrategy(right.Object), Times.Once); break;
            case 3: context.Verify(c => c.SetStrategy(forward.Object), Times.Once); break;
            case 4: context.Verify(c => c.SetStrategy(backward.Object), Times.Once); break;
        }
    }

    [Theory]
    [InlineData(5, 20, 10)]
    [InlineData(6, 10, 20)]
    public void PerformAction_ShouldIncreaseValues(int input, int expectedEnergy, int expectedGas)
    {
        //Arrange
        var mockStrategy = new Mock<IDirectionStrategy>();
        mockStrategy.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
        SimulationLogicService.DirectionStrategyResolver resolver = action => mockStrategy.Object;



        var service = new SimulationLogicService(new Mock<IDirectionContext>().Object, resolver);
        var status = new StatusDTO { EnergyValue = 10, GasValue = 10 };

        //Act
        var result = service.PerformAction(input, status);

        //Assert
        Assert.Equal(expectedEnergy, result.EnergyValue);
        Assert.Equal(expectedGas, result.GasValue);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    
    public void DecreaseStatusValues_ShouldProperlyDecreaseValues(int input)
    {
        //Arrange
        var mockStatus = new StatusDTO { EnergyValue = 10, GasValue = 10 };
        var mockStrategy = new Mock<IDirectionStrategy>();
        mockStrategy.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
        SimulationLogicService.DirectionStrategyResolver resolver = action => mockStrategy.Object;
        var service = new SimulationLogicService(new Mock<IDirectionContext>().Object, resolver);


        //Act
        var result = service.DecreaseStatusValues(input, mockStatus);

        //Assert
        Assert.InRange(result.EnergyValue, 5, 9);
        Assert.True(result.EnergyValue >= 0);
        Assert.True(result.GasValue >= 0);

        if (input == 5)
        {
            Assert.Equal(10, result.GasValue);

        }
        else
        {
            Assert.InRange(result.GasValue, 5, 9);
        }
    }
}
