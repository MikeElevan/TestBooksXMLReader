using System.Xml.Linq;
using TestBooksXMLReader;
using TestBooksXMLReader.Constants;
using TestBooksXMLReader.Models;

namespace BooksXMLReaderUnitTests
{
    public class BookXMLReaderTests
    {
        private BooksXMLReader _reader;

        public BookXMLReaderTests()
        {
            _reader = new BooksXMLReader();
        }

        #region GetReadOnlyListOfBooks Tests

        [Fact]
        public void GetReadOnlyListOfBooks_WhenNoBooks_ReturnsEmptyList()
        {
            // Arrange & Act
            var result = _reader.GetReadOnlyListOfBooks();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetReadOnlyListOfBooks_WhenBooksAdded_ReturnsAllBooks()
        {
            // Arrange
            var book1 = new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", Pages = 310 };
            var book2 = new Book { Title = "1984", Author = "George Orwell", Pages = 328 };

            _reader.AddBook(book1);
            _reader.AddBook(book2);

            // Act
            var result = _reader.GetReadOnlyListOfBooks();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(book1, result);
            Assert.Contains(book2, result);
        }

        [Fact]
        public void GetReadOnlyListOfBooks_IsImmutable()
        {
            var book = new Book { Title = "Test", Author = "A", Pages = 1 };
            _reader.AddBook(book);

            var result = _reader.GetReadOnlyListOfBooks();

            Assert.Throws<NotSupportedException>(() =>
            {
                ((ICollection<Book>)result).Add(new Book());
            });
        }

        [Fact]
        public void GetReadOnlyListOfBooks_ReturnsSnapshot()
        {
            var book1 = new Book { Title = "A", Author = "A", Pages = 1 };
            _reader.AddBook(book1);

            var snapshot = _reader.GetReadOnlyListOfBooks();

            _reader.AddBook(new Book { Title = "B", Author = "B", Pages = 2 });

            Assert.Single(snapshot);
            Assert.Equal("A", snapshot[0].Title);
        }

        #endregion

        #region AddBook Tests

        [Fact]
        public void AddBook_WithValidBook_AddsBookSuccessfully()
        {
            // Arrange
            var book = new Book { Title = "Test Book", Author = "Test Author", Pages = 250 };

            // Act
            _reader.AddBook(book);
            var result = _reader.GetReadOnlyListOfBooks();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Book", result[0].Title);
            Assert.Equal("Test Author", result[0].Author);
            Assert.Equal(250, result[0].Pages);
        }

        [Fact]
        public void AddBook_WithNullBook_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _reader.AddBook(null!));
        }

        [Fact]
        public void AddBook_MultipleBooks_AllAreStored()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Title = "Book 1", Author = "Author 1", Pages = 100 },
                new Book { Title = "Book 2", Author = "Author 2", Pages = 200 },
                new Book { Title = "Book 3", Author = "Author 3", Pages = 300 }
            };

            // Act
            foreach (var book in books)
            {
                _reader.AddBook(book);
            }

            var result = _reader.GetReadOnlyListOfBooks();

            // Assert
            Assert.Equal(3, result.Count);
        }

        #endregion

        #region Sort Tests

        [Fact]
        public void Sort_SortsByAuthorThenTitle()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Title = "Zebra Book", Author = "Arthur Miller", Pages = 100 },
                new Book { Title = "Apple Book", Author = "Arthur Miller", Pages = 150 },
                new Book { Title = "Book Z", Author = "Zack Brown", Pages = 200 }
            };

            foreach (var book in books)
            {
                _reader.AddBook(book);
            }

            // Act
            _reader.Sort();
            var result = _reader.GetReadOnlyListOfBooks();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Arthur Miller", result[0].Author);
            Assert.Equal("Apple Book", result[0].Title);
            Assert.Equal("Arthur Miller", result[1].Author);
            Assert.Equal("Zebra Book", result[1].Title);
            Assert.Equal("Zack Brown", result[2].Author);
        }

        [Fact]
        public void Sort_EmptyList_DoesNotThrow()
        {
            // Act & Assert
            _reader.Sort();
            Assert.Empty(_reader.GetReadOnlyListOfBooks());
        }

        [Fact]
        public void Sort_SingleBook_RemainsUnchanged()
        {
            // Arrange
            var book = new Book { Title = "Only Book", Author = "Only Author", Pages = 100 };
            _reader.AddBook(book);

            // Act
            _reader.Sort();
            var result = _reader.GetReadOnlyListOfBooks();

            // Assert
            Assert.Single(result);
            Assert.Equal("Only Book", result[0].Title);
        }

        #endregion

        #region SearchBookByTitle Tests

        [Fact]
        public void SearchBookByTitle_WithExactMatch_ReturnsBook()
        {
            // Arrange
            var book = new Book { Title = "The Hobbit", Author = "Tolkien", Pages = 310 };
            _reader.AddBook(book);

            // Act
            var result = _reader.SearchBookByTitle("The Hobbit");

            // Assert
            Assert.Single(result);
            Assert.Equal("The Hobbit", result.First().Title);
        }

        [Fact]
        public void SearchBookByTitle_WithPartialMatch_ReturnsMatchingBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Title = "Harry Potter and the Philosopher's Stone", Author = "J.K. Rowling", Pages = 309 },
                new Book { Title = "Harry Potter and the Chamber of Secrets", Author = "J.K. Rowling", Pages = 251 },
                new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", Pages = 310 }
            };

            foreach (var book in books)
            {
                _reader.AddBook(book);
            }

            // Act
            var result = _reader.SearchBookByTitle("Harry");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, b => Assert.Contains("Harry", b.Title));
        }

        [Fact]
        public void SearchBookByTitle_WithNoMatch_ReturnsEmptyList()
        {
            // Arrange
            _reader.AddBook(new Book { Title = "The Hobbit", Author = "Tolkien", Pages = 310 });

            // Act
            var result = _reader.SearchBookByTitle("NonExistent");

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void SearchBookByTitle_WithEmptyOrWhitespaceInput_ThrowsArgumentException(string input)
        {
            // Arrange
            _reader.AddBook(new Book { Title = "The Hobbit", Author = "Tolkien", Pages = 310 });

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _reader.SearchBookByTitle(input));
        }

        [Fact]
        public void SearchBookByTitle_CaseInsensitive_ReturnsMatches()
        {
            // Arrange
            var book = new Book { Title = "The Hobbit", Author = "Tolkien", Pages = 310 };
            _reader.AddBook(book);

            // Act
            var result = _reader.SearchBookByTitle("the hobbit");

            // Assert
            Assert.Single(result);
        }
        #endregion

        #region XML File Operations Tests

        [Fact]
        public async Task LoadFromXMLAsync_WithValidFile_LoadsBooksSuccessfully()
        {
            // Arrange
            var filePath = Path.Combine(Path.GetTempPath(), "test_books.xml");
            CreateSampleXmlFile(filePath);

            try
            {
                // Act
                await _reader.LoadFromXMLAsync(filePath);
                var result = _reader.GetReadOnlyListOfBooks();

                // Assert
                Assert.Equal(2, result.Count);
                Assert.Equal("The Hobbit", result[0].Title);
                Assert.Equal("J.R.R. Tolkien", result[0].Author);
                Assert.Equal(310, result[0].Pages);
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        [Fact]
        public async Task LoadFromXMLAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var filePath = Path.Combine(Path.GetTempPath(), "nonexistent_books.xml");

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => _reader.LoadFromXMLAsync(filePath));
        }

        [Fact]
        public async Task LoadFromXMLAsync_WithMissingTitle_ThrowsInvalidDataException()
        {
            // Arrange
            var filePath = Path.Combine(Path.GetTempPath(), "invalid_books.xml");
            var doc = new XDocument(
                new XElement(BooksConstants.RootNodeName,
                    new XElement(BooksConstants.ChildNodeName,
                        new XElement(BooksConstants.NodeAuthorName, "J.R.R. Tolkien"),
                        new XElement(BooksConstants.NodePagesName, "310")
                    )
                )
            );
            doc.Save(filePath);

            try
            {
                // Act & Assert
                await Assert.ThrowsAsync<InvalidDataException>(() => _reader.LoadFromXMLAsync(filePath));
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        [Fact]
        public async Task LoadFromXMLAsync_WithMissingAuthor_ThrowsInvalidDataException()
        {
            // Arrange
            var filePath = Path.Combine(Path.GetTempPath(), "invalid_author_books.xml");
            var doc = new XDocument(
                new XElement(BooksConstants.RootNodeName,
                    new XElement(BooksConstants.ChildNodeName,
                        new XElement(BooksConstants.NodeTitleName, "The Hobbit"),
                        new XElement(BooksConstants.NodePagesName, "310")
                    )
                )
            );
            doc.Save(filePath);

            try
            {
                // Act & Assert
                await Assert.ThrowsAsync<InvalidDataException>(() => _reader.LoadFromXMLAsync(filePath));
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        [Fact]
        public async Task SaveBooksToXMLFileAsync_SavesBooksCorrectly()
        {
            // Arrange
            var filePath = Path.Combine(Path.GetTempPath(), "saved_books.xml");
            var books = new List<Book>
            {
                new Book { Title = "Book 1", Author = "Author 1", Pages = 100 },
                new Book { Title = "Book 2", Author = "Author 2", Pages = 200 }
            };

            foreach (var book in books)
            {
                _reader.AddBook(book);
            }

            try
            {
                // Act
                await _reader.SaveBooksToXMLFileAsync(filePath);

                // Assert
                Assert.True(File.Exists(filePath));
                var doc = XDocument.Load(filePath);
                var bookElements = doc.Root?.Elements(BooksConstants.ChildNodeName).ToList();
                Assert.NotNull(bookElements);
                Assert.Equal(2, bookElements.Count);
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        [Fact]
        public async Task LoadFromXMLAsync_ClearsExistingBooksBeforeLoading()
        {
            // Arrange
            _reader.AddBook(new Book { Title = "Old Book", Author = "Old Author", Pages = 50 });

            var filePath = Path.Combine(Path.GetTempPath(), "test_books.xml");
            CreateSampleXmlFile(filePath);

            try
            {
                // Act
                await _reader.LoadFromXMLAsync(filePath);
                var result = _reader.GetReadOnlyListOfBooks();

                // Assert
                Assert.Equal(2, result.Count);
                Assert.DoesNotContain(result, b => b.Title == "Old Book");
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        [Fact]
        public async Task RoundTrip_SaveAndLoad_PreservesData()
        {
            // Arrange
            var filePath = Path.Combine(Path.GetTempPath(), "roundtrip_books.xml");
            var originalBooks = new List<Book>
            {
                new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", Pages = 310 },
                new Book { Title = "1984", Author = "George Orwell", Pages = 328 }
            };

            foreach (var book in originalBooks)
            {
                _reader.AddBook(book);
            }

            try
            {
                // Act
                await _reader.SaveBooksToXMLFileAsync(filePath);

                var newService = new BooksXMLReader();
                await newService.LoadFromXMLAsync(filePath);
                var loadedBooks = newService.GetReadOnlyListOfBooks();

                // Assert
                Assert.Equal(2, loadedBooks.Count);
                for (int i = 0; i < originalBooks.Count; i++)
                {
                    Assert.Equal(originalBooks[i].Title, loadedBooks[i].Title);
                    Assert.Equal(originalBooks[i].Author, loadedBooks[i].Author);
                    Assert.Equal(originalBooks[i].Pages, loadedBooks[i].Pages);
                }
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        [Fact]
        public async Task SaveBooksToXMLFileAsync_WithEmptyBookList_CreatesValidXml()
        {
            // Arrange
            var filePath = Path.Combine(Path.GetTempPath(), "empty_books.xml");

            try
            {
                // Act
                await _reader.SaveBooksToXMLFileAsync(filePath);

                // Assert
                Assert.True(File.Exists(filePath));
                var doc = XDocument.Load(filePath);
                var bookElements = doc.Root?.Elements(BooksConstants.ChildNodeName).ToList();
                Assert.NotNull(bookElements);
                Assert.Empty(bookElements);
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        public async Task AddBook_WithMultipleThreads_AllBooksAreAdded()
        {
            // Arrange
            int bookCount = 100;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < bookCount; i++)
            {
                int index = i;
                tasks.Add(Task.Run(() =>
                {
                    var book = new Book { Title = $"Book {index}", Author = $"Author {index}", Pages = 100 + index };
                    _reader.AddBook(book);
                }));
            }

            await Task.WhenAll(tasks);
            var result = _reader.GetReadOnlyListOfBooks();

            // Assert
            Assert.Equal(bookCount, result.Count);
        }
        #endregion

        #region Helper Methods

        private void CreateSampleXmlFile(string filePath)
        {
            var doc = new XDocument(
                new XElement(BooksConstants.RootNodeName,
                    new XElement(BooksConstants.ChildNodeName,
                        new XElement(BooksConstants.NodeTitleName, "The Hobbit"),
                        new XElement(BooksConstants.NodeAuthorName, "J.R.R. Tolkien"),
                        new XElement(BooksConstants.NodePagesName, "310")
                    ),
                    new XElement(BooksConstants.ChildNodeName,
                        new XElement(BooksConstants.NodeTitleName, "1984"),
                        new XElement(BooksConstants.NodeAuthorName, "George Orwell"),
                        new XElement(BooksConstants.NodePagesName, "328")
                    )
                )
            );
            doc.Save(filePath);
        }

        #endregion
    }

    /// <summary>
    /// Custom equality comparer for Book objects to support testing.
    /// </summary>
    public class BookEqualityComparer : IEqualityComparer<Book>
    {
        public bool Equals(Book? x, Book? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return x.Title == y.Title &&
                   x.Author == y.Author &&
                   x.Pages == y.Pages;
        }

        public int GetHashCode(Book obj)
        {
            unchecked
            {
                int hashCode = obj.Title?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (obj.Author?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ obj.Pages.GetHashCode();
                return hashCode;
            }
        }
    }
}