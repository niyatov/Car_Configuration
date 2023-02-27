using Car_Configuration.Data;
using Car_Configuration.Dtoes;
using Car_Configuration.Entities;
using Car_Configuration.Exceptions;
using Car_Configuration.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarConfiguration_api.Controllers;

[Authorize]
public class CarsController : Controller
{
    private readonly AppDbContext _context;

    public CarsController(AppDbContext context)
    {
        _context = context;
    }


    public IActionResult ChooseModel(string? error)
    {
        var models = _context.Models.Where(x => x.IsReady == true).ToList();
        @ViewBag.Models = models.Select(x => x.Adapt<ChooseModelDto>()).ToList();

        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View();
        }

        return View();
    }


    public IActionResult ChooseColorAndWheel(int modelId, string? error)
    {
        if (modelId == 0)
        {
            string errorString = "choose one";
            return Redirect($"ChooseModel?error={errorString}");
        }

        @ViewBag.ModelId = modelId;

        var model = _context.Models.FirstOrDefault(x => x.Id == modelId);
        if (model == null)
            throw new NotFoundException("Car not found");

        ChooseColorAndWheelDto result = new ChooseColorAndWheelDto();
        result.Wheels = model.Wheels?.Select(x => x.Adapt<GetWheelVM>()).ToList();
        result.Colors = new List<GetColorModelVM>();

        if (model.ColorModels != null)
            foreach (var color in model.ColorModels)
            {
                var res = new GetColorModelVM();
                res = color.Adapt<GetColorModelVM>();
                res.Name = color.Color.Name;
                res.ImagePath = color.Color.ImagePath;

                result.Colors.Add(res);
            }

        if (result.Wheels == null || result.Colors == null)
            return View();

        @ViewBag.ColorAndWheels = result;
        @ViewBag.CarImagePath = model.ImagePath;

        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View();
        }

        return View();
    }


    public IActionResult ChooseCar(ChooseCarDto chooseCarDto, string? error)
    {
        if (chooseCarDto.ColorId == 0)
        {
            string errorString = "choose one color";
            return Redirect($"ChooseColorAndWheel?error={errorString}&&modelId={chooseCarDto.ModelId}");
        }

        if (chooseCarDto.WheelId == 0)
        {
            string errorString = "choose one wheel";
            return Redirect($"ChooseColorAndWheel?error={errorString}&&modelId={chooseCarDto.ModelId}");
        }

        var wheelColorModel = _context.WheelColorModels.Where(x => x.ColorModelId == chooseCarDto.ColorId && x.WheelId == chooseCarDto.WheelId).FirstOrDefault();

        if (wheelColorModel == null)
        {
            ModelState.AddModelError("", "not found");
            return View();
        }

        ViewBag.ColorWheelPath = wheelColorModel.ColorWheelPath;
        ViewBag.ColorId = wheelColorModel.ColorModelId;
        ViewBag.WheelId = wheelColorModel.WheelId;
        ViewBag.ModelId = wheelColorModel.ColorModel.ModelId;
        ViewBag.ModelName = wheelColorModel.ColorModel.Model.Name;
        ViewBag.ColorName = wheelColorModel.ColorModel.Color.Name;
        ViewBag.WheelName = wheelColorModel.Wheel.Name;

        var models = _context.Models.ToList().Where(x => x.Id == chooseCarDto.ModelId);
        @ViewBag.Models = models.Select(x => x.Adapt<ChooseModelDto>()).ToList();

        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View();
        }

        return View();
    }


    public async Task<IActionResult> SaveCar(int colorId, int wheelId)
    {
        var wheelColorModel = _context.WheelColorModels.Where(x => x.ColorModelId == colorId && x.WheelId == wheelId).FirstOrDefault();

        if (wheelColorModel == null)
            throw new NotFoundException("Car not found");

        var userId = Convert.ToInt32(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        if (_context.UserWheelColors.Any(x => x.UserId == userId && x.WheelColorModelId == wheelColorModel.Id))
        {
            string error = "you already have this car";
            return Redirect($"ChooseCar?ModelId={error}&&ColorId={colorId}&&WheelId={wheelId}&&error={error}");
        }

        var userWheelColor = new UserWheelColor();
        userWheelColor.UserId = userId;
        userWheelColor.WheelColorModelId = wheelColorModel.Id;

        _context.UserWheelColors.Add(userWheelColor);
        await _context.SaveChangesAsync();

        return Redirect("GetCars");
    }


    public IActionResult GetCars()
    {
        var userId = Convert.ToInt32(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

        var userWheelColors = _context.UserWheelColors.Where(x => x.UserId == userId).ToList();
        var results = new List<GetCarsVM>();

        if (userWheelColors is not null)
            foreach (var userWheelColor in userWheelColors)
            {
                var result = new GetCarsVM();
                result.Id = userWheelColor.WheelColorModel.Id;
                result.ModelName = userWheelColor.WheelColorModel.Wheel.Model.Name;
                result.ImagePath = userWheelColor.WheelColorModel.ColorWheelPath;

                results.Add(result);
            }

        return View(results);
    }


    public async Task<IActionResult> GetCar(int carId)
    {
        var userId = Convert.ToInt32(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        var car = await _context.UserWheelColors.FirstOrDefaultAsync(x => x.WheelColorModelId == carId && x.UserId == userId);

        if (car is null)
            throw new NotFoundException("Car not found");

        var carVM = car.Adapt<GetCarVM>();
        carVM.CarId = car.WheelColorModel.Id;
        carVM.ModelName = car.WheelColorModel.Wheel.Model.Name;
        carVM.ColorName = car.WheelColorModel.ColorModel.Color.Name;
        carVM.WheelName = car.WheelColorModel.Wheel.Name;
        carVM.ImagePath = car.WheelColorModel.ColorWheelPath;

        return View(carVM);
    }


    public async Task<IActionResult> DeleteCar(int carId)
    {
        var userId = Convert.ToInt32(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        var car = _context.UserWheelColors.FirstOrDefault(x => x.WheelColorModelId == carId && x.UserId == userId);

        if (car is null)
            throw new NotFoundException("Car not found");

        _context.UserWheelColors.Remove(car);
        await _context.SaveChangesAsync();

        return RedirectToAction("GetCars");
    }
}
