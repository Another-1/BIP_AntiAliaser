using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ImageMagick;
using System.Globalization;
using System.Drawing;
using System.Resources;

namespace BIP_AntiAliaser
{
    class Program
    {
        static bool DirFound;
        static bool isDirectory;
        static bool isFile;

        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] == null)
            {
                ThatsAll("Нужно указать файлы или папку.");
            }
            DirFound = false;
            foreach (var input in args)
            {
                isDirectory = Directory.Exists(input);
                DirFound = DirFound | isDirectory;
            }
            if (DirFound & (args.Length > 1)) ThatsAll("Каталог можно передавать только один.");
            else
            {
                foreach (var input in args)
                {
                    isDirectory = Directory.Exists(input);
                    isFile = File.Exists(input);
                    if (!(isFile | isDirectory))
                    {
                        Console.WriteLine($"{input} и не файл, и не каталог. Пропускаем.");
                        continue;
                    }
                    if (isDirectory) ProcessDir(input); else ProcessFile(input);
                }
                Console.WriteLine("\r\nРабота завершена, нажмите что-нибудь чтобы выйти.");
                Console.ReadKey();
            }
        }
        static void ThatsAll(string errMess)
        {
            Console.WriteLine($"Ошибка. {errMess}");
            Console.WriteLine("\r\nНажмите что-нибудь чтобы выйти.");
            Console.ReadKey();
            return;
        }
        static void ProcessDir(string dir)
        {
            var files = Directory.GetFiles(dir);
            foreach (var file in files)
            {
                ProcessFile(file);
            }
        }
        static void ProcessFile(string file)
        {

            string str;
            Bitmap bmp;
            MagickImage pattern;
            MagickImage pattern2;
            MagickSearchResult pos;
            MagickImage tmpImg;
            MagickImage poSearchIn;
            bool changed;
            int X, Y;

            Console.WriteLine($"Обрабатываем файл {file}");
            try
            {
                var image = new MagickImage(file);
                var inputFileExtension = Path.GetExtension(file);
                if (inputFileExtension != ".png")
                {
                    Console.WriteLine($"Файл {file} не PNG. Пропускаем.");
                    return;
                }
                changed = false;
                tmpImg = new MagickImage(file);
                MagickImage yellowpattern = new MagickImage(color: Color.Yellow, width: image.Width - 6, height: image.Height - 2);
                tmpImg.Composite(yellowpattern, 3, 1, CompositeOperator.Over);
                for (int i = 1; i <= 36; i++)
                {
                    if (image.Height > 3 && image.Width > 3)
                    {
                        X = 177; Y = 177;
                        str = "find" + i.ToString();
                        bmp = (Bitmap)Properties.Resources.ResourceManager.GetObject(str);
                        str = "with" + i.ToString();
                        pattern = new MagickImage(bmp);
                        bmp = (Bitmap)Properties.Resources.ResourceManager.GetObject(str);
                        pattern2 = new MagickImage(bmp);
                        Console.WriteLine($"  Ищем шаблон {i}");
                        do
                        {
                            if (i <= 32) poSearchIn = image; else poSearchIn = tmpImg;
                            pos = poSearchIn.SubImageSearch(pattern, ErrorMetric.Absolute, 0.001);
                            if (pos != null
                                && !(X == pos.BestMatch.X & Y == pos.BestMatch.Y)
                                && pos.SimilarityMetric < 0.01
                                )
                            {
                                //if (i <= 32)
                                //{
                                    changed = true;
                                    str = string.Format("{0},{1}", pos.BestMatch.X, pos.BestMatch.Y);
                                    Console.WriteLine($"    Найден в позиции {str}");
                                //}
                                poSearchIn.Composite(pattern2, pos.BestMatch.X, pos.BestMatch.Y, CompositeOperator.Over);
                                if (i > 32)
                                {
                                    image.Composite(pattern2, pos.BestMatch.X, pos.BestMatch.Y, CompositeOperator.Over);
                                }
                            }
                            else pos = null;
                        }
                        while (pos != null);
                        if (changed)
                        {
                            image.Normalize();
                            image.Write(file);
                        }
                    }
                    else Console.WriteLine("Слишком маленькое, пропускаем");
                }
            }
            catch (MagickMissingDelegateErrorException)
            {
                Console.WriteLine($"Файл {file} не является картинкой. Пропускаем.");
            }
        }
    }
}