using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthwindConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthwindConsole
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "//nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related products");
                    Console.WriteLine("4) Display all Categories and their related products");
                    Console.WriteLine("5) Add a new Product");
                    Console.WriteLine("6) Display Products");
                    Console.WriteLine("7) Edit a Product");
                    Console.WriteLine("8) Edit a Category");                    
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    var db = new NWConsole_48_JTEContext();
                    if (choice == "1")
                    {
                        DisplayCategories(db);
                    }
                    else if (choice == "2")
                    {
                        Category category = new Category();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();

                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            
                            // check for unique name
                            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                Console.WriteLine(category.CategoryName + " " + category.Description);
                                db.Categories.Add(category);
                                db.SaveChanges();
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                        foreach (Product p in category.Products.Where(p => p.Discontinued == false))
                        {
                            Console.WriteLine(p.ProductName);
                        }
                    }
                    else if (choice == "4")
                    {
                        
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Product p in item.Products)
                            {
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                    }
                    else if (choice == "5")
                    {
                        
                        Category category = new Category();
                        Product product = new Product();
                        DisplayCategories(db);
                        Console.WriteLine("Enter the Category Id for this product:");
                        product.CategoryId = Int32.Parse(Console.ReadLine());
                        
                        if (db.Categories.Any(c => c.CategoryId == product.CategoryId))
                        {
                            DisplaySuppliers(db);
                            Console.WriteLine("Enter the Supplier Id for this product:");
                            product.SupplierId = Int32.Parse(Console.ReadLine());
                            if (db.Suppliers.Any(c => c.SupplierId == product.SupplierId))
                            {
                                Console.WriteLine("Enter Product Name:");
                                product.ProductName = Console.ReadLine();

                                if (db.Products.Any(c => c.ProductName != product.ProductName))
                                {
                                    Console.WriteLine("Enter the Product quantity per unit:");
                                    product.QuantityPerUnit = Console.ReadLine();

                                    Console.WriteLine("Enter the Product unit price");
                                    product.UnitPrice = decimal.Round(decimal.Parse(Console.ReadLine()), 2);

                                    
                                    product.UnitsInStock = 0;
                                    product.UnitsOnOrder = 0;
                                    product.ReorderLevel = 0;
                                    product.Discontinued = false;

                                    db.Products.Add(product);
                                    db.SaveChanges();
                                    Console.WriteLine("New product: " + product.ProductName + " - has been added!");
                                }
                                else
                                {
                                    logger.Error("Entered Product Name is the same as an existing Product Name");
                                }
                            }
                            else
                            {
                                logger.Error("Entered Supplier Id doesn't match any Id in Supplier");
                            }
                        }
                        else
                        {
                            logger.Error("Entered Categories Id doesn't match any Id in Categories");
                        }



                    }else if (choice == "6")
                    {
                        int displayChoice;
                        Console.WriteLine("1) Display all Products");
                        Console.WriteLine("2) Display Active Products");
                        Console.WriteLine("3) Display Discontinued Products");
                        Console.WriteLine("4) Display a specific Product");
                        displayChoice = int.Parse(Console.ReadLine());
                        Console.Clear();

                        switch (displayChoice)
                        {
                            case 1:
                                DisplayAllProducts(db);
                                break;

                            case 2:
                                DisplayActiveProducts(db);
                                break;

                            case 3:
                                DisplayDiscontinuedProducts(db);
                                break;

                            case 4:
                                DisplaySpecificProduct(db);
                                break;

                            default:
                                logger.Error("You didn't pick of the options");
                                break;
                        }
                    }else if (choice == "7")
                    {

                        Product product = new Product();
                        DisplayAllProducts(db);
                        Console.WriteLine("Enter the id of the product you want to edit:");
                        product.ProductId = int.Parse(Console.ReadLine());

                        if (db.Products.Any(c => c.ProductId == product.ProductId))
                        {
                            product = db.Products.FirstOrDefault(c => c.ProductId == product.ProductId);

                            Console.WriteLine("Do you want to change the Product Name Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new name:");
                                product.ProductName = Console.ReadLine();
                            }

                            Console.WriteLine("Do you want to change the Supplier Id Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new Supplier Id:");
                                product.SupplierId = int.Parse(Console.ReadLine());
                            }

                            Console.WriteLine("Do you want to change the Category Id Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new Category Id:");
                                product.CategoryId = int.Parse(Console.ReadLine());
                            }


                            Console.WriteLine("Do you want to change the Quantiry Per Unit Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new Quantiry Per Unit:");
                                product.QuantityPerUnit = Console.ReadLine();
                            }


                            Console.WriteLine("Do you want to change the Unit Price Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new Unit Price:");
                                product.UnitPrice = decimal.Round(decimal.Parse(Console.ReadLine()), 2);
                            }


                            Console.WriteLine("Do you want to change the Units In Stock Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new Units In Stock:");
                                product.UnitsInStock = short.Parse(Console.ReadLine());
                            }


                            Console.WriteLine("Do you want to change the Units On Order Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new Units On Order:");
                                product.UnitsOnOrder = short.Parse(Console.ReadLine());
                            }


                            Console.WriteLine("Do you want to change the Reorder Level Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new Reorder Level:");
                                product.ReorderLevel = short.Parse(Console.ReadLine());
                            }


                            Console.WriteLine("Do you want to change the Discontinued Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                int isDiscontined;
                                Console.WriteLine("Enter new Discontined (1 = true | 0 = false):");
                                isDiscontined = int.Parse(Console.ReadLine());

                                if (isDiscontined == 1)
                                {
                                    product.Discontinued = true;
                                }
                                else if (isDiscontined == 2)
                                {
                                    product.Discontinued = false;
                                }
                            }
                            db.EditProduct(product);
                            logger.Info($"Product {product.ProductId} has been updated!");

                        }
                        else
                        {
                            logger.Error("The Product Id you entered does not exist.");
                        }

                    }    else if (choice == "8")
                    {
                        Category category = new Category();
                        DisplayCategories(db);
                        Console.WriteLine("Enter the id of the category you want to edit:");
                        category.CategoryId = int.Parse(Console.ReadLine());

                        if (db.Categories.Any(c => c.CategoryId == category.CategoryId))
                        {
                            category = db.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);

                            Console.WriteLine("Do you want to change the Category Name Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new name:");
                                category.CategoryName = Console.ReadLine();
                            }

                            Console.WriteLine("Do you want to change the Discription Y | N");
                            choice = Console.ReadLine().ToLower();
                            if (choice == "y")
                            {
                                Console.WriteLine("Enter new Discription:");
                                category.Description = Console.ReadLine();
                            }

                            db.EditCategory(category);
                            logger.Info($"Category {category.CategoryId} has been updated!");

                        }
                        else
                        {
                            logger.Error("The Category Id you entered does not exist.");
                        }
                    }
                    Console.WriteLine();

                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        private static void DisplayCategories(NWConsole_48_JTEContext db)
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine(String.Format("{0,-10} | {1,-10} {2,-10}", $"ID: {item.CategoryId}", $"Name: {item.CategoryName}", $"- {item.Description}"));
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void DisplaySuppliers(NWConsole_48_JTEContext db)
        {

            var query = db.Suppliers.OrderBy(p => p.SupplierId);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine(String.Format("{0,-10} | {1,-10}", $"ID: {item.SupplierId}", $"Name: {item.CompanyName}"));
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void DisplayAllProducts(NWConsole_48_JTEContext db)
        {
            var query = db.Products.OrderBy(p => p.ProductId);
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                if (item.Discontinued == true)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"Id: {item.ProductId} - Discontinued Product Name: {item.ProductName}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Id: {item.ProductId} - Active Product Name: {item.ProductName}");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void DisplayActiveProducts(NWConsole_48_JTEContext db)
        {

            var query = db.Products.OrderBy(p => p.Discontinued).Where(p => p.Discontinued == false);
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;

            foreach (var item in query)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Id: {item.ProductId} - Product Name: {item.ProductName} - {item.Discontinued}");

            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void DisplayDiscontinuedProducts(NWConsole_48_JTEContext db)
        {
            var query = db.Products.OrderBy(p => p.Discontinued).Where(p => p.Discontinued == true);
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;

            foreach (var item in query)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Id: {item.ProductId} - Product Name: {item.ProductName} - {item.Discontinued}");

            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void DisplaySpecificProduct(NWConsole_48_JTEContext db)
        {

            int inputId;

            Console.WriteLine("Enter the id of the product you want to view:");
            inputId = int.Parse(Console.ReadLine());

            if (db.Products.Any(c => c.ProductId == inputId))
            {
                var query = db.Products.Where(p => p.ProductId == inputId);
                Console.ForegroundColor = ConsoleColor.Magenta;

                foreach (var item in query)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Id: {item.ProductId}");
                    Console.WriteLine($"Name: {item.ProductName}");
                    Console.WriteLine($"SupplierId: {item.SupplierId}");
                    Console.WriteLine($"CategoryId: {item.CategoryId}");
                    Console.WriteLine($"QuantityPerUnit: {item.QuantityPerUnit}");
                    Console.WriteLine($"UnitPrice: {item.UnitPrice}");
                    Console.WriteLine($"UnitsInStock: {item.UnitsInStock}");
                    Console.WriteLine($"UnitsOnOrder: {item.UnitsOnOrder}");
                    Console.WriteLine($"ReorderLevel: {item.ReorderLevel}");
                    Console.WriteLine($"Discontinued: {item.Discontinued}");

                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                logger.Info("There is no Product with that Id");
            }

        }

    }
}