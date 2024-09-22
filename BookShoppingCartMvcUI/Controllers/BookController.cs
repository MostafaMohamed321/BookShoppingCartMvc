﻿using BookShoppingCartMvcUI.Constants;
using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

namespace BookShoppingCartMvcUI.Controllers
{

    [Authorize(Roles = (nameof(Roles.Admin)))]
    public class BookController : Controller
    {
        private readonly IBookRepository _bookRepo;
        private readonly IGenreRepository _genreRepo;
        private readonly IFileService _fileService;
        public BookController(IFileService fileService, IBookRepository bookRepository, IGenreRepository genreRepository)
        {
            _fileService = fileService;
            _bookRepo = bookRepository;
            _genreRepo = genreRepository;
        }
        public async Task<IActionResult> Index()
        {
            var books= await _bookRepo.GetBooks();
            return View(books);
        }
        public async Task<IActionResult> AddBook()
        {
          var genreList = (await _genreRepo.GetGenres()).Select(a=> new SelectListItem
          {
              Value =a.Id.ToString(),
              Text =a.GenreName
             
          } );
            BookDTO bookToAdd = new() { GenreList = genreList };
            return View(bookToAdd);
           
        }
        [HttpPost]
        public async Task<IActionResult> AddBook(BookDTO book)
       {
            var genreList = (await _genreRepo.GetGenres()).Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.GenreName

            });
            book.GenreList = genreList;
            if (!ModelState.IsValid)
               return View(book);

            try
            {
                if (book.ImageFile != null)
                {
                    if (book.ImageFile.Length > 1 * 1024 * 1024)
                    {
                        throw new InvalidOperationException("Image file can not Exceed 1MB");
                    }
                    string[] allowedExtension = { ".jpg", ".jpeg", ".png" };
                    string imageName = await _fileService.SaveFile(book.ImageFile, allowedExtension);
                    book.Image = imageName;
                }
                Book bo = new()
                {
                    BookName = book.BookName,
                    Id = book.Id,
                    AuthorName = book.AuthorName,
                    Image = book.Image,
                    Price = book.Price,
                    GenreId = book.GenreId

                };
                await _bookRepo.AddBook(bo);
                TempData["successMassage"] = "Book is add Successfully";
               return RedirectToAction(nameof(AddBook));

            }
            catch (InvalidOperationException ex)
            {
                TempData["errorMassage"] = $"{ex.Message}";
                return View(book);
            }
            catch (FileNotFoundException ex)
            {
                TempData["errorMassage"] = ex.Message;
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["errorMassage"] = "Error in Save Data"; ;
                return View(book);
            }

       }
        public async Task<IActionResult> UpdateBook(int id)
        {
            var book = (await _bookRepo.GetBookById(id));
            if (book == null) 
            {
                TempData["errorMassage"] = " The Book Id not found";
                return RedirectToAction(nameof(Index));
            }
            var genreSelectList = (await _genreRepo.GetGenres()).Select(a => new SelectListItem
            {
                Value =a.Id.ToString(),
                Text =a.GenreName,
                Selected =a.Id ==book.Id
            });
            BookDTO bookUpdate = new()
            {
           
                GenreList = genreSelectList,
                BookName =book.BookName,
                AuthorName =book.AuthorName,
                GenreId = book.GenreId,
                Image =book.Image,
                Price =book.Price,
            };
            return View(bookUpdate);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBook(BookDTO bookToUpdate)
        {
            var genreSelectList = (await _genreRepo.GetGenres()).Select(genre => new SelectListItem
            {
                Text = genre.GenreName,
                Value = genre.Id.ToString(),
                Selected = genre.Id == bookToUpdate.GenreId
            });
            bookToUpdate.GenreList = genreSelectList;

            if (!ModelState.IsValid)
                return View(bookToUpdate);

            try
            {
                string oldImage = "";
                if (bookToUpdate.ImageFile != null)
                {
                    if (bookToUpdate.ImageFile.Length > 1 * 1024 * 1024)
                    {
                        throw new InvalidOperationException("Image file can not exceed 1 MB");
                    }
                    string[] allowedExtensions = { ".jpeg", ".jpg", ".png" };
                    string imageName = await _fileService.SaveFile(bookToUpdate.ImageFile, allowedExtensions);
                    // hold the old image name. Because we will delete this image after updating the new
                    oldImage = bookToUpdate.Image;
                    bookToUpdate.Image = imageName;
                }
                // manual mapping of BookDTO -> Book
                Book book = new()
                {
                    Id = bookToUpdate.Id,
                    BookName = bookToUpdate.BookName,
                    AuthorName = bookToUpdate.AuthorName,
                    GenreId = bookToUpdate.GenreId,
                    Price = bookToUpdate.Price,
                    Image = bookToUpdate.Image
                };
                await _bookRepo.UpdateBook(book);
                // if image is updated, then delete it from the folder too
                if (!string.IsNullOrWhiteSpace(oldImage))
                {
                    _fileService.DeleteFile(oldImage);
                }
                TempData["successMessage"] = "Book is updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["errorMessage"] = ex.Message;
                return View(bookToUpdate);
            }
            catch (FileNotFoundException ex)
            {
                TempData["errorMessage"] = ex.Message;
                return View(bookToUpdate);
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "Error on saving data";
                return View(bookToUpdate);
            }
        }

        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var book = await _bookRepo.GetBookById(id);
                if (book == null)
                {
                    TempData["errorMessage"] = $"Book with the id: {id} does not found";
                }
                else
                {
                    await _bookRepo.DeleteBook(book);
                    if (!string.IsNullOrWhiteSpace(book.Image))
                    {
                        _fileService.DeleteFile(book.Image);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["errorMessage"] = ex.Message;
            }
            catch (FileNotFoundException ex)
            {
                TempData["errorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = "Error on deleting the data";
            }
            return RedirectToAction(nameof(Index));
        }

    }
}

