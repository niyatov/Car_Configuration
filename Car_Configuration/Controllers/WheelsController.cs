using Car_Configuration.Data;
using Car_Configuration.Dtoes;
using Car_Configuration.Entities;
using Car_Configuration.Exceptions;
using Car_Configuration.Models;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Car_Configuration.Controllers;

[Authorize(Roles = "admin,manager")]
public class WheelsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IValidator<UpdateWheelDto> _updateWheelDtoValidator;
    private IValidator<CreateWheelDto> _createWheelDtoValidator;

    public WheelsController(AppDbContext context,
         IValidator<CreateWheelDto> createWheelDtoValidator,
         IValidator<UpdateWheelDto> updateWheelDtoValidator)
    {
        _context = context;
        _createWheelDtoValidator = createWheelDtoValidator;
        _updateWheelDtoValidator = updateWheelDtoValidator;

    }

    public IActionResult CreateWheel() => View();

    [HttpPost]
    public async Task<IActionResult> CreateWheel(CreateWheelDto createWheelDto, IFormFile? file)
    {
        var result = _createWheelDtoValidator.Validate(createWheelDto);

        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View();
        }

        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "file required");
            return View();
        }

        var model = await _context.Models.FirstOrDefaultAsync(x => x.Name == createWheelDto.ModelName);

        if (model == null)
        {
            ModelState.AddModelError("", "model not found");
            return View();
        }

        if ((model.Wheels?.Any(x => x.Name == createWheelDto.Name)) == true)
        {
            ModelState.AddModelError("", "name exists");
            return View();
        }

        var wheel = new Wheel();
        wheel.Name = createWheelDto.Name;
        wheel.ModelId = model.Id;
        string imageName = Guid.NewGuid().ToString();
        var filePath = Path.Combine($"images/{model.FolderPath}", $"{imageName}.jpg");

        using (var stream = new FileStream("wwwroot/" + filePath, FileMode.Create))
        {
            file.CopyTo(stream);


        }

        wheel.ImagePath = filePath;

        await _context.Wheels.AddAsync(wheel);
        await _context.SaveChangesAsync();

        return Redirect($"GetWheel?wheelId={wheel.Id}");
    }


    public async Task<IActionResult> GetWheels()
    {
        var wheels = await _context.Wheels.ToListAsync();
        var wheelsVM = new List<GetWheelsVM>();

        if (wheels is null)
            throw new NotFoundException<Wheel>();

        foreach (var wheel in wheels)
        {
            var wheelVM = new GetWheelsVM();
            wheelVM.Id = wheel.Id;
            wheelVM.ModelName = wheel.Model.Name;
            wheelVM.Name = wheel.Name;

            wheelsVM.Add(wheelVM);
        }

        return View(wheelsVM);
    }


    public async Task<IActionResult> GetWheel(int wheelId)
    {
        var wheel = await _context.Wheels.FirstOrDefaultAsync(x => x.Id == wheelId);

        if (wheel is null)
            throw new NotFoundException<Wheel>();

        return View(wheel.Adapt<GetWheelVM>());
    }


    [HttpGet]
    public async Task<IActionResult> UpdateWheel(int wheelId, string? error)
    {
        var wheel = await _context.Wheels.FirstOrDefaultAsync(x => x.Id == wheelId);
        ViewData["wheelId"] = wheelId;

        if (wheel is null)
        {
            ModelState.AddModelError("", "not found");
            return View();
        }

        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View();
        }

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> UpdateWheel(int wheelId, UpdateWheelDto updateWheelDto, IFormFile? file)
    {
        var result = _updateWheelDtoValidator.Validate(updateWheelDto);

        if (!result.IsValid)
        {
            var errors = "";
            for (int i = 0; i < result.Errors.Count(); i++)
            {
                errors += $"   {i + 1}) " + $"{result.Errors[i].PropertyName}" + " : " + $"{result.Errors[i].ErrorMessage}";
            }
            return Redirect($"UpdateWheel?error={errors}&&wheelId={wheelId}");
        }

        var wheel = await _context.Wheels.FirstAsync(x => x.Id == wheelId);

        if ((wheel.Model.Wheels?.Any(x => x.Name == updateWheelDto.Name)) == true)
        {
            ModelState.AddModelError("", "name exists");
            return View();
        }

        if (file != null && file.Length > 0)
        {

            if (System.IO.File.Exists("wwwroot/" + wheel.ImagePath))
            {
                System.IO.File.Delete("wwwroot/" + wheel.ImagePath);
            }

            using (var stream = new FileStream("wwwroot/" + wheel.ImagePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
        }

        wheel.Name = updateWheelDto.Name;

        _context.Wheels.Update(wheel);
        await _context.SaveChangesAsync();

        return LocalRedirect($"/Wheels/GetWheel?wheelId={wheelId}");
    }


    public async Task<IActionResult> DeleteWheel(int wheelId)
    {
        var wheel = _context.Wheels.FirstOrDefault(x => x.Id == wheelId);

        if (wheel is null)
            throw new NotFoundException<Wheel>();

        if (System.IO.File.Exists("wwwroot/" + wheel.ImagePath))
        {
            System.IO.File.Delete("wwwroot/" + wheel.ImagePath);
        }
        _context.Wheels.Remove(wheel);
        await _context.SaveChangesAsync();

        return RedirectToAction("GetWheels");
    }
}
