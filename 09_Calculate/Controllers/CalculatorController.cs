using Microsoft.AspNetCore.Mvc;

namespace _09_Calculate.Controllers
{
    public enum Operation { Add, Subtract, Multiply, Divide }

    public class CalculatorController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calculate(double num1, double num2, Operation operation)
        {
            double result = 0;
            string errorMessage = null;

            try
            {
                switch (operation)
                {
                    case Operation.Add:
                        result = num1 + num2;
                        break;
                    case Operation.Subtract:
                        result = num1 - num2;
                        break;
                    case Operation.Multiply:
                        result = num1 * num2;
                        break;
                    case Operation.Divide:
                        if (num2 != 0)
                        {
                            result = num1 / num2;
                        }
                        else
                        {
                            errorMessage = "Ошибка: деление на ноль невозможно.";
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Произошла ошибка: " + ex.Message;
            }

            ViewBag.Result = result;
            ViewBag.Num1 = num1; // Сохраняем первое число
            ViewBag.Num2 = num2; // Сохраняем второе число
            ViewBag.Operation = operation.ToString(); // Сохраняем выбранную операцию
            ViewBag.ErrorMessage = errorMessage; // Передаем сообщение об ошибке, если оно есть

            return View("Index");
        }
    }
}
