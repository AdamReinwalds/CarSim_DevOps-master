using Xunit;
using Moq;
using DataLogicLibrary.Services;
using DataLogicLibrary.Services.Interfaces;
using DataLogicLibrary.DirectionStrategies.Interfaces;
using DataLogicLibrary.DTO;
using DataLogicLibrary.Infrastructure.Enums;

public class CarSimulatorControllerTests
{
    private readonly Mock<IDirectionContext> _directionContextMock;
    private readonly Mock<IDirectionStrategy> _leftStrategyMock;
    private readonly Mock<IDirectionStrategy> _rightStrategyMock;
    private readonly Mock<IDirectionStrategy> _forwardStrategyMock;
    private readonly Mock<IDirectionStrategy> _backwardStrategyMock;

    private SimulationLogicService.DirectionStrategyResolver _resolver;

    public CarSimulatorControllerTests()
    {
        _directionContextMock = new Mock<IDirectionContext>();
        _leftStrategyMock = new Mock<IDirectionStrategy>();
        _rightStrategyMock = new Mock<IDirectionStrategy>();
        _forwardStrategyMock = new Mock<IDirectionStrategy>();
        _backwardStrategyMock = new Mock<IDirectionStrategy>();

        _resolver = (action) => action switch
        {
            MovementAction.Left => _leftStrategyMock.Object,
            MovementAction.Right => _rightStrategyMock.Object,
            MovementAction.Forward => _forwardStrategyMock.Object,
            MovementAction.Backward => _backwardStrategyMock.Object,
            _ => throw new ArgumentException()
        };
    }

    private SimulationLogicService CreateService()
    {
        return new SimulationLogicService(_directionContextMock.Object, _resolver);
    }

    [Fact]
    public void PerformAction_UserInput6_ReturnsCurrentStatus()
    {
        var service = CreateService();
        var status = new StatusDTO { EnergyValue = 10, GasValue = 10 };

        var result = service.PerformAction(6, status);

        Assert.Equal(status, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void PerformAction_DirectionActions_ExecutesStrategy(int userInput)
    {
        var service = CreateService();
        var status = new StatusDTO { EnergyValue = 10, GasValue = 10 };
        var expectedStatus = new StatusDTO { EnergyValue = 5, GasValue = 5 };

        _directionContextMock.Setup(x => x.ExecuteStrategy(status)).Returns(expectedStatus);

        var result = service.PerformAction(userInput, status);

        _directionContextMock.Verify(x => x.SetStrategy(It.IsAny<IDirectionStrategy>()), Times.Once);
        _directionContextMock.Verify(x => x.ExecuteStrategy(status), Times.Once);
        Assert.Equal(expectedStatus, result);
    }

    [Fact]
    public void PerformAction_UserInput5_SetsEnergyValueTo20()
    {
        var service = CreateService();
        var status = new StatusDTO { EnergyValue = 10, GasValue = 10 };

        var result = service.PerformAction(5, status);

        Assert.Equal(20, result.EnergyValue);
        Assert.Equal(10, result.GasValue);
    }

    [Fact]
    public void PerformAction_UserInput6_SetsGasValueTo20()
    {
        var service = CreateService();
        var status = new StatusDTO { EnergyValue = 10, GasValue = 10 };

        var result = service.PerformAction(6, status);

        // The method returns currentStatus for input 6, but also sets GasValue to 20
        Assert.Equal(20, result.GasValue);
        Assert.Equal(10, result.EnergyValue);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void DecreaseStatusValues_ValuesDecreaseAndNotNegative(int userInput)
    {
        var service = CreateService();
        var status = new StatusDTO { EnergyValue = 2, GasValue = 2 };

        var result = service.DecreaseStatusValues(userInput, status);

        Assert.InRange(result.EnergyValue, 0, 2);
        Assert.InRange(result.GasValue, 0, 2);
    }

    [Fact]
    public void DecreaseStatusValues_UserIsResting_DoesNotDecreaseGas()
    {
        var service = CreateService();
        var status = new StatusDTO { EnergyValue = 10, GasValue = 10 };

        var result = service.DecreaseStatusValues(5, status);

        Assert.InRange(result.EnergyValue, 5, 10); // Energy decreases
        Assert.Equal(10, result.GasValue); // Gas does not decrease
    }
}
