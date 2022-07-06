using RastrToPFC.Data;
using System;

namespace RastrToPFC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var file = "";

            try
            {
                file = args[0];
                Console.WriteLine(file);
                RastrObject ob = new RastrObject(file);
                ob.PrintPFCFile();
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine("Файл не выбран. Для выбора файла перенесите его на исполняемый файл программы.");
            }

            Console.WriteLine("\nНажмите любую клавишу для завершения...");
        }
    }


}