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
using System.Data;

namespace Car_Configuration.Controllers;

[Authorize(Roles = "admin,manager")]
public class ModelsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateModelDto> _createModelDtoValidator;
    private readonly IValidator<UpdateModelDto> _updateModelDtoValidator;

    public ModelsController(AppDbContext context,
        IValidator<CreateModelDto> createModelDtoValidator,
        IValidator<UpdateModelDto> updateModelDtoValidator)
    {
        _context = context;
        _createModelDtoValidator = createModelDtoValidator;
        _updateModelDtoValidator = updateModelDtoValidator;
    }

    public IActionResult CreateModel() => View();

    [HttpPost]
    public async Task<IActionResult> CreateModel(CreateModelDto createModelDto, IFormFile? file)
    {
        var result = _createModelDtoValidator.Validate(createModelDto);

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

        if (_context.Models.Any(x => x.Name == createModelDto.Name))
        {
            ModelState.AddModelError("Name", "name exists");
            return View();
        }

        var model = new Model();
        model.Name = createModelDto.Name;
        string folderPath = Guid.NewGuid().ToString("N");
        Directory.CreateDirectory("wwwroot/images/" + folderPath);
        string imageName = Guid.NewGuid().ToString("N");
        var filePath = Path.Combine($"images/{folderPath}", $"{imageName}.jpg");

        using (var stream = new FileStream("wwwroot/" + filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        model.ImagePath = filePath;
        model.FolderPath = folderPath;

        await _context.Models.AddAsync(model);
        await _context.SaveChangesAsync();

        return Redirect($"GetModel?modelId={model.Id}");
    }


    public async Task<IActionResult> GetModels()
    {
        var models = await _context.Models.ToListAsync();
        var modelsVM = new List<GetModelsVM>();

        if (models is null)
            return View(null);

        foreach (var model in models)
        {
            var modelVM = new GetModelsVM();
            modelVM.Id = model.Id;
            modelVM.Name = model.Name;
            modelVM.IsReady = model.IsReady;
            modelVM.ColorsAmount = model.ColorModels?.Count() ?? 0;
            modelVM.WheelsAmount = model.Wheels?.Count() ?? 0;
            if (model.ColorModels != null)
                foreach (var m in model.ColorModels)
                {
                    modelVM.ColorWheelsAmount += m.WheelColors?.Count() ?? 0;
                }

            modelsVM.Add(modelVM);
        }

        return View(modelsVM);
    }


    public async Task<IActionResult> GetModel(int modelId)
    {
        var model = await _context.Models.FirstOrDefaultAsync(x => x.Id == modelId);
        if (model is null)  
            throw new NotFoundException<Model>();
        
        return View(model.Adapt<GetModelVM>());
    }



    [HttpGet]
    public async Task<IActionResult> UpdateModel(int modelId, string? error)
    {
        var model = await _context.Models.FirstOrDefaultAsync(x => x.Id == modelId);
        ViewData["modelId"] = modelId;

        if (model is null)
            throw new NotFoundException<Model>();

        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return View();
        }

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> UpdateModel(int modelId, UpdateModelDto updateModelDto, IFormFile? file)
    {
        var result = _updateModelDtoValidator.Validate(updateModelDto);

        if (!result.IsValid)
        {
            var errors = "";
            for (int i = 0; i < result.Errors.Count(); i++)
            {
                errors += $"   {i + 1}) " + $"{result.Errors[i].PropertyName}" + " : " + $"{result.Errors[i].ErrorMessage}";
            }
            return Redirect($"UpdateModel?error={errors}&&modelId={modelId}");
        }

        var model = await _context.Models.FirstAsync(x => x.Id == modelId);

        if (_context.Models.Any(x => x.Name == updateModelDto.Name) && model.Name != updateModelDto.Name)
        {
            ModelState.AddModelError("Name", "name exists");
            return View();
        }

        if (file != null && file.Length > 0)
        {

            if (System.IO.File.Exists("wwwroot/" + model.ImagePath))
            {
                System.IO.File.Delete("wwwroot/" + model.ImagePath);
            }

            using (var stream = new FileStream("wwwroot/" + model.ImagePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
        }

        model.Name = updateModelDto.Name;
        model.IsReady = updateModelDto.IsReady;

        _context.Models.Update(model);
        await _context.SaveChangesAsync();

        return LocalRedirect($"/Models/GetModel?modelId={modelId}");
    }



    public async Task<IActionResult> DeleteModel(int modelId)
    {
        var model = _context.Models.FirstOrDefault(x => x.Id == modelId);

        if (model is null)
            throw new NotFoundException<Model>();

        string path = "wwwroot/images/" + model.FolderPath;

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

        _context.Models.Remove(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("GetModels");
    }

    public IActionResult GetStatistics(int modelId)
    {
        var colorModels = _context.ColorModels.Where(x => x.ModelId == modelId).ToList().Select(x => new { x.Color.Name, x.Id }).ToList();
        var wheelModels = _context.Wheels.Where(x => x.ModelId == modelId).ToList().Select(x => new { x.Name, x.Id }).ToList();
        List<List<string>> results = new List<List<string>>();

        for (int i = 0; i < colorModels.Count(); i++)
        {
            for (int j = 0; j < wheelModels.Count(); j++)
            {
                List<string> result = new List<string>();
                result.Add(colorModels[i].Name);
                result.Add(wheelModels[j].Name);
                var isExist = IsExist(colorModels[i].Id, wheelModels[j].Id);
                result.Add(isExist.ToString());

                results.Add(result);
            }
        }

        results = results.OrderBy(group => group[2]).ToList();

        return View(results);
    }

    private bool IsExist(int id1, int id2)
    {
        return _context.WheelColorModels.Any(x => x.ColorModelId == id1 && x.WheelId == id2);
    }
}
