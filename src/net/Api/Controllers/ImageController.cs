﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;

        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<Image> Post([FromBody] Image image = null)
        {
            return await Data.EnqueueImageAsync(image);
        }

        [HttpGet]
        [Route("/images")]
        public async IAsyncEnumerable<Image> Get()
        {
            await foreach (var image in Data.GetImagesAsync())
            {
                yield return image;
            }
        }

        [HttpGet]
        [Route("/image/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var image = await Data.GetImageAsync(id);
            if(image is null){
                return NotFound(id);
            }
            return Ok(image);
        }
    }
}
