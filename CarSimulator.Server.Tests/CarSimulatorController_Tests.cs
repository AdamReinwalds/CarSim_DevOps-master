using APIServiceLibrary.DTO;
using APIServiceLibrary.Services;
using CarSimulator.Server.Controllers;
using CarSimulator.Server.Factories;
using CarSimulator.Server.Models;
using CarSimulator.Server.Models.ViewModels;
using DataLogicLibrary.DTO;
using DataLogicLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarSimulator.Server.Tests;

public class CarSimulatorController_Tests
{
    [Fact]
    public async Task InputSeven_Should_RedirectToIndex()
    {
        //arrange
        var controller = new CarSimulatorController(null!, null!, null!, null!, null!, null!);
        var viewModel = new SimulationViewModel { SelectedAction = 7 };
        //act
        var result = await controller.Index(viewModel);
        //assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Index_Should_AssignDriver_WhenDriverIsNull()
    {
        //arrange
        var apiServiceMock = new Mock<IAPIService>();
        var driverFactoryMock = new Mock<IDriverFactory>();

        var fakeDto = new ResultsDTO
        {
            Results = new List<ResultDTO>
            {
                new ResultDTO
                {
                    Name = new NameDTO { Title = "Mr", First = "John", Last = "Doe" },
                    Location = new LocationDTO { City = "Stockholm", Country = "Sweden"  }
                }
            }
        };

        var expectedDriver = new Driver
        {
            Title = "Mr",
            First = "John",
            Last = "Doe",
            City = "Stockholm",
            Country = "Sweden"

        };


        apiServiceMock.Setup(api => api.GetOneDriver()).ReturnsAsync(fakeDto);
        driverFactoryMock.Setup(factory => factory.CreateDriver(fakeDto)).Returns(expectedDriver);



        var controller = new CarSimulatorController(apiServiceMock.Object, null!, null!, driverFactoryMock.Object, null!, null!);

        var viewModel = new SimulationViewModel { Driver = null! };

        //act
        var result = await controller.Index(viewModel);


        //assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<SimulationViewModel>(viewResult.Model);

        Assert.NotNull(returnedModel.Driver);
        Assert.Equal("John", returnedModel.Driver.First);
        Assert.Equal("Doe", returnedModel.Driver.Last);
        Assert.Equal("Mr", returnedModel.Driver.Title);
        Assert.Equal("Stockholm", returnedModel.Driver.City);
        Assert.Equal("Sweden", returnedModel.Driver.Country);

        apiServiceMock.Verify(api => api.GetOneDriver(), Times.Once);
        driverFactoryMock.Verify(factory => factory.CreateDriver(fakeDto), Times.Once);

    }

    [Fact]
    public async Task WhenSelcetedActionPerformed_StatusValuesShouldDecrease()
    {
        //arrange
        var simulationLogicServiceMock = new Mock<ISimulationLogicService>();
        var statusMessageServiceMock = new Mock<IStatusMessageService>();

        statusMessageServiceMock.Setup(s => s.GetCurrentActionMessage(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns("Action performed");
        statusMessageServiceMock.Setup(s => s.GetDriverStatusMessage(It.IsAny<int>(), It.IsAny<string>())).Returns("green");
        statusMessageServiceMock.Setup(s => s.GetCarStatusMessage(It.IsAny<int>())).Returns("Action performed");

        var initialStatus = new StatusDTO
        {
            GasValue = 20,
            EnergyValue = 20
        };

        var decreasedStatus = new StatusDTO
        {
            GasValue = 15,
            EnergyValue = 15
        };
        simulationLogicServiceMock.Setup(s => s.DecreaseStatusValues(3, initialStatus)).Returns(decreasedStatus);
        simulationLogicServiceMock.Setup(s => s.PerformAction(3, decreasedStatus)).Returns(decreasedStatus);



        var controller = new CarSimulatorController(null!, simulationLogicServiceMock.Object, null!, null!, null!, statusMessageServiceMock.Object);
        var viewModel = new SimulationViewModel { Driver = new Driver { First = "Adam"}, IsRunning = true, SelectedAction = 3, Car = new Car { }, CurrentStatus = initialStatus };


        //act

        var result = await controller.Index(viewModel);

        //assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<SimulationViewModel>(viewResult.Model);

        Assert.NotNull(returnedModel.CurrentStatus);
        Assert.Equal(15, returnedModel.CurrentStatus.GasValue);
        Assert.Equal(15, returnedModel.CurrentStatus.EnergyValue);
        simulationLogicServiceMock.Verify(s => s.DecreaseStatusValues(3, initialStatus), Times.Once);
        simulationLogicServiceMock.Verify(s => s.PerformAction(3, decreasedStatus), Times.Once);
    }

}
