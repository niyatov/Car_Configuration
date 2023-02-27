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
using System.Data;

namespace Car_Configuration.Controllers;

[Authorize(Roles = "admin,manager")]
public partial class ColorsController : Controller
{
    public IActionResult CreateColorModel() => View();
    [HttpPost]
    public async Task<IActionResult> CreateColorModel(CreateColorModelDto createColorModelDto)
    {
        var result = _createColorModelDtoValidator.Validate(createColorModelDto);

        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return View();
        }

        var model = await _context.Models.FirstOrDefaultAsync(x => x.Name == createColorModelDto.ModelName);
        if (model == null)
        {
            ModelState.AddModelError("", "model not found");
            return View();
        }

        var color = await _context.Colors.FirstOrDefaultAsync(x => x.Name == createColorModelDto.ColorName);
        if (color == null)
        {
            ModelState.AddModelError("", "color not found");
            return View();
        }

        if ((model.ColorModels?.Any(x=>x.ColorId== color.Id) == true))
        {
            ModelState.AddModelError("", "color exist");
            return View();
        }



        var colorModel = new ColorModel();
        colorModel.ColorId = color.Id ;
        colorModel.ModelId = model.Id ;

        await _context.ColorModels.AddAsync(colorModel);
        await _context.SaveChangesAsync();

        return Redirect($"GetColorModel?colorModelId={colorModel.Id}");
    }



    public async Task<IActionResult> GetColorModels()
    {
        var colorModels = await _context.ColorModels.ToListAsync();
        var colorModelsVN = new List<GetColorModelsVM>();

        if (colorModels is null)
            return View(null);

        foreach (var colorModel in colorModels)
        {
            var colorModelVM = new GetColorModelsVM();
            colorModelVM.Id = colorModel.Id;
            colorModelVM.Name = colorModel.Color.Name;
            colorModelVM.ModelName = colorModel.Model.Name;

            colorModelsVN.Add(colorModelVM);
        }

        return View(colorModelsVN);
    }


    public async Task<IActionResult> GetColorModel(int colorModelId)
    {
        var colorModel = await _context.ColorModels.FirstOrDefaultAsync(x => x.Id == colorModelId);

        if (colorModel is null)
            throw new NotFoundException<ColorModel>();

        var result = colorModel.Adapt<GetColorModelVM>();
        result.Name = colorModel.Color.Name;
        result.ImagePath = colorModel.Color.ImagePath;

        return View(result);
    }


    public async Task<IActionResult> DeleteColorModel(int colorModelId)
    {
        var colorModel = _context.ColorModels.FirstOrDefault(x => x.Id == colorModelId);

        if (colorModel is null)
            throw new NotFoundException<ColorModel>();


        _context.ColorModels.Remove(colorModel);
        await _context.SaveChangesAsync();

        return RedirectToAction("GetColorModels");
    }
}
