﻿using _09_Calculate.Controllers;

namespace _09_Calculate.Models
{
    public class CalculatorLibrary
    {
        /// <summary>
        /// Выполнение математической операции.
        /// </summary>
        /// <param name="num1">Первый операнд.</param>
        /// <param name="num2">Второй операнд.</param>
        /// <param name="operation">Тип операции (сложение, вычитание, умножение, деление).</param>
        /// <returns>Результат математической операции.</returns>
        public static double CalculateOperation(double num1, double num2, Operation operation)
        {
            return operation switch
            {
                Operation.Add => num1 + num2,
                Operation.Subtract => num1 - num2,
                Operation.Multiply => num1 * num2,
                Operation.Divide => num1 / num2,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), "Invalid operation"),
            };
        }
    }
}
