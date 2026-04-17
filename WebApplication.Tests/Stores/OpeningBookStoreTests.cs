using Microsoft.EntityFrameworkCore;
using Shared;
using WebApplication.Data.Models;
using WebApplication.Stores;
using WebApplication.Tests.Builders;

namespace WebApplication.Tests.Stores;

[TestFixture]
public class OpeningBookStoreTests : TestBase
{
    /// <summary>
    /// Tests <see cref="OpeningBookStore.GetOpeningBooksForUser" />.
    /// </summary>
    [Test]
    public void GetOpeningBooksForUser()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateUser("test-user")
                .Close()
            .CreateBook("test-book-1")
            .CreateBook("test-book-2", "test-user")
            .CreateBook("test-book-3")
            .CreateBook("test-book-4", "test-user")
        .Close();

        var store = new OpeningBookStore(Factory);
        var userId = Factory.CreateDbContext().Users.First().Id;
        var books =store.GetOpeningBooksForUser(userId);
        Assert.That(books, Is.Not.Empty);
        Assert.That(books, Has.Count.EqualTo(2));
        Assert.That(books[0].Name, Is.Not.EqualTo("test-book-1").Or.Not.EqualTo("test-book-3"));
        Assert.That(books[1].Name, Is.Not.EqualTo("test-book-1").Or.Not.EqualTo("test-book-3"));
    }
    
    /// <summary>
    /// Tests <see cref="OpeningBookStore.GetSharedOpeningBooks" />.
    /// </summary>
    [Test]
    public void GetSharedOpeningBooks()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateUser("test-user")
            .Close()
            .CreateBook("test-book-1")
            .CreateBook("test-book-2", "test-user")
            .CreateBook("test-book-3")
            .CreateBook("test-book-4", "test-user")
            .Close();
        
        var store = new OpeningBookStore(Factory);
        var userId = Factory.CreateDbContext().Users.First().Id;
        var books =store.GetOpeningBooksForUser(userId);
        Assert.That(books, Is.Not.Empty);
        Assert.That(books, Has.Count.EqualTo(2));
        Assert.That(books[0].Name, Is.Not.EqualTo("test-book-2").Or.Not.EqualTo("test-book-4"));
        Assert.That(books[1].Name, Is.Not.EqualTo("test-book-2").Or.Not.EqualTo("test-book-4"));
    }
    
    /// <summary>
    /// Tests <see cref="OpeningBookStore.Add(string, string, byte[], int, OpeningBookType)" />.
    /// </summary>
    [Test]
    public void Add()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateUser("test-user")
            .Close();
        
        var userId = Factory.CreateDbContext().Users.First().Id;
        var store = new OpeningBookStore(Factory);
        store.Add(userId,"test-book", [0x1, 0x2, 0x4], OpeningBookType.EPD);
        
        var openingBooks = Factory.CreateDbContext().OpeningBooks.Include(openingBook => openingBook.Data).ToArray();
        Assert.That(openingBooks, Has.Length.EqualTo(1));

        var book = openingBooks[0];
        Assert.That(book.Data.Data, Is.EqualTo(new byte [] {0x1, 0x2, 0x4}));
        Assert.That(book.Name, Is.EqualTo("test-book"));
        Assert.That(book.Type, Is.EqualTo(OpeningBookType.EPD));
    }
    
    /// <summary>
    /// Tests <see cref="OpeningBookStore.DeleteById" />.
    /// </summary>
    [Test]
    public void DeleteById()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test-book")
            .CreateBook("test-book-2")
            .Close();
        
        var store = new OpeningBookStore(Factory);
        var bookCount = Factory.CreateDbContext().OpeningBooks.Count();
        Assert.That(bookCount,  Is.EqualTo(2));
        
        var bookId = Factory.CreateDbContext().OpeningBooks.First( b => b.Name == "test-book").Id; 
        store.DeleteById(bookId);
        
        var books = Factory.CreateDbContext().OpeningBooks.ToList();
        Assert.That(books, Is.Not.Empty);
        Assert.That(books, Has.Count.EqualTo(1));
        Assert.That(books[0].Name, Is.EqualTo("test-book-2"));
    }
    
    /// <summary>
    /// Tests <see cref="OpeningBookStore.AnyTest" /> if opening book has at least one test. 
    /// </summary>
    [Test]
    public void AnyTest_Exists()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test-book")
            .CreateSprtSettings()
            .CreateUser("test-user")
            .AddEngine("stockfish")
            .AddBranch("base-branch")
            .AddBranch("test-branch")
            .AddTest("test-1", "test-book", "base-branch", "test-branch")
            .Close()
            .Close()
            .Close()
            .Close();
        
        var store = new OpeningBookStore(Factory);
        var result = store.AnyTest(Factory.CreateDbContext().OpeningBooks.First().Id);
        Assert.That(result, Is.EqualTo(true));
    }
    
    
    /// <summary>
    /// Tests <see cref="OpeningBookStore.AnyTest" /> if opening book has zero tests. 
    /// </summary>
    [Test]
    public void AnyTest_Not_Exists()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test-book");
        
        var store = new OpeningBookStore(Factory);
        var result = store.AnyTest(Factory.CreateDbContext().OpeningBooks.First().Id);
        Assert.That(result, Is.EqualTo(false));
    }

    /// <summary>
    /// Tests <see cref="OpeningBookStore.LoadContent"/>.
    /// </summary>
    [Test]
    public void LoadContent()
    {
        new DomainBuilder(Factory.CreateDbContext())
            .CreateBook("test-book", [0x1, 0x2, 0x4]);
        
        var store = new OpeningBookStore(Factory);
        var book = Factory.CreateDbContext().OpeningBooks.First();
        Assert.That(book.Data, Is.Null);
        var content = store.LoadContent(book.Id);
        
        Assert.That(store.LoadContent(book.Id), Is.Not.Null);
        Assert.That(content, Is.EqualTo(new byte[]{0x1, 0x2, 0x4}));
    }
}