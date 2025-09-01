using DataLogicLibrary.DTO;
using DataLogicLibrary.Services;
using DataLogicLibrary.Infrastructure.Enums;
using DataLogicLibrary.DirectionStrategies.Interfaces;
using Moq;
using static DataLogicLibrary.Services.SimulationLogicService;
using DataLogicLibrary.DirectionStrategies;

namespace DataLogicLibrary.Tests.Services;

public class SimulationLogicService_Tests 
{

    private SimulationLogicService _sut;
    private DirectionContext directionContext;
    private DirectionStrategyResolver directionStrategyResolver;

    public SimulationLogicService_Tests()
    {
        directionContext = new DirectionContext();
        directionStrategyResolver = movementAction =>
        {
            switch (movementAction)
            {
                case MovementAction.Left:
                    return new TurnLeftStrategy();
                case MovementAction.Right:
                    return new TurnRightStrategy();
                case MovementAction.Forward:
                    return new DriveForwardStrategy();
                case MovementAction.Backward:
                    return new ReverseStrategy();
                default:
                    throw new KeyNotFoundException();

            }
        };
        _sut = new SimulationLogicService(directionContext, directionStrategyResolver);
    }
    [Fact]
    public void Forward_Action_CardinalDirection_Remains_North_When_Previous_Movement_Is_Not_Backward()
    {
        // Arrange
        var status = new StatusDTO
        {
            CardinalDirection = CardinalDirection.North,
            EnergyValue = 20,
            GasValue = 20,
            MovementAction = MovementAction.Left
        };
        var userInputForward = 3;
        var expectedDirection = CardinalDirection.North;
        // Act
        var result = _sut.PerformAction(userInputForward, status); // 3 corresponds to Forward action
        // Assert
        Assert.Equal(expectedDirection, result.CardinalDirection);
    }
    [Fact]
    public void Right_Action_CardinalDirection_Changes_To_East_When_Previous_CardinalDirection_Was_North_And_Movement_Is_Not_Backward()
    {
        //Arrange
        var status = new StatusDTO
        {
            CardinalDirection = CardinalDirection.North,
            EnergyValue = 20,
            GasValue = 20,
            MovementAction = MovementAction.Right
        };
        var userInputLeft = 2;
        var expectedDirection = CardinalDirection.East;
        //Act
        var result = _sut.PerformAction(userInputLeft, status);
        //Assert
        Assert.Equal(expectedDirection, result.CardinalDirection);
    }
    [Fact]
    public void Left_Action_CardinalDirection_Changes_To_West_When_Previous_CardinalDirection_Was_North_And_Movement_Is_Not_Backward()
    {
        //Arrange
        var status = new StatusDTO
        {
            CardinalDirection = CardinalDirection.North,
            EnergyValue = 20,
            GasValue = 20,
            MovementAction = MovementAction.Right
        };
        var userInputLeft = 1;
        var expectedDirection = CardinalDirection.West;
        //Act
        var result = _sut.PerformAction(userInputLeft, status);
        //Assert
        Assert.Equal(expectedDirection, result.CardinalDirection);
    }

    [Fact]
    public void Reverse_Action_CardinalDirection_Changes_To_South_When_Previous_CardinalDirection_Was_North_And_Movement_Is_Not_Backward()
    {
        //Arrange
        var status = new StatusDTO
        {
            CardinalDirection = CardinalDirection.North,
            EnergyValue = 20,
            GasValue = 20,
            MovementAction = MovementAction.Right
        };
        var userInputLeft = 4;
        var expectedDirection = CardinalDirection.South;
        //Act
        var result = _sut.PerformAction(userInputLeft, status);
        //Assert
        Assert.Equal(expectedDirection, result.CardinalDirection);
    }
    [Fact]
    public void Forward_Action_CardinalDirection_Remians_East_When_Movement_Is_Not_Backward()
    {
        //Arrange
        var status = new StatusDTO
        {
            CardinalDirection = CardinalDirection.East,
            EnergyValue = 20,
            GasValue = 20,
            MovementAction = MovementAction.Right
        };
        var userInputLeft = 3;
        var expectedDirection = CardinalDirection.East;
        //Act
        var result = _sut.PerformAction(userInputLeft, status);
        //Assert
        Assert.Equal(expectedDirection, result.CardinalDirection);
    }
    [Fact]
    public void Right_Action_CardinalDirection_Changes_To_South_When_Previous_CardinalDirection_Was_East_And_Movement_Is_Not_Backward()
    {
        //Arrange
        var status = new StatusDTO
        {
            CardinalDirection = CardinalDirection.East,
            EnergyValue = 20,
            GasValue = 20,
            MovementAction = MovementAction.Left
        };
        var userInputLeft = 2;
        var expectedDirection = CardinalDirection.South;
        //Act
        var result = _sut.PerformAction(userInputLeft, status);
        //Assert
        Assert.Equal(expectedDirection, result.CardinalDirection);
    }
    [Fact]
    public void Reverse_Action_CardinalDirection_Changes_To_West_When_Previous_CardinalDirection_Was_East_And_Movement_Is_Not_Backward()
    {
        //Arrange
        var status = new StatusDTO
        {
            CardinalDirection = CardinalDirection.East,
            EnergyValue = 20,
            GasValue = 20,
            MovementAction = MovementAction.Left
        };
        var userInputLeft = 4;
        var expectedDirection = CardinalDirection.West;
        //Act
        var result = _sut.PerformAction(userInputLeft, status);
        //Assert
        Assert.Equal(expectedDirection, result.CardinalDirection);
    }
    [Fact]
    public void Left_Action_CardinalDirection_Changes_To_North_When_Previous_CardinalDirection_Was_East_And_Movement_Is_Not_Backward()
    {
        //Arrange
        var status = new StatusDTO
        {
            CardinalDirection = CardinalDirection.East,
            EnergyValue = 20,
            GasValue = 20,
            MovementAction = MovementAction.Left
        };
        var userInputLeft = 1;
        var expectedDirection = CardinalDirection.North;
        //Act
        var result = _sut.PerformAction(userInputLeft, status);
        //Assert
        Assert.Equal(expectedDirection, result.CardinalDirection);
    }
    [Theory]
    [InlineData(1, CardinalDirection.South, CardinalDirection.East)]
    [InlineData(2, CardinalDirection.South, CardinalDirection.West)]
    [InlineData(3, CardinalDirection.South, CardinalDirection.South)]
    [InlineData(4, CardinalDirection.South, CardinalDirection.North)]
    [InlineData(1, CardinalDirection.West, CardinalDirection.South)]
    [InlineData(2, CardinalDirection.West, CardinalDirection.North)]
    [InlineData(3, CardinalDirection.West, CardinalDirection.West)]
    [InlineData(1, CardinalDirection.West, CardinalDirection.East)]
    public void South_CardinalDirection_Actions_Should_Return_Expected_Direction(int input, CardinalDirection previousDirection, CardinalDirection expectedDirection)
    {
        //Arrange
        var currentStatus = new StatusDTO
        {
            CardinalDirection = previousDirection,
            EnergyValue = 20,
            GasValue = 20,
            MovementAction = MovementAction.Left
        };
        //Act
        var result = _sut.PerformAction(input, currentStatus);

        //Assert
        Assert.Equal(expectedDirection, result.CardinalDirection);
    }



    //    [Theory]
    //[InlineData(1)]
    //[InlineData(2)]
    //[InlineData(3)]
    //[InlineData(4)]
    //public void PerformAction_ShouldCallCorrectStrategy(int input)
    //{
    //    Arrange
    //   var context = new Mock<IDirectionContext>();
    //    var left = new Mock<IDirectionStrategy>();
    //    var right = new Mock<IDirectionStrategy>();
    //    var forward = new Mock<IDirectionStrategy>();
    //    var backward = new Mock<IDirectionStrategy>();

    //    left.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
    //    right.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
    //    forward.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
    //    backward.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);

    //    SimulationLogicService.DirectionStrategyResolver resolver = action =>
    //        action switch
    //        {
    //            MovementAction.Left => left.Object,
    //            MovementAction.Right => right.Object,
    //            MovementAction.Forward => forward.Object,
    //            MovementAction.Backward => backward.Object,
    //            _ => throw new ArgumentOutOfRangeException(nameof(action), $"Not expected direction value: {action}"),
    //        };
    //    var logicService = new SimulationLogicService(context.Object, resolver);

    //    var status = new StatusDTO
    //    {
    //        CardinalDirection = CardinalDirection.North,
    //        EnergyValue = 10,
    //        GasValue = 10,
    //    };
    //    Act
    //    logicService.PerformAction(input, status);

    //    Assert
    //    switch (input)
    //    {
    //        case 1: context.Verify(c => c.SetStrategy(left.Object), Times.Once); break;
    //        case 2: context.Verify(c => c.SetStrategy(right.Object), Times.Once); break;
    //        case 3: context.Verify(c => c.SetStrategy(forward.Object), Times.Once); break;
    //        case 4: context.Verify(c => c.SetStrategy(backward.Object), Times.Once); break;
    //    }
    //}

    //[Theory]
    //[InlineData(5, 20, 10)]
    //[InlineData(6, 10, 20)]
    //public void PerformAction_ShouldIncreaseValues(int input, int expectedEnergy, int expectedGas)
    //{
    //    //Arrange
    //    var mockStrategy = new Mock<IDirectionStrategy>();
    //    mockStrategy.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
    //    SimulationLogicService.DirectionStrategyResolver resolver = action => mockStrategy.Object;



    //    var service = new SimulationLogicService(new Mock<IDirectionContext>().Object, resolver);
    //    var status = new StatusDTO { EnergyValue = 10, GasValue = 10 };

    //    //Act
    //    var result = service.PerformAction(input, status);

    //    //Assert
    //    Assert.Equal(expectedEnergy, result.EnergyValue);
    //    Assert.Equal(expectedGas, result.GasValue);
    //}

    //[Theory]
    //[InlineData(1, 0, 0)]
    //[InlineData(1, 10, 10)]
    //[InlineData(2, 10, 10)]
    //[InlineData(3, 20, 20)]
    //[InlineData(4, 10, 10)]
    //[InlineData(4, -12, -12)]
    //[InlineData(5, 10, 10)]
    //[InlineData(5, -2, -2)]
    //[InlineData(6, 10, 10)]
    //[InlineData(6, -5, -5)]


    //public void DecreaseStatusValues_ShouldProperlyDecreaseValues(int input, int energyValue, int gasValue)
    //{
    //    //Arrange
    //    var mockStatus = new StatusDTO { EnergyValue = energyValue, GasValue = gasValue};
    //    var mockStrategy = new Mock<IDirectionStrategy>();
    //    mockStrategy.Setup(s => s.Execute(It.IsAny<StatusDTO>())).Returns<StatusDTO>(status => status);
    //    SimulationLogicService.DirectionStrategyResolver resolver = action => mockStrategy.Object;
    //    var service = new SimulationLogicService(new Mock<IDirectionContext>().Object, resolver);

    //    int maxExpectedGasValue = Math.Max(0, gasValue - 1);
    //    int minExpectedGasValue = Math.Max(0, gasValue - 5);

    //    int maxExpectedEnergyValue = Math.Max(0, energyValue - 1);
    //    int minExpectedEnergyValue = Math.Max(0, energyValue - 5);
    //    if (gasValue < 0)
    //        gasValue = 0;

    //    //Act
    //    var result = service.DecreaseStatusValues(input, mockStatus);

    //    //Assert
    //    Assert.InRange(result.EnergyValue, minExpectedEnergyValue, maxExpectedEnergyValue);
    //    Assert.True(result.EnergyValue >= 0);
    //    Assert.True(result.GasValue >= 0);

    //    if (input == 5)
    //    {
    //        Assert.Equal(gasValue, result.GasValue);

    //    }
    //    else
    //    {
    //        Assert.InRange(result.GasValue, minExpectedGasValue, maxExpectedGasValue);
    //    }
    //}
}
