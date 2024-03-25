using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Controllers;

public class SearchController : Controller
{
      private readonly WebStoreContext _context;

        public SearchController(WebStoreContext context)
        {
            _context = context;
        }
        
        //Приведение строки к нормальному виду
        private string StringTransformation(string searchString)
        {
            // Преобразования строки к тому или иному виду 
            searchString = searchString.Trim();
            searchString = searchString.ToLower();
            searchString = Regex.Replace(searchString, @"\s+", " ");

            //Исключение повторяющихся слов
            HashSet<string> str = new HashSet<string>();
            string[] data = searchString.Split(' ');
            for (int i = 0; i < data.Length; i++)
            {
                str.Add(data[i]);
            }

            searchString = string.Join(" ", str);

            return searchString;
        }

        //Распределение слов для условий
        private string[] StringDistribution(string searchString)
        {
            int spaceCounter = 0;

            for (int i = 0; i < searchString.Length; i++)
            {
                if (searchString[i].Equals(' '))
                {
                    spaceCounter++;
                }
            }

            string hashtag = "";
            string hashtag2 = "";
            string productName = "";
            int hashtagsCounter = 0;

            while (spaceCounter >= 0)
            {
                string temp = searchString.Split()[spaceCounter];
                if (temp.Contains('#') || _context.Products.Any(p => p.Hashtags.ToLower().Contains(temp)))
                {
                    hashtagsCounter++;
                    if (hashtagsCounter <= 1)
                    {
                        hashtag = temp;
                    }
                    else if (hashtagsCounter > 1)
                    {
                        hashtag2 = temp;
                    }
                }
                else if (_context.Products.Any(p => p.ProductName.ToLower().Contains(temp)))
                {
                    productName = temp;
                }

                spaceCounter--;
            }
            
            string[] words = {hashtag,hashtag2,productName};

            return words;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<Product>>> SearchMobilePhones(string searchString = "")
        {
            // Исключение, которые приводят сразу к выводу ошибки 
            if (searchString == "")
            {
                return Ok("Empty");
            }

            searchString = StringTransformation(searchString);
            string[] words = StringDistribution(searchString);
            
            string hashtag = words[0];
            string hashtag2 = words[1];
            string productName = words[2];
            
            /*var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", productName + ".png");
            byte[] imageData = System.IO.File.ReadAllBytes(path);
            string base64Image = Convert.ToBase64String(imageData); */

            IQueryable<Product> products = _context.Products;
            if (hashtag == "" || hashtag == "" && hashtag2 == "")
            {
                products = products.Where(u => u.Hashtags.ToLower().Contains(searchString) || u.ProductName.ToLower().Contains(searchString));   
            }
            
            if (hashtag != "" && hashtag2 != "" && productName != "")
            {
                products = products.Where(u => (u.Hashtags.ToLower().Contains(hashtag) && u.Hashtags.ToLower().Contains(hashtag2) && u.ProductName.ToLower().Contains(productName)));
            }
            else if (hashtag != "" && hashtag2 != "" && productName == "")
            {
                products = products.Where(u => u.Hashtags.ToLower().Contains(hashtag) && u.Hashtags.ToLower().Contains(hashtag2));
            }
            else if (hashtag != "" || productName != "")
            {
                products = products.Where(u => u.Hashtags.ToLower().Contains(hashtag) && u.ProductName.ToLower().Contains(productName));
            }
            
            if (!products.Any())
            {
                return NotFound();
            }
            
            await _context.SaveChangesAsync();

            return View(await products.Distinct().ToArrayAsync());
        }

        [HttpGet("GetMobilePicture")]
        public IActionResult GetImage(string imageName)
        {
            imageName = imageName[0].ToString().ToUpper() + imageName.Substring(1);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Images", imageName);
            var image = System.IO.File.OpenRead(path + ".png");
            
            return File(image, "image/png");
        }
}