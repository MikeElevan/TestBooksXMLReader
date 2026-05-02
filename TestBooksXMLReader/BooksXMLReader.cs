using System.Collections.Immutable;
using System.Xml.Linq;
using TestBooksXMLReader.Constants;
using TestBooksXMLReader.Models;

namespace TestBooksXMLReader
{
    public class BooksXMLReader
    {
        private const int BufferSize = 4096;
        private ImmutableList<Book> _books = ImmutableList<Book>.Empty;

        public IReadOnlyList<Book> GetReadOnlyListOfBooks() => _books;

        public async Task LoadFromXMLAsync(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            ImmutableList<Book> loadedBooks;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true))
            {
                XDocument xml = await XDocument.LoadAsync(stream, LoadOptions.None, default);

                loadedBooks = xml.Root?.Elements(BooksConstants.ChildNodeName).Select(b =>
                new Book { 
                    Author = b.Element(BooksConstants.NodeAuthorName)?.Value ?? throw new InvalidDataException(BooksConstants.BookMessageAuthorException), 
                    Title = b.Element(BooksConstants.NodeTitleName)?.Value ?? throw new InvalidDataException(BooksConstants.BookMessageTitleException), 
                    Pages = int.TryParse(b.Element(BooksConstants.NodePagesName)?.Value, out int p) ? p : throw new InvalidDataException(BooksConstants.BookMessageTitleException)
                }).ToImmutableList() ?? ImmutableList<Book>.Empty;
            }

            Interlocked.Exchange(ref _books, loadedBooks);
        }

        public async Task SaveBooksToXMLFileAsync(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            ImmutableList<Book> snapshoot = _books;
            
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true))
            {
                XDocument xml = new XDocument(
                    new XElement(BooksConstants.RootNodeName,
                        snapshoot.Select(b =>
                            new XElement(BooksConstants.ChildNodeName,
                                new XElement(BooksConstants.NodeAuthorName, b.Author),
                                new XElement(BooksConstants.NodeTitleName, b.Title),
                                new XElement(BooksConstants.NodePagesName, b.Pages)
                            )
                        )
                    )
                );

                await xml.SaveAsync(stream, SaveOptions.None, default);
            }
        }

        public void AddBook(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            ImmutableInterlocked.Update(ref _books, list => list.Add(book));
        }

        public void Sort()
        {
            ImmutableList<Book> snapshoot = _books;

            if (snapshoot.Count == 0)
            {
                return;
            }

            ImmutableInterlocked.Update(ref _books, books => books.Sort((x, y) =>
            {
                int res = string.Compare(x.Author, y.Author, StringComparison.OrdinalIgnoreCase);
                return res != 0 ? res : string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase);
            }));
        }

        public List<Book> SearchBookByTitle(string title)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            ImmutableList<Book> snapshoot = _books;

            return snapshoot.
                Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).
                ToList();
        }
    }
}
