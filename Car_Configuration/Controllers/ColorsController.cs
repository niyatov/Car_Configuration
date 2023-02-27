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
public partial class ColorsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateColorDto> _createColorDtoValidator;
    private readonly IValidator<UpdateColorDto> _updateColorDtoValidator;
    private readonly IValidator<CreateColorModelDto> _createColorModelDtoValidator;

    public ColorsController(AppDbContext context,
        IValidator<CreateColorDto> createColorDtoValidator,
        IValidator<UpdateColorDto> updateColorDtoValidator,
        IValidator<CreateColorModelDto> createColorModelDtoValidator)
    {
        _context = context;
        _createColorDtoValidator = createColorDtoValidator;
        _updateColorDtoValidator = updateColorDtoValidator;
        _createColorModelDtoValidator = createColorModelDtoValidator;
    }


    public IActionResult CreateColor() => View();

    [HttpPost]
    public async Task<IActionResult> CreateColor(CreateColorDto createColorDto, IFormFile? file)
    {
        var result = _createColorDtoValidator.Validate(createColorDto);

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

        if ((_context.Colors.Any(x => x.Name == createColorDto.Name)) == true)
        {
            ModelState.AddModelError("Name", "name exists");
            return View();
        }

        var color = new Color();
        color.Name = createColorDto.Name;
        string imageName = Guid.NewGuid().ToString("N");
        var filePath = Path.Combine("images", $"{imageName}.jpg");

        using (var stream = new FileStream("wwwroot/" + filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        color.ImagePath = filePath;

        await _context.Colors.AddAsync(color);
        await _context.SaveChangesAsync();

        return Redirect($"GetColor?colorId={color.Id}");
    }



    public async Task<IActionResult> GetColors()
    {
        var colors = await _context.Colors.ToListAsync();
        var colorsVN = new List<GetColorsVM>();

        if (colors is null)
            return View(null);

        foreach (var color in colors)
        {
            var colorVM = new GetColorsVM();
            colorVM.Id = color.Id;
            colorVM.Name = color.Name;

            colorsVN.Add(colorVM);
        }

        return View(colorsVN);
    }


    public async Task<IActionResult> GetColor(int colorId)
    {
        var color = await _context.Colors.FirstOrDefaultAsync(x => x.Id == colorId);

        if (color is null)
            throw new NotFoundException<Color>();

        return View(color.Adapt<GetColorVM>());
    }


    [HttpGet]
    public async Task<IActionResult> UpdateColor(int colorId, string? error)
    {
        var color = await _context.Colors.FirstOrDefaultAsync(x => x.Id == colorId);
        ViewData["colorId"] = colorId;

        if (color is null)
            throw new NotFoundException<Color>();

        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View();
        }

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> UpdateColor(int colorId, UpdateColorDto updateColorDto, IFormFile? file)
    {
        var result = _updateColorDtoValidator.Validate(updateColorDto);

        if (!result.IsValid)
        {
            var errors = "";
            for (int i = 0; i < result.Errors.Count(); i++)
            {
                errors += $"   {i + 1}) " + $"{result.Errors[i].PropertyName}" + " : " + $"{result.Errors[i].ErrorMessage}";
            }
            return Redirect($"UpdateColor?error={errors}&&colorId={colorId}");
        }

        var color = await _context.Colors.FirstAsync(x => x.Id == colorId);

        if ((_context.Colors.Any(x => x.Name == updateColorDto.Name)) == true)
        {
            ModelState.AddModelError("Name", "name exists");
            return View();
        }

        if (file != null && file.Length > 0)
        {

            if (System.IO.File.Exists("wwwroot/" + color.ImagePath))
            {
                System.IO.File.Delete("wwwroot/" + color.ImagePath);
            }

            using (var stream = new FileStream("wwwroot/" + color.ImagePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
        }

        color.Name = updateColorDto.Name;

        _context.Colors.Update(color);
        await _context.SaveChangesAsync();

        return LocalRedirect($"/Colors/GetColor?colorId={colorId}");
    }



    public async Task<IActionResult> DeleteColor(int colorId)
    {
        var color = _context.Colors.FirstOrDefault(x => x.Id == colorId);

        if (color is null)
            throw new NotFoundException<Color>();

        if (System.IO.File.Exists("wwwroot/" + color.ImagePath))
        {
            System.IO.File.Delete("wwwroot/" + color.ImagePath);
        }

        _context.Colors.Remove(color);
        await _context.SaveChangesAsync();

        return RedirectToAction("GetColors");
    }
}
