using Microsoft.AspNetCore.Mvc;

namespace Project_X_Data.Models.Home
{
    public class AdminGroupFormModel
    {
        [FromForm(Name = "group-name")]
        public String Name { get; set; } = null!;

        [FromForm(Name = "group-description")]
        public String Description { get; set; } = null!;

        [FromForm(Name = "group-slug")]
        public String Slug { get; set; } = null!;

        [FromForm(Name = "group-img")]
        public IFormFile Image { get; set; } = null!;
    }
}
/* HTTP Request  [FromRoute]  [FromQuery]
 * ------------------|--------|-------------------
 * POST /Home/Admin/345?mode=group  HTTP/2
 * Host: localhost:7031
 * Accept: text/html
 * Content-Type: application/x-www-form-urlencoded
 * My-Header: 123                                        [FromHeader]
 * 
 * product-name=TheProduct&product-slug=the-product&...  [FromForm]
 * 
 * 
 * Д.З. Закласти основу курсової роботи
 * - визначитись з командою (щонайменше 2 учасники)
 * - обрати тематику проєкту ("клон" популярного сайту)
 * - представити схему даних (БД)
 * - створити контекст даних та основні сервіси
 * Прикласти посилання на репозиторій курсового проєкту
 */
