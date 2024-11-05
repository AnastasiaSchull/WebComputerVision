using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using WebComputerVision.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebComputerVision.Controllers
{
	public class ImageController : Controller
	{
		private ComputerVisionClient Vision;

		public ImageController()
		{
            // доступ к сервису Azure Computer Vision
            string Key = "fa66c368d9b94bb4a9c7e28865fbb6f1";
          
            string endpoint = "https://anastasiavision.cognitiveservices.azure.com/";
			Vision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(Key))
			{
				Endpoint = endpoint
			};
		}

		[HttpGet]
		public IActionResult Upload()
		{
			return View("Index");
		}
		private ImageResult ExtractText(OcrResult result)
		{
			var extractedText = new ImageResult();
			foreach (var region in result.Regions)
			{
				foreach (var line in region.Lines)
				{
					foreach (var word in line.Words)
					{
						extractedText.Text += word.Text + " ";
					}
					extractedText.Text += "\n";
				}
			}
		
			return extractedText;
		}



        /*
           1 способ обработки загрузки изображений 

           метод сохраняет изображение в директорию wwwroot/images, затем открывает файл для чтения и отправляет его в Azure OCR для распознавания текста.
           Это метод RecognizePrintedTextInStreamAsync является частью клиентской библиотеки Azure Cognitive Services для Computer Vision,
           и он и обеспечивает обращение к Azure OCR (Optical Character Recognition) 
           После получения результатов текст извлекается и отображается пользователю.*/

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", image.FileName);//абсолютный путь к images\image.jpg
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);                                                           //сохраняем в wwwroot/images.
                }

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    var result = await Vision.RecognizePrintedTextInStreamAsync(true, stream);
                    var analysisResult = ExtractText(result);
                    return View("Result", analysisResult);
                }
            }
            return View("Index");
        }


        /*
        * 2 способ обработки загрузки изображений 
        
        В этом методе image сначала копируется в поток в памяти.
        Затем image изменяется в размере и сохраняется обратно в поток.
        Bitmap — это формат хранения изображения, используемый в компьютерной графике.
        В контексте .NET и многих других программировочных платформ, класс Bitmap представляет
        изображение в виде массива битов или пикселей. Этот класс позволяет работать с изображениями 
        непосредственно в памяти: можно читать, изменять, анализировать и сохранять изображения в различные форматы.
        Далее изменённое изображение отправляется в Azure OCR для распознавания текста.
        Полученный текст извлекается и отображается в представлении Result !*/

        /*[HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                using var stream = new MemoryStream();
                await image.CopyToAsync(stream);
                using var bitmap = new Bitmap(stream);

                //  изменение размера изображения
                var resized = new Bitmap(bitmap, new Size(bitmap.Width / 2, bitmap.Height / 2));

                using var ms = new MemoryStream();
                resized.Save(ms, ImageFormat.Png); // сохраняем изменённое изображение обратно в поток
                ms.Position = 0;                    // сброс позиции потока перед чтением

                // oтправка изображения в Azure OCR
                var result = await Vision.RecognizePrintedTextInStreamAsync(true, ms);
                var analysisResult = ExtractText(result);
                return View("Result", analysisResult);
            }
            return View("Index");
        }*/
    }
}
