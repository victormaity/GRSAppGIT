using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GlobalReportingSystem.Data.DB;

namespace TestConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new GRSDataBaseEntities())
            {
                Console.WriteLine("To start press 'y'");
                var start = Console.ReadLine();
                while (start == "y")
                {
                    Console.WriteLine("Specify the row that you want to fill");
                    var crl = Console.ReadLine();
                    int target;
                    int initial;
                    if (int.TryParse(crl, out target))
                    {
                        var fsToReplaсe = db.FilesStorages.FirstOrDefault(p => p.ID == target);
                        if (fsToReplaсe != null)
                        {
                            Console.WriteLine("Row with ID='" + target + "' was found");
                            Console.WriteLine("Specify the row that you want be filled from");
                            crl = Console.ReadLine();
                            if (int.TryParse(crl, out initial))
                            {
                                var fsToTransmit = db.FilesStorages.FirstOrDefault(p => p.ID == initial);
                                if (fsToTransmit != null)
                                {
                                    Console.WriteLine("Row with ID='" + initial + "' was found");
                                    fsToReplaсe.FileContent = fsToTransmit.FileContent;
                                    db.SaveChanges();
                                    Console.WriteLine("Changes were saved");
                                    Console.WriteLine("Data was transferred");
                                    db.FilesStorages.Remove(fsToTransmit);
                                    Console.WriteLine("Row with ID='" + initial + "' was deleted");
                                    db.SaveChanges();
                                    Console.WriteLine("Changes were saved");
                                    //Console.WriteLine("To continue press ENTER");
                                    //ConsoleKeyInfo cki = Console.ReadKey();
                                    //if (cki.Key != ConsoleKey.Enter)
                                    //{
                                    //    start = "n";
                                    //}
                                }
                                else
                                {
                                    Console.WriteLine("Row where ID='" + initial + "' wasn't found");
                                }
                            }
                            else
                            {
                                Console.WriteLine("ID should be integer");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Row where ID='" + target + "' wasn't found");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ID should be integer");
                    }
                }
            }
        }
    }
}
