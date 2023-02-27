using Car_Configuration.Data;
using Car_Configuration.Dtoes;
using Car_Configuration.Entities;
using Car_Configuration.Exceptions;
using Car_Configuration.Models;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Car_Configuration.Controllers;

[Authorize(Roles = "admin,manager")]
public class WheelColorModelsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateWheelColorModelDto> _createWheelColorModelDtoValidator;

    public WheelColorModelsController(AppDbContext context,
         IValidator<CreateWheelColorModelDto> createWheelColorModelDtoValidator)
    {
        _context = context;
        _createWheelColorModelDtoValidator = createWheelColorModelDtoValidator;
    }

    public IActionResult CreateWheelColorModel() => View();

    [HttpPost]
    public async Task<IActionResult> CreateWheelColorModel(CreateWheelColorModelDto createWheelColorModelDto, IFormFile? file)
    {
        var result = _createWheelColorModelDtoValidator.Validate(createWheelColorModelDto);

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

        var model = await _context.Models.FirstOrDefaultAsync(x => x.Name == createWheelColorModelDto.ModelName);

        if (model == null)
        {
            ModelState.AddModelError("", "model not found");
            return View();
        }

        var colorModel = model.ColorModels?.FirstOrDefault(x => x.Color.Name == createWheelColorModelDto.ColorModelName);

        if (colorModel == null)
        {
            ModelState.AddModelError("", "color not found");
            return View();
        }

        var wheel = model.Wheels?.FirstOrDefault(x => x.Name == createWheelColorModelDto.WheelName);

        if (wheel == null)
        {
            ModelState.AddModelError("", "wheel not found");
            return View();
        }

        if ((_context.WheelColorModels.Any(x => x.ColorModelId == colorModel.Id && x.WheelId == wheel.Id)) == true)
        {
            ModelState.AddModelError("", "those exists");
            return View();
        }

        var wheelColorModel = new WheelColorModel();
        wheelColorModel.ColorModelId = colorModel.Id;
        wheelColorModel.WheelId = wheel.Id;
        string imageName = Guid.NewGuid().ToString("N");
        var filePath = Path.Combine($"images/{model.FolderPath}", $"{imageName}.jpg");

        using (var stream = new FileStream("wwwroot/" + filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        wheelColorModel.ColorWheelPath = filePath;

        await _context.WheelColorModels.AddAsync(wheelColorModel);
        await _context.SaveChangesAsync();

        return Redirect($"GetWheelColorModel?wheelColorModelId={wheelColorModel.Id}");
    }


    public async Task<IActionResult> GetWheelColorModels()
    {
        var wheelColorModels = await _context.WheelColorModels.ToListAsync();
        var wheelColorModelsVM = new List<GetWheelColorModelsVM>();

        if (wheelColorModels is null)
            return View(null);

        foreach (var wheelColorModel in wheelColorModels)
        {
            var wheelColorModelVM = new GetWheelColorModelsVM();
            wheelColorModelVM.Id = wheelColorModel.Id;
            wheelColorModelVM.ModelName = wheelColorModel.ColorModel.Model.Name;
            wheelColorModelVM.ColorModelName = wheelColorModel.ColorModel.Color.Name;
            wheelColorModelVM.WheelName = wheelColorModel.Wheel.Name;

            wheelColorModelsVM.Add(wheelColorModelVM);
        }

        return View(wheelColorModelsVM);
    }


    public async Task<IActionResult> GetWheelColorModel(int wheelColorModelId)
    {
        var wheelColorMode = await _context.WheelColorModels.FirstOrDefaultAsync(x => x.Id == wheelColorModelId);

        if (wheelColorMode is null)
            throw new NotFoundException<WheelColorModel>();

        return View(wheelColorMode.Adapt<GetWheelColorModelVM>());
    }




    [HttpGet]
    public async Task<IActionResult> UpdateWheelColorModel(int wheelColorModelId, string? error)
    {
        var wheelColorModel = await _context.WheelColorModels.FirstOrDefaultAsync(x => x.Id == wheelColorModelId);
        @ViewData["wheelColorModelId"] = wheelColorModelId;

        if (wheelColorModel is null)
            throw new NotFoundException<WheelColorModel>();

        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View();
        }

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> UpdateWheelColorModel(int wheelColorModelId, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            List<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
            string errorsString = "";

            for (int i = 0; i < allErrors.Count(); i++)
            {
                errorsString += $"   {i + 1}) " + allErrors[i].ErrorMessage;
            }
            return Redirect($"UpdateWheelColorModel?error={errorsString}&&wheelColorModelId={wheelColorModelId}");
        }


        var wheelColorModel = await _context.WheelColorModels.FirstAsync(x => x.Id == wheelColorModelId);


        if (file != null && file.Length > 0)
        {

            if (System.IO.File.Exists("wwwroot/" + wheelColorModel.ColorWheelPath))
            {
                System.IO.File.Delete("wwwroot/" + wheelColorModel.ColorWheelPath);
            }

            using (var stream = new FileStream("wwwroot/" + wheelColorModel.ColorWheelPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
        }

        return LocalRedirect($"/WheelColorModels/GetWheelColorModel?wheelColorModelId={wheelColorModelId}");
    }



    public async Task<IActionResult> DeleteWheelColorModel(int wheelColorModelId)
    {
        var wheelColorModel = _context.WheelColorModels.FirstOrDefault(x => x.Id == wheelColorModelId);

        if (wheelColorModel is null)
            throw new NotFoundException<WheelColorModel>();

        if (System.IO.File.Exists("wwwroot/" + wheelColorModel.ColorWheelPath))
        {
            System.IO.File.Delete("wwwroot/" + wheelColorModel.ColorWheelPath);
        }
        string path = "wwwroot/images/" + wheelColorModel.ColorWheelPath;


        _context.WheelColorModels.Remove(wheelColorModel);
        await _context.SaveChangesAsync();

        return RedirectToAction("GetWheelColorModels");
    }


}
